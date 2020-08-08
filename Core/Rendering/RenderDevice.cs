// Created by Sakri Koskimies (Github: Saggre) on 05/11/2019

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using WindowsInput.Native;
using EconSim.Core.Input;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Windows;
using Device11 = SharpDX.Direct3D11.Device;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace EconSim.Core.Rendering
{
    /// <summary>
    /// A class that handles rendering the visible
    /// </summary>
    public class RenderDevice : IDisposable
    {
        private RenderTargetView backbufferView;
        private DepthStencilView zbufferView;

        private RasterizerState rasterState;
        private BlendState blendState;
        private DepthStencilState depthState;
        private SamplerState samplerState;

        private RenderForm renderForm;

        public Device11 d3dDevice;
        public DeviceContext d3dDeviceContext;
        private SwapChain swapChain;

        /// <summary>
        /// Indicate that device must be resized
        /// </summary>
        public bool MustResize { get; private set; }

        /// <summary>
        /// Data that is sent to every shaded object
        /// </summary>
        public struct PerFrameBuffer
        {
            /// <summary>
            /// Object position, rotation, scale
            /// </summary>
            public Matrix modelMatrix;

            /// <summary>
            /// Camera position, rotation, scale
            /// </summary>
            public Matrix viewMatrix;

            /// <summary>
            /// Projection matrix
            /// </summary>
            public Matrix projectionMatrix;
        }

        /// <summary>
        /// Data that is sent to every shaded object
        /// </summary>
        public PerFrameBuffer frameBuffer;

        private Viewport viewport;

        public RenderDevice(RenderForm renderForm)
        {
            this.renderForm = renderForm;
            InitializeDeviceResources();
        }


        /// <summary>
        /// Initialize DirectX
        /// </summary>
        private void InitializeDeviceResources()
        {

            ModeDescription backBufferDesc =
                new ModeDescription(Engine.Width, Engine.Height, new Rational(Engine.Fps, 1), Format.R8G8B8A8_UNorm);

            SwapChainDescription swapChainDesc = new SwapChainDescription()
            {
                ModeDescription = backBufferDesc,
                SampleDescription = new SampleDescription(1, 0),
                Usage = Usage.RenderTargetOutput,
                BufferCount = 1,
                OutputHandle = renderForm.Handle,
                IsWindowed = !Engine.IsFullscreen,
                Flags = SwapChainFlags.AllowModeSwitch, // Allows other fullscreen resolutions than native one.
            };

            FeatureLevel[] levels = { FeatureLevel.Level_11_1 };
            Device11.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.None, levels, swapChainDesc, out d3dDevice,
                out swapChain);
            d3dDeviceContext = d3dDevice.ImmediateContext;

            /*using (Texture2D backBuffer = swapChain.GetBackBuffer<Texture2D>(0))
            {
                renderTargetView = new RenderTargetView(d3dDevice, backBuffer);
            }*/

            viewport = new Viewport(0, 0, Engine.Width, Engine.Height);
            d3dDevice.ImmediateContext.Rasterizer.SetViewport(viewport);

            //Ignore all windows events
            var factory = swapChain.GetParent<Factory>();
            factory.MakeWindowAssociation(renderForm.Handle, WindowAssociationFlags.IgnoreAll);

            // TODO what is this
            //Setup handler on resize form
            renderForm.UserResized += (sender, args) => MustResize = true;

            //Set Default State
            SetDefaultRasterState();
            //SetWireframeRasterState();
            SetDefaultDepthState();
            SetDefaultBlendState();
            SetDefaultSamplerState();

            //Resize all items
            Resize();
        }

        #region Draw loop

        /// <summary>
        /// Draw vertices and call the callback between setting object-specific shaders and object-specific buffers.
        /// </summary>
        /// <param name="deltaTime"></param>
        /// <param name="updateCallbackAction"></param>
        public void Draw(float deltaTime, Action<GameObject> updateCallbackAction)
        {
            // Close program with esc
            if (InputDevice.Keyboard.IsKeyDown(VirtualKeyCode.ESCAPE))
            {
                renderForm.Dispose();
            }

            //Resizing
            if (MustResize)
            {
                Resize();
            }

            //clear color
            Clear(Color.CornflowerBlue);
            UpdateAllStates();

            // These matrices are not per-object
            frameBuffer.viewMatrix = Engine.CurrentScene.ActiveCamera.ViewMatrix();
            frameBuffer.projectionMatrix = ProjectionMatrix();
            frameBuffer.viewMatrix.Transpose();
            frameBuffer.projectionMatrix.Transpose();

            // Render all GameObjects in scene
            foreach (GameObject gameObject in Engine.CurrentScene.GameObjects)
            {
                // Don't render if the object is not active
                if (!gameObject.Active)
                {
                    continue;
                }

                // This matrix is per-object
                frameBuffer.modelMatrix = gameObject.ModelMatrix();
                frameBuffer.modelMatrix.Transpose();

                Buffer sharpDxPerFrameBuffer = Shader.CreateSingleElementBuffer(ref frameBuffer);

                if (gameObject.Shader == null)
                {
                    throw new ArgumentNullException("A GameObject's shader is not set");
                }

                gameObject.Shader.SendBufferToShader(0, sharpDxPerFrameBuffer);

                // Set as current shaders
                d3dDeviceContext.InputAssembler.InputLayout = gameObject.Shader.InputLayout;
                d3dDeviceContext.VertexShader.Set(gameObject.Shader.VertexShader);
                d3dDeviceContext.HullShader.Set(gameObject.Shader.HullShader);
                d3dDeviceContext.DomainShader.Set(gameObject.Shader.DomainShader);
                d3dDeviceContext.GeometryShader.Set(gameObject.Shader.GeometryShader);
                d3dDeviceContext.PixelShader.Set(gameObject.Shader.PixelShader);

                // Get GameObject-specific buffers
                foreach (KeyValuePair<int, Buffer> buffer in gameObject.Shader.ConstantBuffers(gameObject))
                {
                    // Updates buffer in all shader types at once
                    gameObject.Shader.SendBufferToShader(buffer.Key, buffer.Value);
                }

                // Get GameObject-specific resource views
                foreach (KeyValuePair<int, ShaderResourceView> shaderResource in gameObject.Shader.ShaderResourceViews(gameObject))
                {
                    gameObject.Shader.SendResourceViewToShader(shaderResource.Key, shaderResource.Value);
                }

                // Call Updates
                updateCallbackAction(gameObject);

                // Draw object through its Mesh class
                //gameObject.Mesh.DrawPatch(PrimitiveTopology.PatchListWith3ControlPoints); <-- THIS IS NEEDED FOR TESSELLATION SHADER
                gameObject.Mesh.Draw(); // <-- THIS IS NEEDED FOR NORMAL SHADER

                sharpDxPerFrameBuffer.Dispose();
            }

            // Present scene to screen
            Present();
        }

        /// <summary>
        /// Clear backbuffer and zbuffer
        /// </summary>
        /// <param name="color">background color</param>
        public void Clear(Color4 color)
        {
            d3dDeviceContext.ClearRenderTargetView(backbufferView, color);
            d3dDeviceContext.ClearDepthStencilView(zbufferView, DepthStencilClearFlags.Depth, 1.0F, 0);
        }

        /// <summary>
        /// Present scene to screen
        /// </summary>
        public void Present()
        {
            swapChain.Present(1, PresentFlags.None);
        }

        #endregion

        public Matrix ProjectionMatrix()
        {
            float aspectRatio = Engine.AspectRatio();
            float fieldOfView = (float)Math.PI / 4.0f;
            float nearClipPlane = 0.1f;
            float farClipPlane = 200.0f;

            return Matrix.PerspectiveFovLH(
                fieldOfView, aspectRatio, nearClipPlane, farClipPlane);
        }

        #region States

        /// <summary>
        /// Set current rasterizer state to default
        /// </summary>
        public void SetDefaultRasterState()
        {
            Utilities.Dispose(ref rasterState);
            //Rasterize state
            RasterizerStateDescription rasterDescription = RasterizerStateDescription.Default();
            rasterDescription.FillMode = FillMode.Solid;
            rasterState = new RasterizerState(d3dDevice, rasterDescription);
            d3dDeviceContext.Rasterizer.State = rasterState;
        }

        /// <summary>
        /// Set current rasterizer state to wireframe
        /// </summary>
        public void SetWireframeRasterState()
        {
            rasterState.Dispose();
            //Rasterize state
            RasterizerStateDescription rasterDescription = RasterizerStateDescription.Default();
            rasterDescription.FillMode = FillMode.Wireframe;
            rasterState = new RasterizerState(d3dDevice, rasterDescription);
            d3dDeviceContext.Rasterizer.State = rasterState;
        }

        /// <summary>
        /// Set current blending state to default
        /// </summary>
        public void SetDefaultBlendState()
        {
            Utilities.Dispose(ref blendState);
            BlendStateDescription description = BlendStateDescription.Default();
            blendState = new BlendState(d3dDevice, description);
        }

        /// <summary>
        /// Set current blending state
        /// </summary>
        /// <param name="operation">Blend Operation</param>
        /// <param name="source">Source Option</param>
        /// <param name="destination">Destination Option</param>
        public void SetBlend(BlendOperation operation, BlendOption source, BlendOption destination)
        {
            Utilities.Dispose(ref blendState);
            BlendStateDescription description = BlendStateDescription.Default();

            description.RenderTarget[0].BlendOperation = operation;
            description.RenderTarget[0].SourceBlend = source;
            description.RenderTarget[0].DestinationBlend = destination;
            description.RenderTarget[0].IsBlendEnabled = true;
            blendState = new BlendState(d3dDevice, description);
        }

        /// <summary>
        /// Set current depth state to default
        /// </summary>
        public void SetDefaultDepthState()
        {
            Utilities.Dispose(ref depthState);
            DepthStencilStateDescription description = DepthStencilStateDescription.Default();
            description.DepthComparison = Comparison.LessEqual;
            description.IsDepthEnabled = true;

            depthState = new DepthStencilState(d3dDevice, description);
        }

        /// <summary>
        /// Set current sampler state to default
        /// </summary>
        public void SetDefaultSamplerState()
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
        /// Set current states inside context
        /// </summary>
        public void UpdateAllStates()
        {
            d3dDeviceContext.Rasterizer.State = rasterState;
            d3dDeviceContext.OutputMerger.SetBlendState(blendState);
            d3dDeviceContext.OutputMerger.SetDepthStencilState(depthState);
            d3dDeviceContext.PixelShader.SetSampler(0, samplerState);
            d3dDeviceContext.DomainShader.SetSampler(0, samplerState);
            // TODO set also other shaders
        }

        #endregion

        #region Resize

        /// <summary>
        /// Create and Resize all items
        /// </summary>
        public void Resize()
        {
            // Dispose all previous allocated resources
            //font.Release();
            Utilities.Dispose(ref backbufferView);
            Utilities.Dispose(ref zbufferView);


            if (renderForm.ClientSize.Width == 0 || renderForm.ClientSize.Height == 0)
                return;

            // Resize the backbuffer
            swapChain.ResizeBuffers(1, renderForm.ClientSize.Width, renderForm.ClientSize.Height, Format.R8G8B8A8_UNorm, SwapChainFlags.None);

            // Get the backbuffer from the swapchain
            Texture2D backBufferTexture = swapChain.GetBackBuffer<Texture2D>(0);

            //update font
            //Font.UpdateResources(_backBufferTexture);

            // Backbuffer
            backbufferView = new RenderTargetView(d3dDevice, backBufferTexture);
            backBufferTexture.Dispose();

            // Depth buffer

            Texture2D zbufferTexture = new Texture2D(d3dDevice, new Texture2DDescription()
            {
                Format = Format.D16_UNorm,
                ArraySize = 1,
                MipLevels = 1,
                Width = renderForm.ClientSize.Width,
                Height = renderForm.ClientSize.Height,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.DepthStencil,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None
            });


            // Create the depth buffer view
            zbufferView = new DepthStencilView(d3dDevice, zbufferTexture);
            zbufferTexture.Dispose();

            SetDefaultTargets();

            // End resize
            MustResize = false;
        }

        /// <summary>
        /// Set default render and depth buffer inside device context
        /// </summary>
        public void SetDefaultTargets()
        {
            // Setup targets and viewport for rendering
            d3dDeviceContext.Rasterizer.SetViewport(0, 0, renderForm.ClientSize.Width, renderForm.ClientSize.Height);
            d3dDeviceContext.OutputMerger.SetTargets(zbufferView, backbufferView);
        }

        #endregion

        public void Dispose()
        {
            swapChain.Dispose();
            d3dDevice.Dispose();
            d3dDeviceContext.Dispose();
            renderForm.Dispose();

            int unixTime = Util.ConvertToUnixTimestamp(DateTime.Now);

            // Execute all end methods
            StaticUpdater.ExecuteEndActions(unixTime);

            // Execute each scene GameObject's end methods
            foreach (GameObject mainSceneGameObject in Engine.CurrentScene.GameObjects)
            {
                foreach (IUpdateable updateable in mainSceneGameObject.Updateables)
                {
                    updateable.End(unixTime);
                }
            }

        }
    }
}
