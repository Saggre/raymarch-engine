// Created by Sakri Koskimies (Github: Saggre) on 21/10/2019

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using EconSim.Core;
using EconSim.Geometry;
using EconSim.Input;
using EconSim.Math;
using EconSim.Terrain;
using System.Drawing;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Windows;
using Buffer = SharpDX.Direct3D11.Buffer;

using Color = Microsoft.Xna.Framework.Color;
using Device = SharpDX.Direct3D11.Device;

using Viewport = SharpDX.Viewport;

namespace EconSim
{

    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class EconSim : IDisposable
    {
        private RenderTargetView renderTargetView;

        private RenderForm renderForm;
        private const int Width = 1280;
        private const int Height = 720;

        private HullShader hullShader;
        private DomainShader domainShader;
        private VertexShader vertexShader;
        private PixelShader pixelShader;

        private Device d3dDevice;
        private DeviceContext d3dDeviceContext;
        private SwapChain swapChain;

        private Texture2D terrainTexture;
        private Viewport viewport;

        private Buffer triangleVertexBuffer;

        private Vector3[] vertices = new Vector3[]
            {new Vector3(-0.5f, 0.5f, 0.0f), new Vector3(0.5f, 0.5f, 0.0f), new Vector3(0.0f, -0.5f, 0.0f)};

        private InputElement[] inputElements = new InputElement[]
        {
            new InputElement("POSITION", 0, Format.R32G32B32_Float, 0)
        };

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
            triangleVertexBuffer = Buffer.Create<Vector3>(d3dDevice, BindFlags.VertexBuffer, vertices);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected void Initialize()
        {
            renderForm = new RenderForm("My first SharpDX game");
            renderForm.ClientSize = new Size(Width, Height);
            renderForm.AllowUserResizing = false;

            d3dDevice = Game1.graphics.GraphicsDevice.Handle as Device;


            /*// Load hull shader
            var compiledHullShader = ShaderBytecode.CompileFromFile(@"Shader\Tessellation\TessellationHull.hlsl", "HS", "hs_5_0");
            hullShader = new HullShader(d3dDevice, compiledHullShader.Bytecode);

            // Load domain shader
            var compiledDomainShader = ShaderBytecode.CompileFromFile(@"Shader\Tessellation\TessellationDomain.hlsl", "DS", "ds_5_0");
            domainShader = new DomainShader(d3dDevice, compiledDomainShader.Bytecode);*/

            // Load pixel shader
            var compiledPixelShader =
                ShaderBytecode.CompileFromFile(@"Shader\Tessellation\TessellationPixel.hlsl", "PS", "ps_5_0");
            pixelShader = new PixelShader(d3dDevice, compiledPixelShader.Bytecode);

            // Load vertex shader
            var compiledVertexShader =
                ShaderBytecode.CompileFromFile(@"Shader\Tessellation\TessellationVertex.hlsl", "VS", "vs_5_0");
            vertexShader = new VertexShader(d3dDevice, compiledVertexShader.Bytecode);
            inputSignature = ShaderSignature.GetInputSignature(compiledVertexShader);

            inputLayout = new InputLayout(d3dDevice, inputSignature, inputElements);
            d3dDevice.ImmediateContext.InputAssembler.InputLayout = inputLayout;

            //Compute();


            //d3dDevice.ImmediateContext.HullShader.Set(hullShader);
            //d3dDevice.ImmediateContext.DomainShader.Set(domainShader);
            d3dDevice.ImmediateContext.VertexShader.Set(vertexShader);
            d3dDevice.ImmediateContext.PixelShader.Set(pixelShader);
            d3dDevice.ImmediateContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;

        }

        public void Run()
        {
            RenderLoop.Run(renderForm, RenderCallback);
        }

        private void RenderCallback()
        {
            Draw();
        }

        /*
        void Compute()
        {
            TerrainGenerator terrainGenerator = new TerrainGenerator();
            TerrainChunk c = terrainGenerator.CreateTerrainChunk(new SquareRect(0, 0, 128));
            terrainTexture = c.CreateVertexMaps();
        }
        */

        private void InitializeShaders()
        {
            using (var vertexShaderByteCode = ShaderBytecode.CompileFromFile(@"Shader\Tessellation\Vertex.hlsl", "VS", "vs_4_0", ShaderFlags.Debug))
            {
                vertexShader = new VertexShader(d3dDevice, vertexShaderByteCode);
            }
            using (var pixelShaderByteCode = ShaderBytecode.CompileFromFile(@"Shader\Tessellation\Pixel.hlsl", "PS", "ps_4_0", ShaderFlags.Debug))
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

            inputLayout = new InputLayout(d3dDevice, inputSignature, inputElements);
            d3dDeviceContext.InputAssembler.InputLayout = inputLayout;
        }

        private void Draw()
        {
            d3dDeviceContext.OutputMerger.SetRenderTargets(renderTargetView);
            d3dDeviceContext.ClearRenderTargetView(renderTargetView, new SharpDX.Color(32, 103, 178));

            d3dDeviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(triangleVertexBuffer, Utilities.SizeOf<Vector3>(), 0));
            d3dDeviceContext.Draw(vertices.Count(), 0);

            swapChain.Present(1, PresentFlags.None);
        }

        /*
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            d3dDevice.ImmediateContext.OutputMerger.SetRenderTargets(renderTargetView);
            d3dDevice.ImmediateContext.ClearRenderTargetView(renderTargetView, new SharpDX.Color(32, 103, 178));

            d3dDevice.ImmediateContext.InputAssembler.SetVertexBuffers(0, VertexBufferBinding(triangleVertexBuffer, Utilities.SizeOf<Vector3>(), 0));
            d3dDevice.ImmediateContext.Draw(vertices.Count(), 0);

            //swapChain.Present(1, PresentFlags.None);

            base.Draw(gameTime);
        }*/

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