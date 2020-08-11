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
    public class Shader
    {
        public InputLayout InputLayout { get; }

        public VertexShader VertexShader { get; }

        public HullShader HullShader { get; }

        public DomainShader DomainShader { get; }

        public GeometryShader GeometryShader { get; }

        public PixelShader PixelShader { get; }

        /// <summary>
        /// Resource views for this shader
        /// </summary>
        private Dictionary<int, ShaderResourceView> shaderResourceViews;

        public Shader(InputLayout inputLayout, VertexShader vertexShader, HullShader hullShader,
            DomainShader domainShader,
            GeometryShader geometryShader, PixelShader pixelShader)
        {
            InputLayout = inputLayout;
            VertexShader = vertexShader;
            HullShader = hullShader;
            DomainShader = domainShader;
            GeometryShader = geometryShader;
            PixelShader = pixelShader;

            shaderResourceViews = new Dictionary<int, ShaderResourceView>();
        }


        #region Implements

        /// <summary>
        /// Get all resource views attached to this shader
        /// </summary>
        /// <returns></returns>
        public Dictionary<int, ShaderResourceView> ResourceViews()
        {
            return shaderResourceViews;
        }

        /// <summary>
        /// Attach a resource view to this shader, or update it if it already exists
        /// </summary>
        /// <param name="slot"></param>
        /// <param name="resourceView"></param>
        public void AddShaderResource(int slot, ShaderResourceView resourceView)
        {
            // TODO check if exists
            shaderResourceViews.Add(slot, resourceView);

            SendResourceViewToShader(slot, resourceView);
        }

        #endregion

        /// <summary>
        /// Send the buffer to all shader stages
        /// </summary>
        /// <param name="slot"></param>
        /// <param name="constantBuffer"></param>
        public void SendBufferToShader(int slot, Buffer constantBuffer)
        {
            Engine.RenderDevice.deviceContext.VertexShader.SetConstantBuffer(slot, constantBuffer);
            Engine.RenderDevice.deviceContext.HullShader.SetConstantBuffer(slot, constantBuffer);
            Engine.RenderDevice.deviceContext.DomainShader.SetConstantBuffer(slot, constantBuffer);
            Engine.RenderDevice.deviceContext.GeometryShader.SetConstantBuffer(slot, constantBuffer);
            Engine.RenderDevice.deviceContext.PixelShader.SetConstantBuffer(slot, constantBuffer);
        }

        /// <summary>
        /// Send the shader resource view to all shader stages
        /// </summary>
        /// <param name="slot"></param>
        /// <param name="resourceView"></param>
        public void SendResourceViewToShader(int slot, ShaderResourceView resourceView)
        {
            // TODO ability to send selectively to only certain stages
            Engine.RenderDevice.deviceContext.VertexShader.SetShaderResource(slot, resourceView);
            Engine.RenderDevice.deviceContext.HullShader.SetShaderResource(slot, resourceView);
            Engine.RenderDevice.deviceContext.DomainShader.SetShaderResource(slot, resourceView);
            Engine.RenderDevice.deviceContext.GeometryShader.SetShaderResource(slot, resourceView);
            Engine.RenderDevice.deviceContext.PixelShader.SetShaderResource(slot, resourceView);
        }

        /// <summary>
        /// Compiles files into shader byte-code and creates a shader from the shader files that exist
        /// </summary>
        /// <param name="folderPath"></param>
        public static Shader CompileFromFiles(string folderPath)
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
                    vertexShader = new VertexShader(Engine.RenderDevice.device, byteCode);

                    ShaderSignature inputSignature = ShaderSignature.GetInputSignature(byteCode);
                    inputLayout = new InputLayout(Engine.RenderDevice.device, inputSignature,
                        RenderVertex.InputElements);
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
                    hullShader = new HullShader(Engine.RenderDevice.device, byteCode);
                }
            }

            // Domain shader
            {
                string path = Path.Combine(folderPath, "Domain.hlsl");

                if (File.Exists(path))
                {
                    CompilationResult byteCode = ShaderBytecode.CompileFromFile(path, "main", "ds_5_0", shaderFlags,
                        EffectFlags.None, null, includeHandler);
                    domainShader = new DomainShader(Engine.RenderDevice.device, byteCode);
                }
            }

            // Geometry shader
            {
                string path = Path.Combine(folderPath, "Geometry.hlsl");

                if (File.Exists(path))
                {
                    CompilationResult byteCode = ShaderBytecode.CompileFromFile(path, "main", "gs_5_0", shaderFlags,
                        EffectFlags.None, null, includeHandler);
                    geometryShader = new GeometryShader(Engine.RenderDevice.device, byteCode);
                }
            }

            // Pixel shader
            {
                string path = Path.Combine(folderPath, "Pixel.hlsl");

                if (File.Exists(path))
                {
                    CompilationResult byteCode = ShaderBytecode.CompileFromFile(path, "main", "ps_5_0", shaderFlags,
                        EffectFlags.None, null, includeHandler);
                    pixelShader = new PixelShader(Engine.RenderDevice.device, byteCode);
                }
            }

            return new Shader(inputLayout, vertexShader, hullShader, domainShader, geometryShader, pixelShader);
        }
    }
}