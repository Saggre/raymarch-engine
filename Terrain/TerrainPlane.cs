// Created by Sakri Koskimies (Github: Saggre) on 29/09/2019

using System;
using System.Diagnostics;
using EconSim.Geometry;
using EconSim.Geometry.CardinalCollection;

namespace EconSim.Terrain
{
    public class TerrainPlane
    {
        private readonly Tile[,] tiles;
        private readonly Vertex[,] vertices;
        private readonly int size;

        /// <summary>
        /// A plane of size x size tiles will be created
        /// </summary>
        /// <param name="size"></param>
        public TerrainPlane(int size)
        {
            tiles = new Tile[size, size];
            vertices = new Vertex[size + 1, size + 1];
            //edges = new Edge[(size + 1) * size * 2];
            this.size = size;

            Create();
        }

        /// <summary>
        /// Execute an action for each tile
        /// </summary>
        /// <param name="action">First param is X index / coordinate, second is Y index / coordinate and third is cumulative index of both</param>
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

        /// <summary>
        /// Iterate each tile starting from (startX, startY) and continuing outwards in a spiral.
        /// Makes finding the closest tile of a certain type a lot faster.
        /// </summary>
        public void ForEachTileSpiralic(int startX, int startY, Action<int, int> action)
        {
            /*
             *     ... 11 10
                7 7 7 7 6 10
                8 3 3 2 6 10
                8 4 . 1 6 10
                8 4 5 5 5 10
                8 9 9 9 9 9
             */

            // TODO create test

            int turns = 1;
            while (true)
            {
                int awind = turns * 2 - 1;
                int xCoord, yCoord;

                // Check whether the coord is inside the plane and execute the action if it is
                void CheckAndExecute(int xCoord, int yCoord)
                {
                    if (xCoord <= size && yCoord <= size)
                    {
                        action(xCoord, yCoord);
                    }
                }

                // Bottom - iterate through the row horizontally
                for (int x = 0; x < awind; x++)
                {
                    xCoord = startX - turns + 2 + x;
                    yCoord = startY - turns + 1;

                    CheckAndExecute(xCoord, yCoord);
                }

                // Right - iterate through the column vertically
                for (int y = 0; y < awind; y++)
                {
                    xCoord = startX + turns;
                    yCoord = startY + turns - y;

                    CheckAndExecute(xCoord, yCoord);
                }

                // Top
                for (int x = 0; x < awind + 1; x++)
                {
                    xCoord = startX - turns + x;
                    yCoord = startY - turns;

                    CheckAndExecute(xCoord, yCoord);
                }

                // Left
                for (int y = 0; y < awind + 1; y++)
                {
                    xCoord = startX - turns;
                    yCoord = tartY + turns - y;

                    CheckAndExecute(xCoord, yCoord);
                }

                turns++;
            }
        }

        /// <summary>
        /// Execute an action for each tile
        /// </summary>
        /// <param name="action">First param is X index / coordinate, second is Y index / coordinate and third is cumulative index of both</param>
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

            // For debugging
            /*foreach (var tile in tiles)
            {
                if (tile.Vertices.Count() != 4)
                {
                    Debug.WriteLine("Error");
                }
            }*/

        }

        public Tile[,] Tiles => tiles;

        public Vertex[,] Vertices => vertices;
    }
}