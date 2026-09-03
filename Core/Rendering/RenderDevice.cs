// Created by Sakri Koskimies (Github: Saggre) on 05/11/2019

using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using RaymarchEngine.Core.Buffers;
using RaymarchEngine.Core.Primitives;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Windows;
using Device = SharpDX.Direct3D11.Device;
using Color = SharpDX.Color;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;
using Vector4 = System.Numerics.Vector4;

namespace RaymarchEngine.Core.Rendering
{
    /// <summary>
    /// A class that handles rendering the visible
    /// </summary>
    public class RenderDevice : IDisposable
    {
        /// <summary>
        /// Slots 0..7 belong to the per-primitive structured buffers. Anything bound inside that
        /// range silently replaces whichever primitive buffer shares the slot.
        /// </summary>
        private const int NoiseTextureSlot = 8;

        private Resolution renderResolution;

        private RenderTargetView backbufferView;

        private SampleDescription antiAliasing; // Used for backbuffer and depth buffer
        private RasterizerState rasterState;
        private BlendState blendState;
        private DepthStencilState depthState;
        private SamplerState samplerState;

        private RenderForm renderForm;

        public Device device;
        public DeviceContext deviceContext;
        private SwapChain swapChain;

        // Raymarch
        private Mesh raymarchRenderPlane; // Plane to render raymarch shader on
        private Shader raymarchShader;
        private RaymarchShaderBufferData raymarchShaderBufferData; // Values to send to the raymarch shader
        private ConstantBuffer<RaymarchShaderBufferData> raymarchShaderBuffer;
        private StructuredBuffer<PrimitiveBufferData>[] primitivesBuffer;
        private TextureBuffer<Color> noiseTextureBuffer;

        [StructLayout(LayoutKind.Sequential)]
        struct RaymarchShaderBufferData
        {
            public Vector3 cameraPosition;
            public float aspectRatio;
            public Vector3 cameraDirection;
            public float time;
            public Vector4 additionalData;
        }

        /// <summary>
        /// Set up the device for rendering.
        /// D3DDevice is actually only properly set up when first frame has started rendering, and NOT when this class is created.
        /// RenderDeviceStarted() is called when it is set up.
        /// </summary>
        /// <param name="renderForm">SharpDX RenderForm to render in</param>
        /// <param name="renderResolution">Resolution at which render</param>
        public RenderDevice(RenderForm renderForm, Resolution renderResolution)
        {
            this.renderResolution = renderResolution;
            this.renderForm = renderForm;
            InitializeDeviceResources();
        }

        private bool renderDeviceStarted;

        /// <summary>
        /// Called when the first frame rendering is started, and D3DDevice is properly started and functional
        /// </summary>
        private void RenderDeviceStarted()
        {
            // TODO pre-compile shader
            raymarchShader = Shader.CompileFromFiles(@"Shaders\Raymarch");
            raymarchRenderPlane = Mesh.CreateQuad();

            // Set as current shaders
            // TODO what's inputlayout?
            deviceContext.InputAssembler.InputLayout = raymarchShader.InputLayout;
            deviceContext.VertexShader.Set(raymarchShader.VertexShader);
            deviceContext.PixelShader.Set(raymarchShader.PixelShader);

            raymarchShaderBuffer = new ConstantBuffer<RaymarchShaderBufferData>(device);

            // Create buffers for different types of shapes. Combine later?
            primitivesBuffer = new StructuredBuffer<PrimitiveBufferData>[8];
            for (int i = 0; i < primitivesBuffer.Length; i++)
            {
                primitivesBuffer[i] = new StructuredBuffer<PrimitiveBufferData>(device, i);
            }

            noiseTextureBuffer = new TextureBuffer<Color>(device, CreateNoise(1024), 1024,
                Format.R8G8B8A8_UNorm, NoiseTextureSlot);
        }

        /// <summary>
        /// Creates noise to use in the shader
        /// </summary>
        /// <param name="size"></param>
        private Color[] CreateNoise(int size)
        {
            FastNoise fastNoise = new FastNoise();
            fastNoise.SetNoiseType(FastNoise.NoiseType.CubicFractal);
            fastNoise.SetFractalOctaves(8);
            fastNoise.SetFractalType(FastNoise.FractalType.Billow);
            fastNoise.SetFrequency(0.01f);

            Color[] noiseData = new Color[size * size];

            int i = 0;
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    noiseData[i] = new Color((fastNoise.GetNoise(x * 80, y * 80) + 1.0f) / 2.0f);
                    noiseData[i] = Color.Lerp(noiseData[i], Color.White, 0.5f);
                    i++;
                }
            }

            return noiseData;
        }

        #region Setup

        /// <summary>
        /// Initialize resources to be used in rendering, such as back buffer and swap chain
        /// </summary>
        private void InitializeDeviceResources()
        {
            ModeDescription backBufferDesc = new ModeDescription()
            {
                Width = renderResolution.Width,
                Height = renderResolution.Height,
                RefreshRate = new Rational(0, 1),
                Scaling = DisplayModeScaling.Stretched,
                Format = Format.R8G8B8A8_UNorm,
            };

            // One fullscreen quad has no interior edges for MSAA to resolve, so it only costs bandwidth
            antiAliasing = new SampleDescription(1, 0);

            SwapChainDescription swapChainDesc = new SwapChainDescription()
            {
                ModeDescription = backBufferDesc,
                SampleDescription = antiAliasing,
                Usage = Usage.RenderTargetOutput, // Means it's rendered directly to screen
                BufferCount = 1,
                OutputHandle =
                    renderForm
                        .Handle, // Output window TODO check if this can be any other window than SharpDX renderForm
                IsWindowed = true, // Windowed even if fullscreen
                SwapEffect = SwapEffect.Discard,
                Flags = SwapChainFlags.AllowModeSwitch, // Allows other fullscreen resolutions than native one.
            };

            // TODO support more levels?
            FeatureLevel[] levels =
            {
                FeatureLevel.Level_11_1,
                FeatureLevel.Level_11_0,
            };

            Device.CreateWithSwapChain(
                DriverType.Hardware,
                DeviceCreationFlags.DisableGpuTimeout,
                levels,
                swapChainDesc,
                out device,
                out swapChain
            );

            deviceContext = device.ImmediateContext;

            // TODO what is this vvvvvvv

            //Ignore all windows events
            //var factory = swapChain.GetParent<Factory>();
            //factory.MakeWindowAssociation(renderForm.Handle, WindowAssociationFlags.IgnoreAll);

            //Setup handler on resize form
            //renderForm.UserResized += (sender, args) => MustResize = true;

            SetRasterState();
            SetBlendState();
            SetDepthState();
            SetSamplerState();

            ApplyStates();

            //Resize all items
            Resize();
        }

        /// <summary>
        /// Whether to render solid or wireframe
        /// </summary>
        void SetRasterState(bool isWireframe = false)
        {
            // Dispose of old variable
            Utilities.Dispose(ref rasterState);

            RasterizerStateDescription description = RasterizerStateDescription.Default();
            description.FillMode = isWireframe ? FillMode.Wireframe : FillMode.Solid;
            description.IsMultisampleEnabled = false;

            rasterState = new RasterizerState(device, description);
        }

        /// <summary>
        /// Set the colour blending state. Disabled: the raymarch quad is opaque and covers the target.
        /// </summary>
        void SetBlendState()
        {
            Utilities.Dispose(ref blendState);

            BlendStateDescription description = BlendStateDescription.Default();
            description.RenderTarget[0].IsBlendEnabled = false;

            blendState = new BlendState(device, description);
        }

        /// <summary>
        /// Set depth state. Off, with no depth buffer: a single fullscreen quad occludes nothing.
        /// </summary>
        void SetDepthState()
        {
            Utilities.Dispose(ref depthState);

            DepthStencilStateDescription description = DepthStencilStateDescription.Default();
            description.IsDepthEnabled = false;

            depthState = new DepthStencilState(device, description);
        }

        /// <summary>
        /// Set texture sampling
        /// </summary>
        void SetSamplerState()
        {
            Utilities.Dispose(ref samplerState);

            SamplerStateDescription description = SamplerStateDescription.Default();
            /*description.Filter = Filter.Anisotropic;
            description.AddressU = TextureAddressMode.Clamp;
            description.AddressV = TextureAddressMode.Clamp;
            description.AddressW = TextureAddressMode.Clamp;
            description.BorderColor = new Color4(0, 0, 0, 1);
            description.ComparisonFunction = Comparison.Never;
            description.MipLodBias = 0;
            description.MinimumLod = -float.MaxValue;
            description.MaximumLod = float.MaxValue;*/

            samplerState = new SamplerState(device, description);
        }

        /// <summary>
        /// Applies states that have been saved as variables to the device context
        /// </summary>
        void ApplyStates()
        {
            deviceContext.Rasterizer.State = rasterState;
            deviceContext.OutputMerger.SetBlendState(blendState);
            deviceContext.OutputMerger.SetDepthStencilState(depthState);

            deviceContext.PixelShader.SetSampler(0, samplerState);
            deviceContext.DomainShader.SetSampler(0, samplerState);
        }

        #endregion

        #region Draw

        /// <summary>
        /// Main drawing method to be executed per-frame
        /// Draw vertices and call the callback between setting object-specific shaders and object-specific buffers.
        /// </summary>
        public void Draw()
        {
            if (!renderDeviceStarted)
            {
                renderDeviceStarted = true;
                RenderDeviceStarted();
            }

            // Clear with a color
            Clear(Color.Black);

            // Set raymarch shader buffer data
            {
                raymarchShaderBufferData.cameraPosition = Scene.CurrentScene.ActiveCamera.Movement.Position;
                raymarchShaderBufferData.cameraDirection = Scene.CurrentScene.ActiveCamera.Movement.Forward;
                // The window's ratio, not the render target's: uv comes from TexCoord, so the
                // correction has to match the area the back buffer is stretched onto, not its size.
                raymarchShaderBufferData.aspectRatio = Engine.AspectRatio();
                raymarchShaderBufferData.time = Engine.ElapsedTime; // TODO reset time when it is too large

                raymarchShaderBuffer.UpdateValue(raymarchShaderBufferData);

                primitivesBuffer[0].UpdateValue(
                    Scene.CurrentScene.Components<RaymarchRenderer<Sphere>>()
                        .Select(primitive => primitive.GetBufferData()).ToArray()
                );

                primitivesBuffer[1].UpdateValue(
                    Scene.CurrentScene.Components<RaymarchRenderer<Box>>()
                        .Select(primitive => primitive.GetBufferData()).ToArray()
                );

                primitivesBuffer[2].UpdateValue(
                    Scene.CurrentScene.Components<RaymarchRenderer<Primitives.Plane>>()
                        .Select(primitive => primitive.GetBufferData()).ToArray()
                );

                /*primitivesBuffer[3].UpdateValue(
                    Engine.CurrentScene.GroupedPrimitives.GetPrimitivesOfType<Ellipsoid>()
                        .Select(primitive => primitive.GetBufferData()).ToArray()
                );

                primitivesBuffer[4].UpdateValue(
                    Engine.CurrentScene.GroupedPrimitives.GetPrimitivesOfType<Torus>()
                        .Select(primitive => primitive.GetBufferData()).ToArray()
                );

                primitivesBuffer[5].UpdateValue(
                    Engine.CurrentScene.GroupedPrimitives.GetPrimitivesOfType<CappedTorus>()
                        .Select(primitive => primitive.GetBufferData()).ToArray()
                );*/
            }

            // Draw raymarch plane
            raymarchRenderPlane.Draw();

            // Draw rendered scene to screen
            swapChain.Present(1, PresentFlags.None);
        }

        /// <summary>
        /// Clears backbuffer and depth buffer
        /// </summary>
        /// <param name="color">Background color</param>
        void Clear(Color4 color)
        {
            deviceContext.ClearRenderTargetView(backbufferView, color);
        }

        #endregion


        #region Resize

        /// <summary>
        /// Must be called on window resize.
        /// Resizes all buffers.
        /// </summary>
        void Resize()
        {
            // Dispose all previous allocated resources
            Utilities.Dispose(ref backbufferView);

            // Error check
            if (renderForm.ClientSize.Width == 0 || renderForm.ClientSize.Height == 0)
            {
                return;
            }

            // Resize the backbuffer 
            swapChain.ResizeBuffers(
                1,
                renderResolution.Width,
                renderResolution.Height,
                Format.R8G8B8A8_UNorm,
                SwapChainFlags.None
            );

            // Get the actual backbuffer texture from swapchain
            Texture2D backBufferTexture = swapChain.GetBackBuffer<Texture2D>(0);

            // Create new render target for backbuffer
            backbufferView = new RenderTargetView(device, backBufferTexture);
            backBufferTexture.Dispose();

            // Setup targets and viewport for rendering
            deviceContext.Rasterizer.SetViewport(0, 0, renderResolution.Width, renderResolution.Height);
            deviceContext.OutputMerger.SetTargets(backbufferView);
        }

        #endregion

        /// <summary>
        /// Called on program close.
        /// Disposes of variables taking up resources.
        /// </summary>
        public void Dispose()
        {
            // Resources before the context and device that own them. Engine owns the render form.
            // These are still null if the program closes before the first frame.
            raymarchShaderBuffer?.Dispose();
            noiseTextureBuffer?.Dispose();
            raymarchRenderPlane?.Dispose();
            raymarchShader?.Dispose();

            if (primitivesBuffer != null)
            {
                foreach (StructuredBuffer<PrimitiveBufferData> buffer in primitivesBuffer)
                {
                    buffer.Dispose();
                }
            }

            Utilities.Dispose(ref backbufferView);
            Utilities.Dispose(ref rasterState);
            Utilities.Dispose(ref blendState);
            Utilities.Dispose(ref depthState);
            Utilities.Dispose(ref samplerState);

            swapChain.Dispose();
            deviceContext.Dispose();
            device.Dispose();
        }
    }
}
