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
    /// To Render Static Object
    /// </summary>
    public class Mesh : IDisposable
    {
        /// <summary>
        /// Vertex Buffer
        /// </summary>
        public Buffer VertexBuffer { get; private set; }

        /// <summary>
        /// Index Buffer
        /// </summary>
        public Buffer IndexBuffer { get; private set; }

        /// <summary>
        /// Vertex Size
        /// </summary>
        public int VertexSize { get; private set; }

        /// <summary>
        /// Mesh Parts
        /// Like material in Unity?
        /// </summary>
        public List<Material> SubSets { get; private set; }

        private Mesh()
        {
            SubSets = new List<Material>();
        }

        /// <summary>
        /// Draw Mesh
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
        /// Create a quad for Multiple Render Target
        /// </summary>
        /// <returns>Mesh</returns>
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
        /// Release resource
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