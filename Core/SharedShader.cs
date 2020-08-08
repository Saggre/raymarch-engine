// Created by Sakri Koskimies (Github: Saggre) on 25/10/2019

using System.Collections.Generic;
using System.IO;
using EconSim.Geometry;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;

namespace EconSim.Core
{
    /// <summary>
    /// A shader class combining different shader stages. Extends CommonShaderStage to add things such as buffers to all shader stages.
    /// SharedShader saves different buffers per-object and switches between them. This enables two objects to use the same shader, but with different textures for example. (TODO)
    /// </summary>
    public class SharedShader : IShader
    {
        public InputLayout InputLayout { get; }

        public VertexShader VertexShader { get; }

        public HullShader HullShader { get; }

        public DomainShader DomainShader { get; }

        public GeometryShader GeometryShader { get; }

        public PixelShader PixelShader { get; }

        private Dictionary<GameObject, PerObjectResources> perObjectShaderResources;

        public SharedShader(InputLayout inputLayout, VertexShader vertexShader, HullShader hullShader, DomainShader domainShader,
            GeometryShader geometryShader, PixelShader pixelShader)
        {
            InputLayout = inputLayout;
            VertexShader = vertexShader;
            HullShader = hullShader;
            DomainShader = domainShader;
            GeometryShader = geometryShader;
            PixelShader = pixelShader;

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
        public Dictionary<int, ShaderResourceView> ShaderResourceViews(GameObject gameObject)
        {
            return perObjectShaderResources.ContainsKey(gameObject) ? perObjectShaderResources[gameObject].ShaderResourceViews : new Dictionary<int, ShaderResourceView>();
        }

        /// <summary>
        /// Get all Buffers attached to a GameObject
        /// </summary>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        public Dictionary<int, Buffer> ConstantBuffers(GameObject gameObject)
        {
            return perObjectShaderResources.ContainsKey(gameObject) ? perObjectShaderResources[gameObject].ConstantBuffers : new Dictionary<int, Buffer>();
        }

        /// <summary>
        /// Add a per-object buffer to the shader, or update it if it already exists
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="slot"></param>
        /// <param name="constantBuffer"></param>
        public void SetConstantBuffer(GameObject gameObject, int slot, Buffer constantBuffer)
        {
            // Check if the gameObject is already in resource list
            AddGameObjectIfNotExists(gameObject);

            // Check if a buffer at this slot exists already
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
        /// Add a per-object resource view to the shader, or update it if it already exists
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="slot"></param>
        /// <param name="resourceView"></param>
        public void SetShaderResource(GameObject gameObject, int slot, ShaderResourceView resourceView)
        {
            AddGameObjectIfNotExists(gameObject);

            // Check if a resource at this slot exists already
            if (perObjectShaderResources[gameObject].ShaderResourceViews.ContainsKey(slot))
            {
                perObjectShaderResources[gameObject].ShaderResourceViews[slot].Dispose();
                perObjectShaderResources[gameObject].ShaderResourceViews[slot] = resourceView;
            }
            else
            {
                perObjectShaderResources[gameObject].ShaderResourceViews.Add(slot, resourceView);
            }
        }

        #endregion

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

        /// <summary>
        /// Compiles files into shader byte-code and creates a shader from the shader files that exist
        /// </summary>
        /// <param name="folderPath"></param>
        public static SharedShader CompileFromFiles(string folderPath)
        {
            // TODO simplify method with a loop
            // TODO build shaders on program build with dxc

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

    }

}