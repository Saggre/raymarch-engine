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

        private Dictionary<GameObject, PerObjectResources> perObjectShaderResources;

        public SharedShader(InputLayout inputLayout, VertexShader vertexShader, HullShader hullShader, DomainShader domainShader,
            GeometryShader geometryShader, PixelShader pixelShader) : base(inputLayout, vertexShader, hullShader, domainShader, geometryShader, pixelShader)
        {
            perObjectShaderResources = new Dictionary<GameObject, PerObjectResources>();
        }

        public Dictionary<GameObject, PerObjectResources> PerObjectShaderResources => perObjectShaderResources;

        #region Implements

        /// <summary>
        /// Add a GameObject to Per-object shader resources list if it already isn't there
        /// </summary>
        /// <param name="gameObject"></param>
        private void AddGameObjectIfNotExists(GameObject gameObject)
        {
            if (!perObjectShaderResources.ContainsKey(gameObject))
            {
                perObjectShaderResources.Add(gameObject, new PerObjectResources());
            }
        }

        /// <summary>
        /// Get all ShaderResourceViews attached to a GameObject
        /// </summary>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        public new Dictionary<int, ShaderResourceView> ShaderResourceViews(GameObject gameObject)
        {
            return perObjectShaderResources.ContainsKey(gameObject) ? perObjectShaderResources[gameObject].ShaderResourceViews : null;
        }

        /// <summary>
        /// Get all Buffers attached to a GameObject
        /// </summary>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        public new Dictionary<int, Buffer> ConstantBuffers(GameObject gameObject)
        {
            return perObjectShaderResources.ContainsKey(gameObject) ? perObjectShaderResources[gameObject].ConstantBuffers : null;
        }

        /// <summary>
        /// Add a per-object buffer to the shader, or update it if it already exists.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="slot"></param>
        /// <param name="constantBuffer"></param>
        public void SetConstantBuffer(GameObject gameObject, int slot, Buffer constantBuffer)
        {
            // Check if the gameObject is already in resource list
            AddGameObjectIfNotExists(gameObject);

            // Check if a buffer in this slot exists already
            if (perObjectShaderResources[gameObject].ConstantBuffers.ContainsKey(slot))
            {
                perObjectShaderResources[gameObject].ConstantBuffers[slot].Dispose();
                perObjectShaderResources[gameObject].ConstantBuffers[slot] = constantBuffer;
            }
            else
            {
                perObjectShaderResources[gameObject].ConstantBuffers.Add(slot, constantBuffer);
            }
        }

        /// <summary>
        /// SRV
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="slot"></param>
        /// <param name="resourceView"></param>
        public void SetShaderResource(GameObject gameObject, int slot, ShaderResourceView resourceView)
        {
            // TODO make shader resources updateable

            AddGameObjectIfNotExists(gameObject);
            perObjectShaderResources[gameObject].ShaderResourceViews.Add(slot, resourceView);
        }

        #endregion

    }

}