// Created by Sakri Koskimies (Github: Saggre) on 01/10/2019

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EconSim.Terrain
{
    struct TerrainRenderVertex : IVertexType
    {
        public Vector3 position;
        public Vector2 textureCoordinate;
        public Color color;
        public Vector3 normal;

        public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration(
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(sizeof(float) * 3, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
            new VertexElement(sizeof(float) * 5, VertexElementFormat.Color, VertexElementUsage.Color, 0),
            new VertexElement(sizeof(float) * 5 + 4, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0)
            );
        
        VertexDeclaration IVertexType.VertexDeclaration
        {
            get { return TerrainRenderVertex.VertexDeclaration; }
        }

        public Vector3 Position
        {
            get => position;
            set => position = value;
        }

        public Vector2 TextureCoordinate
        {
            get => textureCoordinate;
            set => textureCoordinate = value;
        }

        public Color Color
        {
            get => color;
            set => color = value;
        }

        public Vector3 Normal
        {
            get => normal;
            set => normal = value;
        }
    }
}