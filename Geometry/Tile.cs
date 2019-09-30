// Created by Sakri Koskimies (Github: Saggre) on 29/09/2019

using System;
using System.Collections.Generic;
using EconSim.Terrain;
using Microsoft.Xna.Framework;

namespace EconSim.Geometry
{
    /// <summary>
    /// Site variation to be used in shader computations. Position should be normalized to [0,1]
    /// </summary>
    public struct STile
    {
        public int terrainType;
        public Vector2 position;

        public STile(Tile tile, SquareRect bounds)
        {
            terrainType = (int)tile.TerrainType;
            position = new Vector2((tile.X - bounds.X) / bounds.Size, (tile.X - bounds.X) / bounds.Size);
        }

        public static int Bytes()
        {
            return 12;
        }
    }

    /// <summary>
    /// Tile helpers
    /// </summary>
    public static class TileHelpers
    {
        /// <summary>
        /// Returns the struct representation of this Site for shader programs
        /// </summary>
        /// <returns></returns>
        public static STile AsStruct(this Tile tile, SquareRect bounds)
        {
            return new STile(tile, bounds);
        }
    }

    /// <summary>
    /// Represents a rectangle in 2D space. Position is top-left.
    /// </summary>
    public class Tile
    {
        private Vector2 position;
        private Vector2 centroid;
        private CardinalCollection<Vertex> vertices;
        private CardinalCollection<Edge> edges;
        private CardinalCollection<Tile> neighbors;
        private Util.TerrainType terrainType;
        private Util.BiomeType biomeType;

        public Tile()
        {
            Init();
        }

        public Tile(int x, int y)
        {
            position = new Vector2(x, y);
            Init();
        }

        private void Init()
        {
            vertices = new CardinalCollection<Vertex>();
            edges = new CardinalCollection<Edge>();
            neighbors = new CardinalCollection<Tile>();
        }

        /// <summary>
        /// Returns the percentage of vertices that are of aquatic type
        /// </summary>
        /// <returns></returns>
        public float AquaticPercentage()
        {
            var total = 0f;

            foreach (var vertex in vertices)
            {
                if (vertex.TerrainType.IsAquatic())
                {
                    total += 0.25f;
                }
            }

            return total;
        }

        /// <summary>
        /// Returns a list of terrain types of tiles connected to this tile
        /// </summary>
        /// <returns></returns>
        public HashSet<Util.TerrainType> ConnectedTileTerrainTypes()
        {
            var terrainTypes = new HashSet<Util.TerrainType>();

            foreach (var tile in neighbors)
            {
                terrainTypes.Add(tile.TerrainType);
            }

            return terrainTypes;
        }

        /// <summary>
        /// Returns connected vertices average moisture
        /// </summary>
        /// <returns></returns>
        public float GetMoisture()
        {
            var total = 0f;

            foreach (var vertex in vertices)
            {
                total += vertex.Moisture;
            }

            return total / 4;
        }

        /// <summary>
        /// Returns connected vertices average elevation
        /// </summary>
        /// <returns></returns>
        public float GetElevation()
        {
            var total = 0f;

            foreach (var vertex in vertices)
            {
                total += vertex.Elevation;
            }

            return total / 4;
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

        public Vector2 Centroid => new Vector2(X + 0.5f, Y + 0.5f);

        public CardinalCollection<Vertex> Vertices
        {
            get => vertices;
            set => vertices = value;
        }

        public CardinalCollection<Edge> Edges
        {
            get => edges;
            set => edges = value;
        }

        public CardinalCollection<Tile> Neighbors
        {
            get => neighbors;
            set => neighbors = value;
        }

        public Util.TerrainType TerrainType
        {
            get => terrainType;
            set => terrainType = value;
        }

        public Util.BiomeType BiomeType
        {
            get => biomeType;
            set => biomeType = value;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}