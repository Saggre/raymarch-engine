// Created by Sakri Koskimies (Github: Saggre) on 21/10/2019

using System;
using System.Linq;
using System.Numerics;
using EconSim.Geometry;
using System.Drawing;
using System.Runtime.InteropServices;
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
using Device = SharpDX.Direct3D11.Device;

using Viewport = SharpDX.Viewport;

namespace EconSim
{

    [StructLayout(LayoutKind.Sequential)]
    public struct PerFrameBuffer
    {
        public Matrix modelMatrix; // 16 floats
        public Matrix viewMatrix; // 16 floats
        public Matrix projectionMatrix; // 16 floats
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

        private Buffer triangleVertexBuffer;
        private PerFrameBuffer frameBuffer;

        private RenderVertex[] vertices = Primitive.Plane();

        private ShaderSignature inputSignature;
        private InputLayout inputLayout;

        public EconSim()
        {
            renderForm = new RenderForm("EconSim");
            renderForm.ClientSize = new Size(Width, Height);
            renderForm.AllowUserResizing = false;

            InitializeDeviceResources();
            InitializeShaders();
            InitializeTriangle();

            // Temp
            TerrainGenerator terrainGenerator = new TerrainGenerator();
            TerrainChunk c = terrainGenerator.CreateTerrainChunk(new SquareRect(0, 0, 128));
            texture = c.CreateVertexMaps();
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

        private void InitializeTriangle()
        {
            triangleVertexBuffer = Buffer.Create(d3dDevice, BindFlags.VertexBuffer, vertices);
        }

        public void Run()
        {
            RenderLoop.Run(renderForm, RenderCallback);
        }

        private void RenderCallback()
        {
            Draw();
        }

        private void InitializeShaders()
        {
            using (var vertexShaderByteCode = ShaderBytecode.CompileFromFile(@"Shader\Diffuse\Vertex.hlsl", "VS", "vs_4_0", ShaderFlags.Debug))
            {
                vertexShader = new VertexShader(d3dDevice, vertexShaderByteCode);
            }
            using (var pixelShaderByteCode = ShaderBytecode.CompileFromFile(@"Shader\Diffuse\Pixel.hlsl", "PS", "ps_4_0", ShaderFlags.Debug))
            {
                pixelShader = new PixelShader(d3dDevice, pixelShaderByteCode);
            }

            // Set as current vertex and pixel shaders
            d3dDeviceContext.VertexShader.Set(vertexShader);
            d3dDeviceContext.PixelShader.Set(pixelShader);

            d3dDeviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;

            using (var vertexShaderByteCode = ShaderBytecode.CompileFromFile(@"Shader\Tessellation\Vertex.hlsl", "VS", "vs_4_0", ShaderFlags.Debug))
            {
                inputSignature = ShaderSignature.GetInputSignature(vertexShaderByteCode);
            }

            inputLayout = new InputLayout(d3dDevice, inputSignature, RenderVertex.InputElements);
            d3dDeviceContext.InputAssembler.InputLayout = inputLayout;
        }

        private void Draw()
        {
            Buffer sharpDxPerFrameBuffer = Buffer.Create(d3dDevice,
                BindFlags.ConstantBuffer,
                ref frameBuffer,
                192, // 3 * 4X4m
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
            }, new DataRectangle(Core.Util.GetDataPtr(texture), 4));

            ShaderResourceView textureView = new ShaderResourceView(d3dDevice, t);

            d3dDeviceContext.OutputMerger.SetRenderTargets(renderTargetView);
            d3dDeviceContext.ClearRenderTargetView(renderTargetView, new SharpDX.Color(32, 103, 178));

            d3dDeviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(triangleVertexBuffer, Utilities.SizeOf<RenderVertex>(), 0));
            d3dDeviceContext.VertexShader.SetConstantBuffer(0, sharpDxPerFrameBuffer);
            d3dDeviceContext.PixelShader.SetShaderResource(1, textureView);

            d3dDeviceContext.Draw(vertices.Count(), 0);

            swapChain.Present(1, PresentFlags.None);

            // TODO fix memory leak
            textureView.Dispose();
            t.Dispose();
        }

        public void Dispose()
        {
            renderTargetView.Dispose();
            swapChain.Dispose();
            d3dDevice.Dispose();
            d3dDeviceContext.Dispose();
            renderForm.Dispose();
            triangleVertexBuffer.Dispose();
            vertexShader.Dispose();
            pixelShader.Dispose();
            inputLayout.Dispose();
            inputSignature.Dispose();
        }

    }
}