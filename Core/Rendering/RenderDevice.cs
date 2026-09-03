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

        /// <summary>
        /// Value noise lattice the cloud shader samples
        /// </summary>
        private const int CloudNoiseTextureSlot = 9;

        /// <summary>
        /// Width and height of the cloud noise texture in texels. Has to be a power of two, the
        /// slice offset below wraps with a mask, and has to match CLOUD_NOISE_SIZE in Options.hlsl.
        /// </summary>
        private const int CloudNoiseSize = 256;

        private Resolution renderResolution;

        private RenderTargetView backbufferView;

        private SampleDescription antiAliasing; // Used for backbuffer and depth buffer
        private RasterizerState rasterState;
        private BlendState blendState;
        private DepthStencilState depthState;
        private SamplerState samplerState;
        private SamplerState wrapSamplerState;

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
        private TextureBuffer<Color> cloudNoiseTextureBuffer;

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

            cloudNoiseTextureBuffer = new TextureBuffer<Color>(device, CreateCloudNoise(CloudNoiseSize),
                CloudNoiseSize, Format.R8G8B8A8_UNorm, CloudNoiseTextureSlot);
        }

        /// <summary>
        /// Creates the noise texture the shader dithers ambient occlusion with.
        /// This is fractal value noise, which is low frequency, so it blotches at large scales.
        /// TODO generate real blue noise (void-and-cluster) instead.
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

        /// <summary>
        /// Builds the lattice the cloud shader reads its 3D value noise out of.
        ///
        /// A 3D lattice would want a volume texture and eight fetches per cell. Laying the slices
        /// out across a 2D texture instead, each one offset from the last by (37, 239) texels,
        /// turns that into two bilinear fetches: red is the slice below the sample and green the
        /// slice above, and the shader interpolates between them. The offsets are the same on
        /// both sides, see cloudNoise in Sky.hlsl.
        ///
        /// The values are white noise. The bilinear filter is what makes them value noise, and
        /// the octaves in cloudFbm are what make that look like cloud.
        /// </summary>
        /// <param name="size">Width and height in texels, a power of two</param>
        /// <returns>Texels in row major order, size squared of them</returns>
        private Color[] CreateCloudNoise(int size)
        {
            const int sliceOffsetX = 37;
            const int sliceOffsetY = 239;

            byte[] lattice = new byte[size * size];

            // Fixed seed, so the clouds are the same shape every run
            Random random = new Random(1337);
            random.NextBytes(lattice);

            int mask = size - 1;
            Color[] noiseData = new Color[size * size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    byte slice = lattice[y * size + x];
                    byte nextSlice = lattice[((y + sliceOffsetY) & mask) * size + ((x + sliceOffsetX) & mask)];

                    noiseData[y * size + x] = new Color(slice, nextSlice, (byte) 0, (byte) 255);
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

            Utilities.Dispose(ref wrapSamplerState);

            SamplerStateDescription wrapDescription = SamplerStateDescription.Default();
            wrapDescription.AddressU = TextureAddressMode.Wrap;
            wrapDescription.AddressV = TextureAddressMode.Wrap;
            wrapDescription.AddressW = TextureAddressMode.Wrap;

            wrapSamplerState = new SamplerState(device, wrapDescription);
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

            deviceContext.PixelShader.SetSampler(1, wrapSamplerState);
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

                // Slot order has to match the register indices in Common.hlsl
                UploadPrimitives<Sphere>(0);
                UploadPrimitives<Box>(1);
                UploadPrimitives<Primitives.Plane>(2);
                UploadPrimitives<Torus>(3);
                UploadPrimitives<Octahedron>(4);
                UploadPrimitives<Ellipsoid>(5);
                UploadPrimitives<Cylinder>(6);
            }

            // Draw raymarch plane
            raymarchRenderPlane.Draw();

            // Draw rendered scene to screen
            swapChain.Present(1, PresentFlags.None);
        }

        /// <summary>
        /// Uploads every renderer of one primitive type in the current scene to its buffer slot
        /// </summary>
        private void UploadPrimitives<T>(int slot) where T : IPrimitive
        {
            primitivesBuffer[slot].UpdateValue(
                Scene.CurrentScene.Components<RaymarchRenderer<T>>()
                    .Select(primitive => primitive.GetBufferData()).ToArray()
            );
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
            cloudNoiseTextureBuffer?.Dispose();
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
            Utilities.Dispose(ref wrapSamplerState);

            swapChain.Dispose();
            deviceContext.Dispose();
            device.Dispose();
        }
    }
}
