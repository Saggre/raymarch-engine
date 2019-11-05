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

        // Example
        private Dictionary<int, ShaderResourceView> shaderResources;

        public Shader(InputLayout inputLayout, VertexShader vertexShader, HullShader hullShader, DomainShader domainShader, GeometryShader geometryShader, PixelShader pixelShader)
        {
            this.inputLayout = inputLayout;
            this.vertexShader = vertexShader;
            this.hullShader = hullShader;
            this.domainShader = domainShader;
            this.geometryShader = geometryShader;
            this.pixelShader = pixelShader;

            shaderResources = new Dictionary<int, ShaderResourceView>();
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
                    vertexShader = new VertexShader(EconSim.d3dDevice, byteCode);

                    ShaderSignature inputSignature = ShaderSignature.GetInputSignature(byteCode);
                    inputLayout = new InputLayout(EconSim.d3dDevice, inputSignature, RenderVertex.InputElements);
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
                    hullShader = new HullShader(EconSim.d3dDevice, byteCode);
                }
            }

            // Domain shader
            {
                string path = Path.Combine(folderPath, "Domain.hlsl");

                if (File.Exists(path))
                {
                    CompilationResult byteCode = ShaderBytecode.CompileFromFile(path, "main", "ds_5_0", shaderFlags,
                        EffectFlags.None, null, includeHandler);
                    domainShader = new DomainShader(EconSim.d3dDevice, byteCode);
                }
            }

            // Geometry shader
            {
                string path = Path.Combine(folderPath, "Geometry.hlsl");

                if (File.Exists(path))
                {
                    CompilationResult byteCode = ShaderBytecode.CompileFromFile(path, "main", "gs_5_0", shaderFlags,
                        EffectFlags.None, null, includeHandler);
                    geometryShader = new GeometryShader(EconSim.d3dDevice, byteCode);
                }
            }

            // Pixel shader
            {
                string path = Path.Combine(folderPath, "Pixel.hlsl");

                if (File.Exists(path))
                {
                    CompilationResult byteCode = ShaderBytecode.CompileFromFile(path, "main", "ps_5_0", shaderFlags,
                        EffectFlags.None, null, includeHandler);
                    pixelShader = new PixelShader(EconSim.d3dDevice, byteCode);
                }
            }

            return new SharedShader(inputLayout, vertexShader, hullShader, domainShader, geometryShader, pixelShader);
        }

        public Dictionary<int, ShaderResourceView> ShaderResources => shaderResources;

        public InputLayout GetInputLayout()
        {
            return inputLayout;
        }

        #region Implements

        public void SetVertexBuffers()
        {

        }

        /// <summary>
        /// CBV
        /// </summary>
        /// <param name="slot"></param>
        /// <param name="constantBuffer"></param>
        public void SetConstantBuffer(int slot, Buffer constantBuffer)
        {
            EconSim.d3dDeviceContext.VertexShader.SetConstantBuffer(slot, constantBuffer);
            EconSim.d3dDeviceContext.HullShader.SetConstantBuffer(slot, constantBuffer);
            EconSim.d3dDeviceContext.DomainShader.SetConstantBuffer(slot, constantBuffer);
            EconSim.d3dDeviceContext.GeometryShader.SetConstantBuffer(slot, constantBuffer);
            EconSim.d3dDeviceContext.PixelShader.SetConstantBuffer(slot, constantBuffer);
        }

        /// <summary>
        /// Sampler
        /// </summary>
        /// <param name="slot"></param>
        /// <param name="sampler"></param>
        public void SetSampler(int slot, SamplerState sampler)
        {
            EconSim.d3dDeviceContext.VertexShader.SetSampler(slot, sampler);
            // TODO add also other shaders?
        }

        /// <summary>
        /// SRV
        /// </summary>
        /// <param name="slot"></param>
        /// <param name="resourceView"></param>
        public void SetShaderResource(int slot, ShaderResourceView resourceView)
        {
            shaderResources.Add(slot, resourceView);
            //EconSim.d3dDeviceContext.PixelShader.SetShaderResource(0, resourceView);
        }

        #endregion

        public VertexShader VertexShader => vertexShader;

        public HullShader HullShader => hullShader;

        public DomainShader DomainShader => domainShader;

        public GeometryShader GeometryShader => geometryShader;

        public PixelShader PixelShader => pixelShader;
    }

    public static class ShaderUtils
    {
        private static SamplerState defaultSamplerState;

        // TODO default shader

        /// <summary>
        /// Returns a default sampler
        /// </summary>
        /// <returns></returns>
        public static SamplerState DefaultSamplerState()
        {
            if (defaultSamplerState == null)
            {
                defaultSamplerState = new SamplerState(EconSim.d3dDevice, new SamplerStateDescription()
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
            }

            return defaultSamplerState;
        }
    }
}