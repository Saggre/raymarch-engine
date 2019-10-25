// Created by Sakri Koskimies (Github: Saggre) on 25/10/2019

using EconSim.Geometry;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;

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

        public Shader(VertexShader vertexShader, HullShader hullShader, DomainShader domainShader, GeometryShader geometryShader, PixelShader pixelShader)
        {
            this.vertexShader = vertexShader;
            this.hullShader = hullShader;
            this.domainShader = domainShader;
            this.geometryShader = geometryShader;
            this.pixelShader = pixelShader;
        }

        public static Shader CompileFromFiles(string folderPath)
        {
            // TODO paths

            var vertexShaderByteCode = ShaderBytecode.CompileFromFile(@"Shader\Diffuse\Vertex.hlsl", "VS", "vs_5_0", ShaderFlags.Debug);
            VertexShader vs = new VertexShader(EconSim.d3dDevice, vertexShaderByteCode);

            var pixelShaderByteCode = ShaderBytecode.CompileFromFile(@"Shader\Diffuse\Pixel.hlsl", "PS", "ps_5_0", ShaderFlags.Debug);
            PixelShader ps = new PixelShader(EconSim.d3dDevice, pixelShaderByteCode);

            Shader shader = new Shader(vs, null, null, null, ps);

            ShaderSignature inputSignature = ShaderSignature.GetInputSignature(vertexShaderByteCode);
            shader.inputLayout = new InputLayout(EconSim.d3dDevice, inputSignature, RenderVertex.InputElements);

            return shader;
        }

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
            // TODO add also other shaders
        }

        /// <summary>
        /// Sampler
        /// </summary>
        /// <param name="slot"></param>
        /// <param name="sampler"></param>
        public void SetSampler(int slot, SamplerState sampler)
        {
            EconSim.d3dDeviceContext.PixelShader.SetSampler(slot, sampler);
        }

        /// <summary>
        /// SRV
        /// </summary>
        /// <param name="slot"></param>
        /// <param name="resourceView"></param>
        public void SetShaderResource(int slot, ShaderResourceView resourceView)
        {
            EconSim.d3dDeviceContext.PixelShader.SetShaderResource(0, resourceView);
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