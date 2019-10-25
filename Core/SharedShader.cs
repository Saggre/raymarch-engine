// Created by Sakri Koskimies (Github: Saggre) on 25/10/2019

using System.Collections.Generic;
using EconSim.Geometry;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;

namespace EconSim.Core
{
    /// <summary>
    /// A shader class combining different shader stages. Extends CommonShaderStage to add things such as buffers to all shader stages.
    /// SharedShader saves different buffers per-object and switches between them. This enables two objects to use the same shader, but with different textures for example. (TODO)
    /// </summary>
    public struct SharedShader
    {
        private InputLayout inputLayout;
        private VertexShader vertexShader;
        private HullShader hullShader;
        private DomainShader domainShader;
        private GeometryShader geometryShader;
        private PixelShader pixelShader;

        // TODO struct for shader buffers

        // TODO create shader base class or interface

        // Example
        private Dictionary<int, ShaderResourceView> shaderResources;

        public SharedShader(VertexShader vertexShader, HullShader hullShader, DomainShader domainShader,
            GeometryShader geometryShader, PixelShader pixelShader)
        {
            inputLayout = null;

            this.vertexShader = vertexShader;
            this.hullShader = hullShader;
            this.domainShader = domainShader;
            this.geometryShader = geometryShader;
            this.pixelShader = pixelShader;

            shaderResources = new Dictionary<int, ShaderResourceView>();
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

}