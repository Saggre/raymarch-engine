// Created by Sakri Koskimies (Github: Saggre) on 25/10/2019

using System;
using System.Collections.Generic;
using System.IO;
using RaymarchEngine.Geometry;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;

namespace RaymarchEngine.Core
{
    /// <summary>
    /// A shader class combining different shader stages. Extends CommonShaderStage to add things such as buffers to all shader stages.
    /// SharedShader saves different buffers per-object and switches between them. This enables two objects to use the same shader, but with different textures for example. (TODO)
    /// </summary>
    public class Shader : IDisposable
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
        private readonly Dictionary<int, ShaderResourceView> shaderResourceViews;

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
            if (shaderResourceViews.ContainsKey(slot))
            {
                shaderResourceViews[slot] = resourceView;
            }
            else
            {
                shaderResourceViews.Add(slot, resourceView);
            }

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
        private void SendResourceViewToShader(int slot, ShaderResourceView resourceView)
        {
            Engine.RenderDevice.deviceContext.VertexShader.SetShaderResource(slot, resourceView);
            Engine.RenderDevice.deviceContext.PixelShader.SetShaderResource(slot, resourceView);
        }

        /// <summary>
        /// Bytecode for one stage, from the cache when it is there and from the compiler when it
        /// is not. Returns null when the folder has no file for this stage.
        /// </summary>
        private static byte[] GetStageBytecode(string folderPath, string fileName, string profile,
            ShaderFlags shaderFlags, HLSLFileIncludeHandler includeHandler)
        {
            string path = Path.Combine(folderPath, fileName);
            if (!File.Exists(path))
            {
                return null;
            }

            string identity = fileName + ":" + profile + ":" + (int) shaderFlags;

            return ShaderCache.GetOrCompile(folderPath, identity, includeHandler.GetShaderConstants(),
                () => ShaderBytecode
                    .CompileFromFile(path, "main", profile, shaderFlags, EffectFlags.None, null, includeHandler)
                    .Bytecode.Data);
        }

        /// <summary>
        /// Compiles files into shader byte-code and creates a shader from the shader files that exist.
        /// Stages are named after their file, so Vertex.hlsl becomes the vertex stage, and a missing
        /// file leaves that stage null. Debug builds skip optimisation.
        /// </summary>
        /// <param name="folderPath">Folder holding the .hlsl files, also the #include search root</param>
        /// <returns>A shader holding the stages that were found</returns>
        /// <exception cref="SharpDX.CompilationException">A shader file failed to compile</exception>
        public static Shader CompileFromFiles(string folderPath)
        {
            // Debug bytecode is unoptimized, so Release was losing the whole shader optimizer
#if DEBUG
            ShaderFlags shaderFlags = ShaderFlags.Debug | ShaderFlags.SkipOptimization;
#else
            ShaderFlags shaderFlags = ShaderFlags.OptimizationLevel3;
#endif

            Device device = Engine.RenderDevice.device;

            // Handler for #include directive
            HLSLFileIncludeHandler includeHandler = new HLSLFileIncludeHandler(folderPath);

            byte[] vertexBytes = GetStageBytecode(folderPath, "Vertex.hlsl", "vs_5_0", shaderFlags, includeHandler);
            byte[] hullBytes = GetStageBytecode(folderPath, "Hull.hlsl", "hs_5_0", shaderFlags, includeHandler);
            byte[] domainBytes = GetStageBytecode(folderPath, "Domain.hlsl", "ds_5_0", shaderFlags, includeHandler);
            byte[] geometryBytes = GetStageBytecode(folderPath, "Geometry.hlsl", "gs_5_0", shaderFlags, includeHandler);
            byte[] pixelBytes = GetStageBytecode(folderPath, "Pixel.hlsl", "ps_5_0", shaderFlags, includeHandler);

            InputLayout inputLayout = null;
            VertexShader vertexShader = null;

            if (vertexBytes != null)
            {
                vertexShader = new VertexShader(device, vertexBytes);

                // The layout has to agree with what the vertex stage declared it takes, which is
                // why it is built from that stage's signature rather than described separately
                ShaderSignature inputSignature =
                    ShaderSignature.GetInputSignature(ShaderCache.ToShaderBytecode(vertexBytes));

                inputLayout = new InputLayout(device, inputSignature, RenderVertex.InputElements);
            }

            return new Shader(
                inputLayout,
                vertexShader,
                hullBytes != null ? new HullShader(device, hullBytes) : null,
                domainBytes != null ? new DomainShader(device, domainBytes) : null,
                geometryBytes != null ? new GeometryShader(device, geometryBytes) : null,
                pixelBytes != null ? new PixelShader(device, pixelBytes) : null);
        }

        /// <summary>
        /// Releases the shader stages and input layout. They hold references to the D3D device.
        /// </summary>
        public void Dispose()
        {
            InputLayout?.Dispose();
            VertexShader?.Dispose();
            HullShader?.Dispose();
            DomainShader?.Dispose();
            GeometryShader?.Dispose();
            PixelShader?.Dispose();
        }
    }
}