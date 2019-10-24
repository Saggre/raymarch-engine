// Created by Sakri Koskimies (Github: Saggre) on 21/10/2019

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using EconSim.Geometry;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using WindowsInput.Native;
using EconSim.Terrain;
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
using EconSim.Math;
using Matrix = SharpDX.Matrix;

namespace EconSim
{

    [StructLayout(LayoutKind.Sequential)]
    public struct PerFrameBuffer
    {
        public Matrix modelMatrix;
        public Matrix viewMatrix;
        public Matrix projectionMatrix;
    }

    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class EconSim : IDisposable
    {
        // Temp variable
        public byte[] texture;

        private RenderTargetView renderTargetView;

        private RenderForm renderForm;
        private const int Width = 1280;
        private const int Height = 720;

        private HullShader hullShader;
        private DomainShader domainShader;
        private VertexShader vertexShader;
        private PixelShader pixelShader;

        public static Device d3dDevice;
        private DeviceContext d3dDeviceContext;
        private SwapChain swapChain;

        private Viewport viewport;

        private Dictionary<GameObject, Buffer> vertexBuffers;
        private PerFrameBuffer frameBuffer;
        private SamplerState sampler;

        private ShaderSignature inputSignature;
        private InputLayout inputLayout;

        private Camera mainCamera;
        private Vector2 lookVector;

        private Scene mainScene;
        private Mouse mainMouse;
        private Keyboard mainKeyboard;
        private PlayerMovement movementInput;

        private Stopwatch stopwatch;

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
            mainScene.AddGameObject(new GameObject(new Mesh(Primitive.Plane())));

            // Init inputs
            mainMouse = new Mouse(renderForm);
            mainMouse.HideCursor();
            mainKeyboard = new Keyboard();

            // Init movement manager
            movementInput = new PlayerMovement();

            // Init stopwatch for deltaTime
            stopwatch = new Stopwatch();
            stopwatch.Start();

            InitializeDeviceResources();
            InitializeShaders();
            InitializeVertexBuffers();

            // Temp
            TerrainGenerator terrainGenerator = new TerrainGenerator();
            TerrainChunk c = terrainGenerator.CreateTerrainChunk(new SquareRect(0, 0, 128));
            texture = c.CreateVertexMaps();

            sampler = new SamplerState(d3dDevice, new SamplerStateDescription()
            {
                Filter = Filter.MinMagMipLinear,
                AddressU = TextureAddressMode.Wrap,
                AddressV = TextureAddressMode.Wrap,
                AddressW = TextureAddressMode.Wrap,
                BorderColor = new Color4(0, 0, 0, 1),
                ComparisonFunction = Comparison.Never,
                MaximumAnisotropy = 16,
                MipLodBias = 0,
                MinimumLod = -float.MaxValue,
                MaximumLod = float.MaxValue
            });

            // Execute all start methods
            StaticUpdater.ExecuteStartActions(DateTime.Now);
        }

        public Matrix ProjectionMatrix()
        {
            float aspectRatio = (float)renderForm.Width / (float)renderForm.Height;
            float fieldOfView = (float)System.Math.PI / 4.0f;
            float nearClipPlane = 0.1f;
            float farClipPlane = 200.0f;
            //return Matrix.CreateOrthographic(5, 5 / aspectRatio, nearClipPlane, farClipPlane);

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

        /// <summary>
        /// Adds the vertices of each gameObject's mesh into a vertex buffer to be sent to the gpu
        /// </summary>
        private void InitializeVertexBuffers()
        {
            vertexBuffers = new Dictionary<GameObject, Buffer>();
            foreach (GameObject gameObject in mainScene.GameObjects)
            {
                vertexBuffers.Add(gameObject, Buffer.Create(d3dDevice, BindFlags.VertexBuffer, gameObject.Mesh.Vertices));
            }
        }

        public void Run()
        {
            RenderLoop.Run(renderForm, RenderCallback);
        }

        private float lastDeltaTime;
        private void RenderCallback()
        {

            stopwatch.Restart();

            // Render
            Draw(lastDeltaTime);

            // Execute all update methods
            StaticUpdater.ExecuteUpdateActions(lastDeltaTime);

            stopwatch.Stop();
            lastDeltaTime = (float)stopwatch.Elapsed.TotalSeconds;
        }

        private void InitializeShaders()
        {
            var vertexShaderByteCode = ShaderBytecode.CompileFromFile(@"Shader\Diffuse\Vertex.hlsl", "VS", "vs_5_0", ShaderFlags.Debug);
            vertexShader = new VertexShader(d3dDevice, vertexShaderByteCode);
            var pixelShaderByteCode = ShaderBytecode.CompileFromFile(@"Shader\Diffuse\Pixel.hlsl", "PS", "ps_5_0", ShaderFlags.Debug);
            pixelShader = new PixelShader(d3dDevice, pixelShaderByteCode);

            // Set as current vertex and pixel shaders
            d3dDeviceContext.VertexShader.Set(vertexShader);
            d3dDeviceContext.PixelShader.Set(pixelShader);

            d3dDeviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;

            inputSignature = ShaderSignature.GetInputSignature(vertexShaderByteCode);

            inputLayout = new InputLayout(d3dDevice, inputSignature, RenderVertex.InputElements);
            d3dDeviceContext.InputAssembler.InputLayout = inputLayout;
        }

        private float r;

        private void Draw(float deltaTime)
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
            Console.WriteLine(lookVector);

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


            r += 0.01f;
            frameBuffer.modelMatrix = Matrix.Identity;// Matrix.RotationX((float)System.Math.PI / 2);
            frameBuffer.viewMatrix = mainCamera.ViewMatrix();
            frameBuffer.projectionMatrix = ProjectionMatrix();

            frameBuffer.modelMatrix.Transpose();
            frameBuffer.viewMatrix.Transpose();
            frameBuffer.projectionMatrix.Transpose();

            //Matrix worldInverseTransposeMatrix = Matrix.Transpose(Matrix.Invert(transform * world));
            //Console.WriteLine(frameBuffer.projectionMatrix);

            Buffer sharpDxPerFrameBuffer = Buffer.Create(d3dDevice,
              BindFlags.ConstantBuffer,
              ref frameBuffer,
              Utilities.SizeOf<PerFrameBuffer>(),
              ResourceUsage.Default,
              CpuAccessFlags.None,
              ResourceOptionFlags.None,
              0);

            // Temp

            Texture2D t = new Texture2D(d3dDevice, new Texture2DDescription()
            {
                BindFlags = BindFlags.UnorderedAccess | BindFlags.ShaderResource,
                Format = Format.R8G8B8A8_UNorm,
                Width = 1024,
                Height = 1024,
                OptionFlags = ResourceOptionFlags.None,
                MipLevels = 1,
                ArraySize = 1,
                SampleDescription = { Count = 1, Quality = 0 }
            }, new DataRectangle(Core.Util.GetDataPtr(texture), 1024 * 4));

            ShaderResourceView textureView = new ShaderResourceView(d3dDevice, t);

            d3dDeviceContext.OutputMerger.SetRenderTargets(renderTargetView);
            d3dDeviceContext.ClearRenderTargetView(renderTargetView, new SharpDX.Color(32, 103, 178));

            // Render all GameObjects in scene
            foreach (KeyValuePair<GameObject, Buffer> keyValuePair in vertexBuffers)
            {
                Buffer vertexBuffer = keyValuePair.Value;
                GameObject gameObject = keyValuePair.Key;

                d3dDeviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(vertexBuffer, Utilities.SizeOf<RenderVertex>(), 0));
                d3dDeviceContext.VertexShader.SetConstantBuffer(0, sharpDxPerFrameBuffer);
                d3dDeviceContext.PixelShader.SetSampler(0, sampler);
                d3dDeviceContext.PixelShader.SetShaderResource(0, textureView);

                d3dDeviceContext.Draw(gameObject.Mesh.Vertices.Length, 0);
            }

            swapChain.Present(1, PresentFlags.None);

            // TODO fix memory leak
            textureView.Dispose();
            t.Dispose();

            // UI


        }

        public void Dispose()
        {
            renderTargetView.Dispose();
            swapChain.Dispose();
            d3dDevice.Dispose();
            d3dDeviceContext.Dispose();
            renderForm.Dispose();
            vertexShader.Dispose();
            pixelShader.Dispose();
            inputLayout.Dispose();
            inputSignature.Dispose();

            // Release all Buffers in scene
            foreach (KeyValuePair<GameObject, Buffer> keyValuePair in vertexBuffers)
            {
                Buffer vertexBuffer = keyValuePair.Value;
                vertexBuffer.Dispose();
            }
        }

    }
}