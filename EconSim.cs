// Created by Sakri Koskimies (Github: Saggre) on 21/10/2019

using System;
using System.Collections.Generic;
using System.Diagnostics;
using EconSim.Geometry;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using WindowsInput.Native;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Windows;

using Buffer = SharpDX.Direct3D11.Buffer;
using Color = System.Drawing.Color;
using Vector3 = System.Numerics.Vector3;
using Vector2 = System.Numerics.Vector2;
using Quaternion = System.Numerics.Quaternion;
using Device = SharpDX.Direct3D11.Device;

using Viewport = SharpDX.Viewport;
using EconSim.Core;
using EconSim.Core.Input;
using EconSim.Game;
using EconSim.Math;
using Matrix = SharpDX.Matrix;

namespace EconSim
{



    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class EconSim : IDisposable
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct PerFrameBuffer
        {
            public Matrix modelMatrix;
            public Matrix viewMatrix;
            public Matrix projectionMatrix;
        }

        public PerFrameBuffer frameBuffer;

        private RenderTargetView renderTargetView;

        private RenderForm renderForm;
        private const int Width = 1280;
        private const int Height = 720;

        public static Device d3dDevice;
        public static DeviceContext d3dDeviceContext;
        private SwapChain swapChain;

        private Viewport viewport;

        public static Camera mainCamera;
        private Vector2 lookVector;

        public static Scene mainScene;
        private Mouse mainMouse;
        private Keyboard mainKeyboard;
        private PlayerMovement movementInput;

        private Stopwatch stopwatch;
        private GameLogic gameLogic;

        public EconSim()
        {
            renderForm = new RenderForm("EconSim");
            renderForm.ClientSize = new Size(Width, Height);
            renderForm.AllowUserResizing = false;

            // Set camera initial pos
            mainCamera = new Camera();
            mainCamera.Position = new Vector3(0, 2, 3);
            lookVector = new Vector2(0, 140);

            // Create main scene
            mainScene = new Scene();

            // Init inputs
            mainMouse = new Mouse(renderForm);
            mainMouse.HideCursor();
            mainKeyboard = new Keyboard();

            // Init main game logic script
            gameLogic = new GameLogic();

            // Init movement manager
            movementInput = new PlayerMovement();

            // Init stopwatch for deltaTime
            stopwatch = new Stopwatch();
            stopwatch.Start();

            InitializeDeviceResources();
            InitializeShaders();

            int unixTime = Core.Util.ConvertToUnixTimestamp(DateTime.Now);

            // Execute all start methods
            StaticUpdater.ExecuteStartActions(unixTime);

            // Execute each scene GameObject's updateables
            foreach (GameObject mainSceneGameObject in mainScene.GameObjects)
            {
                foreach (IUpdateable updateable in mainSceneGameObject.Updateables)
                {
                    updateable.Start(unixTime);
                }
            }
        }

        public Matrix ProjectionMatrix()
        {
            float aspectRatio = (float)renderForm.Width / (float)renderForm.Height;
            float fieldOfView = (float)System.Math.PI / 4.0f;
            float nearClipPlane = 0.1f;
            float farClipPlane = 200.0f;

            return Matrix.PerspectiveFovLH(
              fieldOfView, aspectRatio, nearClipPlane, farClipPlane);
        }

        /// <summary>
        /// Initialize DirectX
        /// </summary>
        private void InitializeDeviceResources()
        {
            ModeDescription backBufferDesc =
              new ModeDescription(Width, Height, new Rational(60, 1), Format.R8G8B8A8_UNorm);

            SwapChainDescription swapChainDesc = new SwapChainDescription()
            {
                ModeDescription = backBufferDesc,
                SampleDescription = new SampleDescription(1, 0),
                Usage = Usage.RenderTargetOutput,
                BufferCount = 1,
                OutputHandle = renderForm.Handle,
                IsWindowed = true
            };

            Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.None, swapChainDesc, out d3dDevice,
              out swapChain);
            d3dDeviceContext = d3dDevice.ImmediateContext;

            using (Texture2D backBuffer = swapChain.GetBackBuffer<Texture2D>(0))
            {
                renderTargetView = new RenderTargetView(d3dDevice, backBuffer);
            }

            viewport = new Viewport(0, 0, Width, Height);
            d3dDevice.ImmediateContext.Rasterizer.SetViewport(viewport);
        }

        public void Run()
        {
            RenderLoop.Run(renderForm, RenderCallback);
        }

        private float lastDeltaTime;
        private void RenderCallback()
        {

            stopwatch.Restart();

            // Execute all update methods
            StaticUpdater.ExecuteUpdateActions(lastDeltaTime);

            // Render
            Draw(lastDeltaTime, gameObject =>
            {
                // Execute updates per-object
                foreach (IUpdateable updateable in gameObject.Updateables)
                {
                    updateable.Update(lastDeltaTime);
                }
            });

            stopwatch.Stop();
            lastDeltaTime = (float)stopwatch.Elapsed.TotalSeconds;
        }

        private void InitializeShaders()
        {

            d3dDeviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
        }

        /// <summary>
        /// Draw vertices and call the callback between setting object-specific shaders and object-specific buffers.
        /// </summary>
        /// <param name="deltaTime"></param>
        /// <param name="updateCallbackAction"></param>
        private void Draw(float deltaTime, Action<GameObject> updateCallbackAction)
        {
            // Close program with esc
            if (mainKeyboard.IsKeyDown(VirtualKeyCode.ESCAPE))
            {
                renderForm.Dispose();
            }

            // Move camera
            mainCamera.Move(movementInput.MovementInput.Multiply(mainCamera.Rotation), deltaTime);

            // Rotate camera
            // 180, 171
            lookVector.X += mainMouse.DeltaPosition.X * deltaTime;
            lookVector.Y -= mainMouse.DeltaPosition.Y * deltaTime;

            if (lookVector.Y < 100)
            {
                lookVector.Y = 100;
            }
            else if (lookVector.Y > 260 - float.Epsilon)
            {
                lookVector.Y = 260;
            }

            mainCamera.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, lookVector.X * Math.Util.Deg2Rad) *
                             Quaternion.CreateFromAxisAngle(Vector3.UnitX, lookVector.Y * Math.Util.Deg2Rad);


            d3dDeviceContext.OutputMerger.SetRenderTargets(renderTargetView);
            d3dDeviceContext.ClearRenderTargetView(renderTargetView, new SharpDX.Color(32, 103, 178));

            // These matrices are not per-object
            frameBuffer.viewMatrix = mainCamera.ViewMatrix();
            frameBuffer.projectionMatrix = ProjectionMatrix();
            frameBuffer.viewMatrix.Transpose();
            frameBuffer.projectionMatrix.Transpose();

            // Render all GameObjects in scene
            foreach (GameObject gameObject in mainScene.GameObjects)
            {
                // This matrix is per-object
                frameBuffer.modelMatrix = gameObject.ModelMatrix();
                frameBuffer.modelMatrix.Transpose();

                Buffer sharpDxPerFrameBuffer = Buffer.Create(d3dDevice,
                    BindFlags.ConstantBuffer,
                    ref frameBuffer,
                    Utilities.SizeOf<PerFrameBuffer>(),
                    ResourceUsage.Default,
                    CpuAccessFlags.None,
                    ResourceOptionFlags.None,
                    0);

                gameObject.Shader.SetConstantBuffer(0, sharpDxPerFrameBuffer);

                Buffer vertexBuffer = gameObject.Mesh.GetVertexBuffer();

                // TODO also set other shaders
                // Set as current vertex and pixel shaders
                d3dDeviceContext.InputAssembler.InputLayout = gameObject.Shader.GetInputLayout();
                d3dDeviceContext.VertexShader.Set(gameObject.Shader.VertexShader);
                d3dDeviceContext.PixelShader.Set(gameObject.Shader.PixelShader);

                d3dDeviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(vertexBuffer, Utilities.SizeOf<RenderVertex>(), 0));

                // TODO set gameObject buffers even if using the same shader
                foreach (KeyValuePair<int, ShaderResourceView> shaderResource in gameObject.Shader.ShaderResources(gameObject))
                {
                    d3dDeviceContext.PixelShader.SetShaderResource(shaderResource.Key, shaderResource.Value);
                }

                // Call Updates
                updateCallbackAction(gameObject);

                d3dDeviceContext.Draw(gameObject.Mesh.Vertices.Length, 0);

                sharpDxPerFrameBuffer.Dispose();
            }

            swapChain.Present(1, PresentFlags.None);
        }

        public void Dispose()
        {
            renderTargetView.Dispose();
            swapChain.Dispose();
            d3dDevice.Dispose();
            d3dDeviceContext.Dispose();
            renderForm.Dispose();

            int unixTime = Core.Util.ConvertToUnixTimestamp(DateTime.Now);

            // Execute all end methods
            StaticUpdater.ExecuteEndActions(unixTime);

            // Execute each scene GameObject's end methods
            foreach (GameObject mainSceneGameObject in mainScene.GameObjects)
            {
                foreach (IUpdateable updateable in mainSceneGameObject.Updateables)
                {
                    updateable.End(unixTime);
                }
            }

        }

    }
}