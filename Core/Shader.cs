// Created by Sakri Koskimies (Github: Saggre) on 25/10/2019

using System;
using System.Collections.Generic;
using System.IO;
using EconSim.Geometry;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace EconSim.Core
{

    public class PerObjectResources
    {
        // Dict keys are shader slots
        public Dictionary<int, Buffer> ConstantBuffers;
        public Dictionary<int, ShaderResourceView> ShaderResourceViews;

        public PerObjectResources()
        {
            ConstantBuffers = new Dictionary<int, Buffer>();
            ShaderResourceViews = new Dictionary<int, ShaderResourceView>();
        }
    }

    /// <summary>
    /// A shader class combining different shader stages. Extends CommonShaderStage to add things such as buffers to all shader stages.
    /// </summary>
    public class Shader
    {
        private InputLayout inputLayout;
        private VertexShader vertexShader;
        private HullShader hullShader;
        private DomainShader domainShader;
        private GeometryShader geometryShader;
        private PixelShader pixelShader;


        public Shader(InputLayout inputLayout, VertexShader vertexShader, HullShader hullShader, DomainShader domainShader, GeometryShader geometryShader, PixelShader pixelShader)
        {
            this.inputLayout = inputLayout;
            this.vertexShader = vertexShader;
            this.hullShader = hullShader;
            this.domainShader = domainShader;
            this.geometryShader = geometryShader;
            this.pixelShader = pixelShader;
        }

        /// <summary>
        /// Compiles files into shader byte-code and creates a shader from the shader files that exist
        /// </summary>
        /// <param name="folderPath"></param>
        public static Shader CompileFromFiles(string folderPath)
        {
            ShaderFlags shaderFlags = ShaderFlags.Debug;

            InputLayout inputLayout = null;
            VertexShader vertexShader = null;
            HullShader hullShader = null;
            DomainShader domainShader = null;
            GeometryShader geometryShader = null;
            PixelShader pixelShader = null;

            // Handler for #include directive
            HLSLFileIncludeHandler includeHandler = new HLSLFileIncludeHandler(folderPath);

            // Vertex shader + Shader signature
            {
                string path = Path.Combine(folderPath, "Vertex.hlsl");

                if (File.Exists(path))
                {
                    CompilationResult byteCode = ShaderBytecode.CompileFromFile(path, "main", "vs_5_0", shaderFlags,
                        EffectFlags.None, null, includeHandler);
                    vertexShader = new VertexShader(Engine.RenderDevice.d3dDevice, byteCode);

                    ShaderSignature inputSignature = ShaderSignature.GetInputSignature(byteCode);
                    inputLayout = new InputLayout(Engine.RenderDevice.d3dDevice, inputSignature, RenderVertex.InputElements);
                }
                else
                {
                    // TODO fail
                }
            }

            // Hull shader
            {
                string path = Path.Combine(folderPath, "Hull.hlsl");

                if (File.Exists(path))
                {
                    CompilationResult byteCode = ShaderBytecode.CompileFromFile(path, "main", "hs_5_0", shaderFlags,
                        EffectFlags.None, null, includeHandler);
                    hullShader = new HullShader(Engine.RenderDevice.d3dDevice, byteCode);
                }
            }

            // Domain shader
            {
                string path = Path.Combine(folderPath, "Domain.hlsl");

                if (File.Exists(path))
                {
                    CompilationResult byteCode = ShaderBytecode.CompileFromFile(path, "main", "ds_5_0", shaderFlags,
                        EffectFlags.None, null, includeHandler);
                    domainShader = new DomainShader(Engine.RenderDevice.d3dDevice, byteCode);
                }
            }

            // Geometry shader
            {
                string path = Path.Combine(folderPath, "Geometry.hlsl");

                if (File.Exists(path))
                {
                    CompilationResult byteCode = ShaderBytecode.CompileFromFile(path, "main", "gs_5_0", shaderFlags,
                        EffectFlags.None, null, includeHandler);
                    geometryShader = new GeometryShader(Engine.RenderDevice.d3dDevice, byteCode);
                }
            }

            // Pixel shader
            {
                string path = Path.Combine(folderPath, "Pixel.hlsl");

                if (File.Exists(path))
                {
                    CompilationResult byteCode = ShaderBytecode.CompileFromFile(path, "main", "ps_5_0", shaderFlags,
                        EffectFlags.None, null, includeHandler);
                    pixelShader = new PixelShader(Engine.RenderDevice.d3dDevice, byteCode);
                }
            }

            return new SharedShader(inputLayout, vertexShader, hullShader, domainShader, geometryShader, pixelShader);
        }

        public InputLayout GetInputLayout()
        {
            return inputLayout;
        }

        #region MyRegion

        /// <summary>
        /// Creates a buffer containing a single instance if a struct
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bufferData"></param>
        /// <returns></returns>
        public static Buffer CreateSingleElementBuffer<T>(ref T bufferData) where T : struct
        {
            return Buffer.Create(Engine.RenderDevice.d3dDevice,
                BindFlags.ConstantBuffer,
                ref bufferData,
                Utilities.SizeOf<T>(),
                ResourceUsage.Default,
                CpuAccessFlags.None,
                ResourceOptionFlags.None,
                0);
        }

        #endregion

        #region Implements

        /// <summary>
        /// Send the buffer to all shader stages
        /// </summary>
        /// <param name="slot"></param>
        /// <param name="constantBuffer"></param>
        public void SendBufferToShader(int slot, Buffer constantBuffer)
        {
            Engine.RenderDevice.d3dDeviceContext.VertexShader.SetConstantBuffer(slot, constantBuffer);
            Engine.RenderDevice.d3dDeviceContext.HullShader.SetConstantBuffer(slot, constantBuffer);
            Engine.RenderDevice.d3dDeviceContext.DomainShader.SetConstantBuffer(slot, constantBuffer);
            Engine.RenderDevice.d3dDeviceContext.GeometryShader.SetConstantBuffer(slot, constantBuffer);
            Engine.RenderDevice.d3dDeviceContext.PixelShader.SetConstantBuffer(slot, constantBuffer);
        }

        /// <summary>
        /// Send the shader resource view to all shader stages
        /// </summary>
        /// <param name="slot"></param>
        /// <param name="resourceView"></param>
        public void SendResourceViewToShader(int slot, ShaderResourceView resourceView)
        {
            // TODO ability to send selectively to only certain stages
            Engine.RenderDevice.d3dDeviceContext.VertexShader.SetShaderResource(slot, resourceView);
            Engine.RenderDevice.d3dDeviceContext.HullShader.SetShaderResource(slot, resourceView);
            Engine.RenderDevice.d3dDeviceContext.DomainShader.SetShaderResource(slot, resourceView);
            Engine.RenderDevice.d3dDeviceContext.GeometryShader.SetShaderResource(slot, resourceView);
            Engine.RenderDevice.d3dDeviceContext.PixelShader.SetShaderResource(slot, resourceView);
        }

        #endregion

        public VertexShader VertexShader => vertexShader;

        public HullShader HullShader => hullShader;

        public DomainShader DomainShader => domainShader;

        public GeometryShader GeometryShader => geometryShader;

        public PixelShader PixelShader => pixelShader;
    }

}