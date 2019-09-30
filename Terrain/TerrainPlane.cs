// Created by Sakri Koskimies (Github: Saggre) on 29/09/2019

namespace EconSim.Terrain
{
    public class TerrainPlane
    {
        private Tile[,] _tiles;

        public TerrainPlane(int size = 16)
        {
            _tiles = new Tile[size, size];

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    CardinalNeighborCollection<Tile> neighbors = new CardinalNeighborCollection<Tile>();
                    _tiles[x, y] = new Tile(neighbors);

                    if (x > 0)
                    {
                        // Left side
                        Tile neighbor = _tiles[x - 1, y];
                        neighbor.Neighbors.SetCardinalNeighbor(_tiles[x, y], CardinalNeighborCollection<Tile>.FourDirection.East);
                        neighbors.SetCardinalNeighbor(neighbor, CardinalNeighborCollection<Tile>.FourDirection.West);
                    }

                    if (y > 0)
                    {
                        // Top side
                        Tile neighbor = _tiles[x, y - 1];
                        neighbor.Neighbors.SetCardinalNeighbor(_tiles[x, y], CardinalNeighborCollection<Tile>.FourDirection.South);
                        neighbors.SetCardinalNeighbor(neighbor, CardinalNeighborCollection<Tile>.FourDirection.North);
                    }


                }
            }
        }
    }
}