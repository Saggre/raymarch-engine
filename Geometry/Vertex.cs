// Created by Sakri Koskimies (Github: Saggre) on 30/09/2019

using System.Collections.Generic;
using EconSim.Terrain;
using Microsoft.Xna.Framework;

namespace EconSim.Geometry
{
    /// <summary>
    /// Vertex variation to be used in shader computations. Position should be normalized to [0,1]
    /// </summary>
    public struct SVertex
    {
        public float elevation;
        public float moisture;
        public Vector2 position;

        public SVertex(Vertex vertex, SquareRect bounds)
        {
            float scale = bounds.Size;
            elevation = (vertex.Elevation + 1) / 2.0f; // Transform [-1,1] into [0,1]
            moisture = vertex.Moisture;
            position = new Vector2((vertex.X - bounds.X) / scale, (vertex.Y - bounds.Y) / scale);
        }

        public static int Bytes()
        {
            return 16;
        }
    }

    /// <summary>
    /// Vertex helpers
    /// </summary>
    public static class VertexHelpers
    {
        /// <summary>
        /// Returns the struct representation of this Vertex for shader programs
        /// </summary>
        /// <returns></returns>
        public static SVertex AsStruct(this Vertex vertex, SquareRect bounds)
        {
            return new SVertex(vertex, bounds);
        }
    }

    public class Vertex
    {
        private Vector2 position;
        //private CardinalCollection<Edge> connectedEdges;
        private CardinalCollection<Tile> connectedTiles;
        private Util.TerrainType terrainType;
        private float moisture;
        private float elevation;

        public Vertex()
        {
            Init();
        }

        public Vertex(Vector2 position)
        {
            this.position = position;
            Init();
        }

        public Vertex(float x, float y)
        {
            position = new Vector2(x, y);
            Init();
        }

        private void Init()
        {

            //connectedEdges = new CardinalCollection<Edge>();
            connectedTiles = new CardinalCollection<Tile>();
        }

        /// <summary>
        /// Returns a list of terrain types of tiles connected to this vertex
        /// </summary>
        /// <returns></returns>
        public List<Util.TerrainType> ConnectedTileTerrainTypes()
        {
            List<Util.TerrainType> terrainTypes = new List<Util.TerrainType>();

            foreach (Tile tile in ConnectedTiles)
            {
                terrainTypes.Add(tile.TerrainType);
            }

            return terrainTypes;
        }

        public Util.TerrainType TerrainType
        {
            get => terrainType;
            set => terrainType = value;
        }

        public float Moisture
        {
            get => moisture;
            set => moisture = value;
        }

        public float Elevation
        {
            get => elevation;
            set => elevation = value;
        }

        /*public CardinalCollection<Edge> ConnectedEdges
        {
            get => connectedEdges;
            set => connectedEdges = value;
        }*/

        public CardinalCollection<Tile> ConnectedTiles
        {
            get => connectedTiles;
            set => connectedTiles = value;
        }

        public float X
        {
            get => position.X;
            set => position.X = value;
        }

        public float Y
        {
            get => position.Y;
            set => position.Y = value;
        }

        public Vector2 Position
        {
            get => position;
            set => position = value;
        }
    }
}