using System;
using System.Collections.Generic;
using System.Linq;
using RaymarchEngine.Geometry;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Buffer = SharpDX.Direct3D11.Buffer;
using Vector2 = System.Numerics.Vector2;
using Vector4 = System.Numerics.Vector4;

namespace RaymarchEngine.Core
{
    /// <summary>
    /// An indexed triangle mesh on the GPU. The engine only builds one, the fullscreen quad the
    /// raymarch pixel shader runs on.
    /// </summary>
    public class Mesh : IDisposable
    {
        /// <summary>
        /// Vertices, laid out as RenderVertex
        /// </summary>
        public Buffer VertexBuffer { get; private set; }

        /// <summary>
        /// Triangle indices, one 32 bit index per vertex reference
        /// </summary>
        public Buffer IndexBuffer { get; private set; }

        /// <summary>
        /// Stride of one vertex in bytes
        /// </summary>
        public int VertexSize { get; private set; }

        /// <summary>
        /// Ranges of the index buffer that are drawn together, each with its own material.
        /// A quad has exactly one.
        /// </summary>
        public List<Material> SubSets { get; private set; }

        private Mesh()
        {
            SubSets = new List<Material>();
        }

        /// <summary>
        /// Binds the buffers and draws the first subset as a triangle list
        /// </summary>
        public void Draw()
        {
            Engine.RenderDevice.deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            Engine.RenderDevice.deviceContext.InputAssembler.SetVertexBuffers(0,
                new VertexBufferBinding(VertexBuffer, VertexSize, 0));
            Engine.RenderDevice.deviceContext.InputAssembler.SetIndexBuffer(IndexBuffer, Format.R32_UInt, 0);
            Engine.RenderDevice.deviceContext.DrawIndexed(SubSets[0].IndexCount, 0, 0);
        }


        /// <summary>
        /// Creates the unit quad the raymarch shader is drawn on. Its texture coordinates span
        /// 0 to 1, which is what the pixel shader turns into a ray direction.
        /// </summary>
        /// <returns>A two triangle quad ready to draw</returns>
        public static Mesh CreateQuad()
        {
            RenderVertex[] vertices = new RenderVertex[]
            {
                new RenderVertex(new Vector4(0, 0, 1, 1), new Vector2(0, 1)),
                new RenderVertex(new Vector4(0, 0, 0, 1), new Vector2(0, 0)),
                new RenderVertex(new Vector4(1, 0, 1, 1), new Vector2(1, 1)),
                new RenderVertex(new Vector4(1, 0, 0, 1), new Vector2(1, 0))
            };

            int[] indices = new int[] {0, 2, 1, 2, 3, 1};

            Mesh mesh = new Mesh();
            mesh.VertexBuffer =
                Buffer.Create(Engine.RenderDevice.device, BindFlags.VertexBuffer, vertices.ToArray());
            mesh.IndexBuffer = Buffer.Create(Engine.RenderDevice.device, BindFlags.IndexBuffer, indices.ToArray());
            mesh.VertexSize = Utilities.SizeOf<RenderVertex>();

            mesh.SubSets.Add(new Material()
            {
                DiffuseColor = new Vector4(1, 1, 1, 1),
                IndexCount = indices.Count()
            });

            return mesh;
        }

        /// <summary>
        /// Releases the vertex and index buffers and any textures the subsets hold
        /// </summary>
        public void Dispose()
        {
            VertexBuffer.Dispose();
            IndexBuffer.Dispose();
            foreach (Material s in SubSets)
            {
                s.DiffuseMap?.Dispose();
                s.NormalMap?.Dispose();
            }
        }
    }
}