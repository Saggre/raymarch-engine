// Created by Sakri Koskimies (Github: Saggre) on 01/10/2019

using System.Drawing;
using System.Runtime.InteropServices;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System.Numerics;

namespace EconSim.Geometry
{
  [StructLayout(LayoutKind.Sequential)]
  public struct RenderVertex
  {
    public Vector4 position;
    public Vector2 texCoord;
    //public Color color;
    //public Vector3 normal;

    public static InputElement[] InputElements => new[]
    {
      new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0, 0),
      new InputElement("TEXCOORD",0, Format.R32G32_Float, 16, 0),
      //new InputElement("COLOR", 0, Format.R32G32B32A32_Float, 24, 0),
      //new InputElement("NORMAL", 0, Format.R32G32B32_Float, 40, 0)
    };

    public RenderVertex(Vector4 position, Vector2 texCoord)
    {
      this.position = position;
      this.texCoord = texCoord;
      //this.color = color;
      //this.normal = normal;
    }
  }
}