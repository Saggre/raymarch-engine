// Created by Sakri Koskimies (Github: Saggre) on 25/10/2019

using System;
using System.Collections.Generic;
using System.IO;
using RaymarchEngine.Geometry;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace RaymarchEngine.Core
{
    /// <summary>
    /// The compiled shader stages for one shader folder, plus the resource views bound alongside
    /// them. Stages whose file does not exist are null, so a raymarch shader is a vertex and a
    /// pixel stage with the rest left empty.
    /// </summary>
    public class Shader : IDisposable
    {
        /// <summary>
        /// Vertex layout built from the vertex stage's input signature
        /// </summary>
        public InputLayout InputLayout { get; }

        /// <summary>
        /// Compiled vertex stage
        /// </summary>
        public VertexShader VertexShader { get; }

        /// <summary>
        /// Compiled hull stage, null when the folder has no Hull.hlsl
        /// </summary>
        public HullShader HullShader { get; }

        /// <summary>
        /// Compiled domain stage, null when the folder has no Domain.hlsl
        /// </summary>
        public DomainShader DomainShader { get; }

        /// <summary>
        /// Compiled geometry stage, null when the folder has no Geometry.hlsl
        /// </summary>
        public GeometryShader GeometryShader { get; }

        /// <summary>
        /// Compiled pixel stage
        /// </summary>
        public PixelShader PixelShader { get; }

        /// <summary>
        /// Resource views for this shader
        /// </summary>
        private readonly Dictionary<int, ShaderResourceView> shaderResourceViews;

        /// <summary>
        /// Takes ownership of already compiled stages. Use CompileFromFiles to build them.
        /// </summary>
        /// <param name="inputLayout">Vertex layout matching the vertex stage</param>
        /// <param name="vertexShader">Compiled vertex stage</param>
        /// <param name="hullShader">Compiled hull stage, or null</param>
        /// <param name="domainShader">Compiled domain stage, or null</param>
        /// <param name="geometryShader">Compiled geometry stage, or null</param>
        /// <param name="pixelShader">Compiled pixel stage</param>
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
        /// <returns>The live dictionary of views, keyed by register slot</returns>
        public Dictionary<int, ShaderResourceView> ResourceViews()
        {
            return shaderResourceViews;
        }

        /// <summary>
        /// Attach a resource view to this shader, or update it if it already exists, and bind it
        /// straight away
        /// </summary>
        /// <param name="slot">Register slot, t0 upwards</param>
        /// <param name="resourceView">View to bind</param>
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
        /// <param name="slot">Constant buffer register slot, b0 upwards</param>
        /// <param name="constantBuffer">Buffer to bind</param>
        public void SendBufferToShader(int slot, Buffer constantBuffer)
        {
            Engine.RenderDevice.deviceContext.VertexShader.SetConstantBuffer(slot, constantBuffer);
            Engine.RenderDevice.deviceContext.HullShader.SetConstantBuffer(slot, constantBuffer);
            Engine.RenderDevice.deviceContext.DomainShader.SetConstantBuffer(slot, constantBuffer);
            Engine.RenderDevice.deviceContext.GeometryShader.SetConstantBuffer(slot, constantBuffer);
            Engine.RenderDevice.deviceContext.PixelShader.SetConstantBuffer(slot, constantBuffer);
        }

        /// <summary>
        /// Bind the shader resource view to the vertex and pixel stages, the only stages this
        /// engine uses
        /// </summary>
        /// <param name="slot">Register slot, t0 upwards</param>
        /// <param name="resourceView">View to bind</param>
        private void SendResourceViewToShader(int slot, ShaderResourceView resourceView)
        {
            Engine.RenderDevice.deviceContext.VertexShader.SetShaderResource(slot, resourceView);
            Engine.RenderDevice.deviceContext.PixelShader.SetShaderResource(slot, resourceView);
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
            // TODO simplify method with a loop
            // TODO build shaders on program build. Not with dxc though: it dropped HLSL interfaces,
            // which Common.hlsl's primitive system is built on.

            // Debug bytecode is unoptimized, so Release was losing the whole shader optimizer
#if DEBUG
            ShaderFlags shaderFlags = ShaderFlags.Debug | ShaderFlags.SkipOptimization;
#else
            ShaderFlags shaderFlags = ShaderFlags.OptimizationLevel3;
#endif

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