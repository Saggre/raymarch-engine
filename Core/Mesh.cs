using EconSim.Geometry;
using SharpDX.Direct3D11;

namespace EconSim.Core
{
    public class Mesh
    {
        private RenderVertex[] vertices;
        private Buffer vertexBuffer;

        public Mesh(RenderVertex[] vertices)
        {
            this.vertices = vertices;
        }

        public RenderVertex[] Vertices
        {
            get => vertices;
            set => vertices = value;
        }

        /// <summary>
        /// Returns a vertex buffer containing the vertices of this mesh. Creates one if one doesn't exist.
        /// </summary>
        /// <returns></returns>
        public Buffer GetVertexBuffer()
        {
            // Check if vertexBuffer needs to be updated
            if (vertexBuffer == null)
            {
                vertexBuffer = Buffer.Create(EconSim.d3dDevice, BindFlags.VertexBuffer, vertices);
            }

            return vertexBuffer;
        }
    }
}
