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
    public class SharedShader : Shader
    {
        public class PerObjectShaderBuffers
        {
            // Dict keys are shader slots
            public Dictionary<int, Buffer> ConstantBuffers;
            public Dictionary<int, SamplerState> Samplers;
            public Dictionary<int, ShaderResourceView> ResourceViews;

            public PerObjectShaderBuffers()
            {
                ConstantBuffers = new Dictionary<int, Buffer>();
                Samplers = new Dictionary<int, SamplerState>();
                ResourceViews = new Dictionary<int, ShaderResourceView>();
            }
        }

        /*public static SharedShader FromShader(Shader sourceShader)
        {
            SharedShader sharedShader = (SharedShader)sourceShader;
            return
        }*/

        private Dictionary<GameObject, PerObjectShaderBuffers> perObjectShaderResources;

        public SharedShader(InputLayout inputLayout, VertexShader vertexShader, HullShader hullShader, DomainShader domainShader,
            GeometryShader geometryShader, PixelShader pixelShader) : base(inputLayout, vertexShader, hullShader, domainShader, geometryShader, pixelShader)
        {
            perObjectShaderResources = new Dictionary<GameObject, PerObjectShaderBuffers>();
        }

        public Dictionary<GameObject, PerObjectShaderBuffers> PerObjectShaderResources => perObjectShaderResources;

        #region Implements

        /// <summary>
        /// Add a GameObject to Per-object shader resources list if it already isn't there
        /// </summary>
        /// <param name="gameObject"></param>
        private void AddGameObjectIfNotExists(GameObject gameObject)
        {
            if (!perObjectShaderResources.ContainsKey(gameObject))
            {
                perObjectShaderResources.Add(gameObject, new PerObjectShaderBuffers());
            }
        }

        public Dictionary<int, ShaderResourceView> ShaderResources(GameObject gameObject)
        {
            // TODO check if gameObject is added
            return perObjectShaderResources[gameObject].ResourceViews;
        }

        /// <summary>
        /// CBV
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="slot"></param>
        /// <param name="constantBuffer"></param>
        public void SetConstantBuffer(GameObject gameObject, int slot, Buffer constantBuffer)
        {
            AddGameObjectIfNotExists(gameObject);
            perObjectShaderResources[gameObject].ConstantBuffers.Add(slot, constantBuffer);
        }

        /// <summary>
        /// Sampler
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="slot"></param>
        /// <param name="sampler"></param>
        public void SetSampler(GameObject gameObject, int slot, SamplerState sampler)
        {
            AddGameObjectIfNotExists(gameObject);
            perObjectShaderResources[gameObject].Samplers.Add(slot, sampler);
        }

        /// <summary>
        /// SRV
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="slot"></param>
        /// <param name="resourceView"></param>
        public void SetShaderResource(GameObject gameObject, int slot, ShaderResourceView resourceView)
        {
            AddGameObjectIfNotExists(gameObject);
            perObjectShaderResources[gameObject].ResourceViews.Add(slot, resourceView);
        }

        #endregion

    }

}