using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using EconSim.Geometry;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Buffer11 = SharpDX.Direct3D11.Buffer;
using Vector2 = System.Numerics.Vector2;
using Vector4 = System.Numerics.Vector4;

namespace EconSim.Core
{
    /// <summary>
    /// To Render Static Object
    /// </summary>
    public class Mesh : IDisposable
    {

        /// <summary>
        /// Vertex Buffer
        /// </summary>
        public Buffer11 VertexBuffer { get; private set; }

        /// <summary>
        /// Index Buffer
        /// </summary>
        public Buffer11 IndexBuffer { get; private set; }

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
            EconSim.d3dDeviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            EconSim.d3dDeviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(VertexBuffer, VertexSize, 0));
            EconSim.d3dDeviceContext.InputAssembler.SetIndexBuffer(IndexBuffer, Format.R32_UInt, 0);
            EconSim.d3dDeviceContext.DrawIndexed(SubSets[0].IndexCount, 0, 0);
        }


        /// <summary>
        /// Draw Mesh
        /// </summary>
        /// <param name="subset">Subsets</param>
        public void Draw(int subset)
        {
            EconSim.d3dDeviceContext.DrawIndexed(SubSets[subset].IndexCount, SubSets[subset].StartIndex, 0);
        }

        /// <summary>
        /// Create From Vertices and Indices array
        /// </summary>
        /// <typeparam name="VType">Vertex Type</typeparam>
        /// <param name="device">Device</param>
        /// <param name="vertices">Vertices</param>
        /// <param name="indices">Indices</param>
        /// <returns>Mesh</returns>
        public static Mesh Create(RenderVertex[] vertices, int[] indices)
        {
            Mesh mesh = new Mesh();
            mesh.VertexBuffer = Buffer11.Create(EconSim.d3dDevice, BindFlags.VertexBuffer, vertices);
            mesh.IndexBuffer = Buffer11.Create(EconSim.d3dDevice, BindFlags.IndexBuffer, indices);
            mesh.VertexSize = SharpDX.Utilities.SizeOf<RenderVertex>();
            mesh.SubSets.Add(new Material()
            {
                DiffuseColor = new Vector4(1, 1, 1, 1),
                IndexCount = indices.Count()
            });
            return mesh;
        }

        /*
        /// <summary>
        /// Create a mesh from wavefront obj file format
        /// </summary>
        /// <param name="device">Device</param>
        /// <param name="filename">Filename</param>
        /// <returns>Mesh</returns>
        public static SharpMesh CreateFromObj(string filename)
        {
            SharpMesh mesh = new SharpMesh();

            WaveFrontModel[] modelParts = WaveFrontModel.CreateFromObj(filename);
            mesh.SubSets = new List<SharpSubSet>();

            List<StaticVertex> vertices = new List<StaticVertex>();
            List<int> indices = new List<int>();

            int vcount = 0;
            int icount = 0;
            foreach (WaveFrontModel model in modelParts)
            {
                vertices.AddRange(model.VertexData);
                indices.AddRange(model.IndexData.Select(i => i + vcount));

                var mate = model.MeshMaterial.First();


                ShaderResourceView tex = null;
                if (!string.IsNullOrEmpty(mate.DiffuseMap))
                {
                    string textureFile = System.IO.Path.GetDirectoryName(filename) + "\\" + System.IO.Path.GetFileName(mate.DiffuseMap);
                    tex = device.LoadTextureFromFile(textureFile);
                }

                mesh.SubSets.Add(new SharpSubSet()
                {
                    IndexCount = model.IndexData.Count,
                    StartIndex = icount,
                    DiffuseMap = tex
                });



                vcount += model.VertexData.Count;
                icount += model.IndexData.Count;
            }

            mesh.VertexBuffer = Buffer11.Create<StaticVertex>(EconSim.d3dDevice, BindFlags.VertexBuffer, vertices.ToArray());
            mesh.IndexBuffer = Buffer11.Create(EconSim.d3dDevice, BindFlags.IndexBuffer, indices.ToArray());
            mesh.VertexSize = SharpDX.Utilities.SizeOf<StaticVertex>();

            return mesh;
        }


        /// <summary>
        /// Create a mesh from wavefront obj file format using Tangent and Binormal vertex format
        /// </summary>
        /// <param name="device">Device</param>
        /// <param name="filename">Filename</param>
        /// <returns>Mesh</returns>
        public static SharpMesh CreateNormalMappedFromObj(string filename)
        {
            SharpMesh mesh = new SharpMesh();

            WaveFrontModel[] modelParts = WaveFrontModel.CreateFromObj(filename);
            mesh.SubSets = new List<SharpSubSet>();

            List<TangentVertex> vertices = new List<TangentVertex>();
            List<int> indices = new List<int>();

            int vcount = 0;
            int icount = 0;
            foreach (WaveFrontModel model in modelParts)
            {
                vertices.AddRange(model.TangentData);
                indices.AddRange(model.IndexData.Select(i => i + vcount));

                var mate = model.MeshMaterial.First();


                ShaderResourceView tex = null;
                ShaderResourceView ntex = null;

                if (!string.IsNullOrEmpty(mate.DiffuseMap))
                {
                    string textureFile = Path.GetDirectoryName(filename) + "\\" + Path.GetFileName(mate.DiffuseMap);
                    tex = device.LoadTextureFromFile(textureFile);

                    string normalMap = Path.GetDirectoryName(textureFile) + "\\" + Path.GetFileNameWithoutExtension(textureFile) + "N" + Path.GetExtension(textureFile);
                    ntex = device.LoadTextureFromFile(normalMap);
                }

                mesh.SubSets.Add(new SharpSubSet()
                {
                    IndexCount = model.IndexData.Count,
                    StartIndex = icount,
                    DiffuseMap = tex,
                    NormalMap = ntex
                });

                vcount += model.VertexData.Count;
                icount += model.IndexData.Count;
            }

            mesh.VertexBuffer = Buffer11.Create<TangentVertex>(EconSim.d3dDevice, BindFlags.VertexBuffer, vertices.ToArray());
            mesh.IndexBuffer = Buffer11.Create(EconSim.d3dDevice, BindFlags.IndexBuffer, indices.ToArray());
            mesh.VertexSize = SharpDX.Utilities.SizeOf<TangentVertex>();

            return mesh;
        }*/

        /// <summary>
        /// Create a quad for Multiple Render Target
        /// </summary>
        /// <returns>Mesh</returns>
        public static Mesh CreateQuad()
        {
            RenderVertex[] vertices = new RenderVertex[]
            {
                new RenderVertex( new Vector4(0, 0, 1,1),new Vector2(0,1)),
                new RenderVertex(new Vector4(0, 0, 0,1),new Vector2(0,0)),
                new RenderVertex(new Vector4(1, 0, 1,1),new Vector2(1,1)),
                new RenderVertex(new Vector4(1, 0, 0,1),new Vector2(1,0))
            };

            int[] indices = new int[] { 0, 2, 1, 2, 3, 1 };

            Mesh mesh = new Mesh();
            mesh.VertexBuffer = Buffer11.Create(EconSim.d3dDevice, BindFlags.VertexBuffer, vertices.ToArray());
            mesh.IndexBuffer = Buffer11.Create(EconSim.d3dDevice, BindFlags.IndexBuffer, indices.ToArray());
            mesh.VertexSize = Utilities.SizeOf<RenderVertex>();

            mesh.SubSets.Add(new Material()
            {
                DiffuseColor = new Vector4(1, 1, 1, 1),
                IndexCount = indices.Count()
            });

            return mesh;
        }

        /// <summary>
        /// Set all buffer and topology property to speed up rendering
        /// </summary>
        public void Begin()
        {
            EconSim.d3dDeviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            EconSim.d3dDeviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(VertexBuffer, VertexSize, 0));
            EconSim.d3dDeviceContext.InputAssembler.SetIndexBuffer(IndexBuffer, Format.R32_UInt, 0);
        }

        /// <summary>
        /// Draw all vertices as points
        /// </summary>
        /// <param name="count"></param>
        public void DrawPoints(int count = int.MaxValue)
        {
            EconSim.d3dDeviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.PointList;
            EconSim.d3dDeviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(VertexBuffer, VertexSize, 0));
            EconSim.d3dDeviceContext.InputAssembler.SetIndexBuffer(IndexBuffer, Format.R32_UInt, 0);
            EconSim.d3dDeviceContext.DrawIndexed(Math.Min(count, SubSets[0].IndexCount), 0, 0);
        }

        /// <summary>
        /// Draw patch
        /// </summary>
        /// <param name="topology">Patch Topology type</param>
        public void DrawPatch(PrimitiveTopology topology)
        {
            EconSim.d3dDeviceContext.InputAssembler.PrimitiveTopology = topology;
            EconSim.d3dDeviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(VertexBuffer, VertexSize, 0));
            EconSim.d3dDeviceContext.InputAssembler.SetIndexBuffer(IndexBuffer, Format.R32_UInt, 0);
            EconSim.d3dDeviceContext.DrawIndexed(SubSets[0].IndexCount, 0, 0);
        }

        /// <summary>
        /// Release resource
        /// </summary>
        public void Dispose()
        {
            VertexBuffer.Dispose();
            IndexBuffer.Dispose();
            foreach (var s in SubSets)
            {
                if (s.DiffuseMap != null)
                    s.DiffuseMap.Dispose();

                if (s.NormalMap != null)
                    s.NormalMap.Dispose();
            }
        }
    }
}