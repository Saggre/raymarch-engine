// Created by Sakri Koskimies (Github: Saggre) on 30/09/2019

using System;
using EconSim.Geometry;
using EconSim.EMath;

namespace EconSim.Terrain
{
    public class TerrainGenerator
    {
        private FastNoise noiseGenerator;
        private TerrainChunk[,] terrainChunks;
        const int size = 128;

        public TerrainGenerator()
        {
            Init();
        }

        private void Init()
        {
            // Create seed
            noiseGenerator = new FastNoise(DateTime.Now.Millisecond * 1024);
            terrainChunks = new TerrainChunk[size, size];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public TerrainChunk GetTerrainChunkAt(int x, int y)
        {
            Vector2Int worldToIndex = new Vector2Int(x + size / 2, y + size / 2);

            // Create the chunk if it doesn't exist
            if (terrainChunks[worldToIndex.X, worldToIndex.Y] == null)
            {
                terrainChunks[worldToIndex.X, worldToIndex.Y] = CreateTerrainChunk(new Vector2Int(x, y));
            }

            return terrainChunks[worldToIndex.X, worldToIndex.Y];
        }

        private TerrainChunk CreateTerrainChunk(Vector2Int position)
        {
            return new TerrainChunk(position, noiseGenerator);
        }

    }
}