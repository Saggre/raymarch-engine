// Created by Sakri Koskimies (Github: Saggre) on 05/11/2019

using System;
using System.Drawing;
using System.Linq;
using System.Numerics;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Windows;
using Device11 = SharpDX.Direct3D11.Device;
using Buffer = SharpDX.Direct3D11.Buffer;
using Color = SharpDX.Color;
using Vector3 = System.Numerics.Vector3;

namespace EconSim.Core.Rendering
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

        public Device11 d3dDevice;
        public DeviceContext d3dDeviceContext;
        private SwapChain swapChain;

        // Raymarch
        private Mesh raymarchRenderPlane; // Plane to render raymarch shader on
        private RaymarchShaderBuffer raymarchShaderBufferData; // Values to send to the raymarch shader
        private Buffer raymarchShaderBuffer;
        private RaymarchObjectsBuffer<RaymarchGameObjectBufferData> raymarchObjectsBuffer;

        struct RaymarchShaderBuffer
        {
            public Vector3 cameraPosition;
            public float aspectRatio;
            public float time;
            public int objectCount;

            /// <summary>
            /// Camera position, rotation, scale
            /// </summary>
            public Matrix4x4 viewMatrix;

            public Matrix4x4[] Get()
            {
                return new Matrix4x4[]
                {
                    viewMatrix,
                    new Matrix4x4(
                        cameraPosition.X, cameraPosition.Y, cameraPosition.Z, aspectRatio, time,
                        Engine.CurrentScene.GameObjects.Count,
                        1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f
                    ),
                };
            }
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

            raymarchShaderBuffer = Buffer.Create(Engine.RenderDevice.d3dDevice,
                BindFlags.ConstantBuffer,
                ref raymarchShaderBufferData,
                128,
                ResourceUsage.Default,
                CpuAccessFlags.None,
                ResourceOptionFlags.None,
                0);

            d3dDeviceContext.VertexShader.SetConstantBuffer(0, raymarchShaderBuffer);
            d3dDeviceContext.PixelShader.SetConstantBuffer(0, raymarchShaderBuffer);

            // Set as current shaders
            // TODO what's inputlayout?
            d3dDeviceContext.InputAssembler.InputLayout = raymarchShader.InputLayout;
            d3dDeviceContext.VertexShader.Set(raymarchShader.VertexShader);
            d3dDeviceContext.PixelShader.Set(raymarchShader.PixelShader);

            raymarchObjectsBuffer = new RaymarchObjectsBuffer<RaymarchGameObjectBufferData>(d3dDevice);
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

            Device11.CreateWithSwapChain(
                DriverType.Hardware,
                DeviceCreationFlags.DisableGpuTimeout,
                levels,
                swapChainDesc,
                out d3dDevice,
                out swapChain
            );

            d3dDeviceContext = d3dDevice.ImmediateContext;

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

            rasterState = new RasterizerState(d3dDevice, description);
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

            blendState = new BlendState(d3dDevice, description);
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

            depthState = new DepthStencilState(d3dDevice, description);
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

            samplerState = new SamplerState(d3dDevice, description);
        }

        /// <summary>
        /// Applies states that have been saved as variables to the device context
        /// </summary>
        void ApplyStates()
        {
            d3dDeviceContext.Rasterizer.State = rasterState;
            d3dDeviceContext.OutputMerger.SetBlendState(blendState);
            d3dDeviceContext.OutputMerger.SetDepthStencilState(depthState);

            // TODO only slot 0?
            d3dDeviceContext.PixelShader.SetSampler(0, samplerState);
            d3dDeviceContext.DomainShader.SetSampler(0, samplerState);
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
            Clear(Color.CornflowerBlue);

            // Set raymarch shader buffer data
            {
                // These matrices are not per-object
                raymarchShaderBufferData.viewMatrix = Engine.CurrentScene.ActiveCamera.ViewMatrix();
                raymarchShaderBufferData.cameraPosition = Engine.CurrentScene.ActiveCamera.Position;
                raymarchShaderBufferData.aspectRatio = Engine.AspectRatio();
                raymarchShaderBufferData.time = Engine.ElapsedTime; // TODO reset time when it is too large

                d3dDeviceContext.UpdateSubresource(raymarchShaderBufferData.Get(), raymarchShaderBuffer);

                // TODO don't do this every frame, but only when needed
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
            d3dDeviceContext.ClearRenderTargetView(backbufferView, color);
            d3dDeviceContext.ClearDepthStencilView(depthView, DepthStencilClearFlags.Depth, 1.0F, 0);
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
            //font.Release();
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
            backbufferView = new RenderTargetView(d3dDevice, backBufferTexture);
            backBufferTexture.Dispose();

            // Depth buffer
            Texture2D depthTexture = new Texture2D(d3dDevice, new Texture2DDescription()
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
            depthView = new DepthStencilView(d3dDevice, depthTexture);
            depthTexture.Dispose();

            // Setup targets and viewport for rendering
            d3dDeviceContext.Rasterizer.SetViewport(0, 0, width, height);
            d3dDeviceContext.OutputMerger.SetTargets(depthView, backbufferView);
        }

        #endregion

        /// <summary>
        /// Called on program close.
        /// Disposes of variables taking up resources.
        /// </summary>
        public void Dispose()
        {
            swapChain.Dispose();
            d3dDevice.Dispose();
            d3dDeviceContext.Dispose();
            renderForm.Dispose();
            raymarchShaderBuffer.Dispose();
        }
    }
}