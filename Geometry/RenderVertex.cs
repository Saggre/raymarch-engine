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
        public Vector3 position;
        public Vector2 textureCoordinate;
        public Color color;
        public Vector3 normal;

        public static InputElement[] InputElements => new []
        {
            new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0, InputClassification.PerVertexData, 0),
            new InputElement("TEXCOORD", 0, Format.R32G32_Float, 12, 0, InputClassification.PerVertexData, 0),
            new InputElement("COLOR", 0, Format.R32G32B32A32_Float, 20, 0, InputClassification.PerVertexData, 0),
            new InputElement("NORMAL", 0, Format.R32G32B32_Float, 36 * 3, 0, InputClassification.PerVertexData, 0)
        };
        
        public RenderVertex(Vector3 position, Vector2 textureCoordinate, Color color, Vector3 normal)
        {
            this.position = position;
            this.textureCoordinate = textureCoordinate;
            this.color = color;
            this.normal = normal;
        }
    }
}