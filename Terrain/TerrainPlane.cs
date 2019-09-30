// Created by Sakri Koskimies (Github: Saggre) on 29/09/2019

using System;
using System.Diagnostics;
using EconSim.Geometry;
using EconSim.Geometry.CardinalCollection;

namespace EconSim.Terrain
{
    public class TerrainPlane
    {
        private Tile[,] tiles;
        private Vertex[,] vertices;
        //private Edge[] edges;
        private int size;

        public TerrainPlane(int size = 128)
        {
            tiles = new Tile[size, size];
            vertices = new Vertex[(size + 1), (size + 1)];
            //edges = new Edge[(size + 1) * size * 2];
            this.size = size;
            Create();
        }

        internal void ForEachTile(Action<int, int, int> action)
        {
            int i = 0;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    action(x, y, i);
                    i++;
                }
            }
        }

        internal void ForEachVertex(Action<int, int, int> action)
        {
            int i = 0;
            for (int y = 0; y < size + 1; y++)
            {
                for (int x = 0; x < size + 1; x++)
                {
                    action(x, y, i);
                    i++;
                }
            }
        }

        internal void Create()
        {

            #region Tiles

            ForEachTile(delegate (int x, int y, int i)
            {

                Tile currentTile = new Tile(x, y);
                tiles[x, y] = currentTile;

                if (x > 0)
                {
                    // Left side
                    Tile neighbor = tiles[x - 1, y];
                    neighbor.Neighbors.Set(tiles[x, y], CardinalDirection.East);
                    currentTile.Neighbors.Set(neighbor, CardinalDirection.West);
                }

                if (y > 0)
                {
                    // Top side
                    Tile neighbor = tiles[x, y - 1];
                    neighbor.Neighbors.Set(tiles[x, y], CardinalDirection.South);
                    currentTile.Neighbors.Set(neighbor, CardinalDirection.North);
                }

            });

            #endregion

            #region Vertices

            ForEachVertex(delegate (int x, int y, int i)
            {
                vertices[x, y] = new Vertex(x, y);
            });

            ForEachTile(delegate (int x, int y, int i)
            {
                var tile = tiles[x, y];

                tile.Vertices.Set(vertices[x, y], CornerDirection.UpperLeft);
                vertices[x, y].ConnectedTiles.Set(tile, CornerDirection.UpperLeft.Opposite());
                tile.Vertices.Set(vertices[x + 1, y], CornerDirection.UpperRight);
                vertices[x + 1, y].ConnectedTiles.Set(tile, CornerDirection.UpperRight.Opposite());
                tile.Vertices.Set(vertices[x + 1, y + 1], CornerDirection.LowerRight);
                vertices[x + 1, y + 1].ConnectedTiles.Set(tile, CornerDirection.LowerRight.Opposite());
                tile.Vertices.Set(vertices[x, y + 1], CornerDirection.LowerLeft);
                vertices[x, y + 1].ConnectedTiles.Set(tile, CornerDirection.LowerLeft.Opposite());

            });

            #endregion

            #region Edges

            ForEachVertex(delegate (int x, int y, int i)
            {

            });

            #endregion

            foreach (var tile in tiles)
            {
                if (tile.Vertices.Count() != 4)
                {
                    Debug.WriteLine("Error");
                }
            }

        }

        public Tile[,] Tiles => tiles;

        public Vertex[,] Vertices => vertices;
    }
}