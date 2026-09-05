// Created by Sakri Koskimies (Github: Saggre) on 01/10/2019

using System.Runtime.InteropServices;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System.Numerics;

namespace RaymarchEngine.Geometry
{
  /// <summary>
  /// One vertex of a Mesh, laid out to match the POSITION and TEXCOORD inputs in Vertex.hlsl
  /// </summary>
  [StructLayout(LayoutKind.Sequential)]
  public struct RenderVertex
  {
    /// <summary>
    /// Object space position, w is 1 for a point
    /// </summary>
    public Vector4 position;

    /// <summary>
    /// Texture coordinate, which the raymarch pixel shader reads as its screen position
    /// </summary>
    public Vector2 texCoord;

    /// <summary>
    /// Input layout description for this struct. The byte offsets have to track the fields above.
    /// </summary>
    public static InputElement[] InputElements => new[]
    {
      new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0, 0),
      new InputElement("TEXCOORD",0, Format.R32G32_Float, 16, 0)
    };

    /// <summary>
    /// Creates a vertex
    /// </summary>
    /// <param name="position">Object space position</param>
    /// <param name="texCoord">Texture coordinate</param>
    public RenderVertex(Vector4 position, Vector2 texCoord)
    {
      this.position = position;
      this.texCoord = texCoord;
    }
  }
}