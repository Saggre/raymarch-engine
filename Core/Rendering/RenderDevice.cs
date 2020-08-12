// Created by Sakri Koskimies (Github: Saggre) on 05/11/2019

using System;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
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
        private RenderTargetView backbufferView;
        private DepthStencilView depthView;

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
        private RaymarchShaderBufferData raymarchShaderBufferData; // Values to send to the raymarch shader
        private ConstantBuffer<RaymarchShaderBufferData> raymarchShaderBuffer;
        private StructuredBuffer<RaymarchGameObjectBufferData> raymarchObjectsBuffer;

        [StructLayout(LayoutKind.Sequential)]
        struct RaymarchShaderBufferData
        {
            /// <summary>
            /// Camera position, rotation, scale
            /// </summary>
            public Matrix4x4 viewMatrix;

            public Vector3 cameraPosition;
            public float aspectRatio;
            public float time;
            public float objectCount;
            public Vector2 blank;
            public Vector4 blank1;
            public Vector4 blank2;
        }

        /// <summary>
        /// Set up the device for rendering.
        /// D3DDevice is actually only properly set up when first frame has started rendering, and NOT when this class is created.
        /// RenderDeviceStarted() is called when it is set up.
        /// </summary>
        /// <param name="renderForm">SharpDX RenderForm to render in</param>
        public RenderDevice(RenderForm renderForm)
        {
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
            Shader raymarchShader = Shader.CompileFromFiles(@"Shaders\Raymarch");
            raymarchRenderPlane = Mesh.CreateQuad();

            // Set as current shaders
            // TODO what's inputlayout?
            deviceContext.InputAssembler.InputLayout = raymarchShader.InputLayout;
            deviceContext.VertexShader.Set(raymarchShader.VertexShader);
            deviceContext.PixelShader.Set(raymarchShader.PixelShader);

            raymarchShaderBuffer = new ConstantBuffer<RaymarchShaderBufferData>(device);
            raymarchObjectsBuffer = new StructuredBuffer<RaymarchGameObjectBufferData>(device);
        }

        #region Setup

        /// <summary>
        /// Initialize resources to be used in rendering, such as back buffer and swap chain
        /// </summary>
        private void InitializeDeviceResources()
        {
            // Does this shit even work?
            Size resolution = new Size(800, 600);

            ModeDescription backBufferDesc = new ModeDescription()
            {
                Width = resolution.Width,
                Height = resolution.Height,
                RefreshRate = new Rational(Engine.Fps, 1), // TODO set numerator to 0 to disable vsync
                Scaling = DisplayModeScaling.Stretched,
                Format = Format.R8G8B8A8_UNorm,
            };

            // Why does count>1 not render anything?
            antiAliasing = new SampleDescription(2, 0);

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
            SetAlphaBlending();
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
            description.IsMultisampleEnabled = true;

            rasterState = new RasterizerState(device, description);
        }

        /// <summary>
        /// Set alpha blending as current color blending state between previous and current pixels. Blending occurs after pixel shader stage
        /// </summary>
        void SetAlphaBlending()
        {
            Utilities.Dispose(ref blendState);

            BlendStateDescription description = BlendStateDescription.Default();
            description.RenderTarget[0].BlendOperation = BlendOperation.Add;
            description.RenderTarget[0].SourceBlend = BlendOption.SourceAlpha;
            description.RenderTarget[0].DestinationBlend = BlendOption.InverseDestinationAlpha;
            description.RenderTarget[0].IsBlendEnabled = true;

            blendState = new BlendState(device, description);
        }

        /// <summary>
        /// Set depth state
        /// </summary>
        void SetDepthState()
        {
            Utilities.Dispose(ref depthState);

            DepthStencilStateDescription description = DepthStencilStateDescription.Default();
            description.DepthComparison = Comparison.LessEqual;
            description.IsDepthEnabled = true;

            depthState = new DepthStencilState(device, description);
        }

        /// <summary>
        /// Set texture sampling
        /// </summary>
        void SetSamplerState()
        {
            Utilities.Dispose(ref samplerState);

            SamplerStateDescription description = SamplerStateDescription.Default();
            description.Filter = Filter.Anisotropic;
            description.AddressU = TextureAddressMode.Clamp;
            description.AddressV = TextureAddressMode.Clamp;
            description.AddressW = TextureAddressMode.Clamp;
            description.BorderColor = new Color4(0, 0, 0, 1);
            description.ComparisonFunction = Comparison.Never;
            description.MipLodBias = 0;
            description.MinimumLod = -float.MaxValue;
            description.MaximumLod = float.MaxValue;

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

            // TODO only slot 0?
            deviceContext.PixelShader.SetSampler(0, samplerState);
            deviceContext.DomainShader.SetSampler(0, samplerState);
            // TODO set sampler state to other types of shaders as well

            // TODO make states changeable
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
                // These matrices are not per-object
                raymarchShaderBufferData.viewMatrix = Engine.CurrentScene.ActiveCamera.ViewMatrix();
                raymarchShaderBufferData.cameraPosition = Engine.CurrentScene.ActiveCamera.Position;
                raymarchShaderBufferData.aspectRatio = Engine.AspectRatio();
                raymarchShaderBufferData.time = Engine.ElapsedTime; // TODO reset time when it is too large
                raymarchShaderBufferData.objectCount = Engine.CurrentScene.GameObjects.Count() * 1.0f;

                raymarchShaderBuffer.UpdateValue(raymarchShaderBufferData);

                raymarchObjectsBuffer.UpdateValue(Engine.CurrentScene.GameObjects
                    .Select(gameObject => gameObject.GetBufferData()).ToArray());
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
            deviceContext.ClearDepthStencilView(depthView, DepthStencilClearFlags.Depth, 1.0F, 0);
        }

        #endregion


        #region Resize

        /// <summary>
        /// Must be called on window resize.
        /// Resizes all buffers.
        /// </summary>
        void Resize()
        {
            // TODO render resolution is actually set here, because back buffer initiator calls Resize()
            int width = 1280; //renderForm.ClientSize.Width;
            int height = 720; //renderForm.ClientSize.Height;

            // Dispose all previous allocated resources
            Utilities.Dispose(ref backbufferView);
            Utilities.Dispose(ref depthView);

            // Error check
            if (renderForm.ClientSize.Width == 0 || renderForm.ClientSize.Height == 0)
            {
                return;
            }

            // Resize the backbuffer 
            swapChain.ResizeBuffers(
                1,
                width,
                height,
                Format.R8G8B8A8_UNorm,
                SwapChainFlags.None
            );

            // Get the actual backbuffer texture from swapchain
            Texture2D backBufferTexture = swapChain.GetBackBuffer<Texture2D>(0);

            // Create new render target for backbuffer
            backbufferView = new RenderTargetView(device, backBufferTexture);
            backBufferTexture.Dispose();

            // Depth buffer
            Texture2D depthTexture = new Texture2D(device, new Texture2DDescription()
            {
                Format = Format.D16_UNorm,
                ArraySize = 1,
                MipLevels = 1,
                Width = width,
                Height = height,
                SampleDescription = antiAliasing,
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.DepthStencil,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None
            });

            // Create the depth buffer view
            depthView = new DepthStencilView(device, depthTexture);
            depthTexture.Dispose();

            // Setup targets and viewport for rendering
            deviceContext.Rasterizer.SetViewport(0, 0, width, height);
            deviceContext.OutputMerger.SetTargets(depthView, backbufferView);
        }

        #endregion

        /// <summary>
        /// Called on program close.
        /// Disposes of variables taking up resources.
        /// </summary>
        public void Dispose()
        {
            swapChain.Dispose();
            device.Dispose();
            deviceContext.Dispose();
            renderForm.Dispose();
            raymarchShaderBuffer.Dispose();
            raymarchObjectsBuffer.Dispose();
        }
    }
}