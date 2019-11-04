// Created by Sakri Koskimies (Github: Saggre) on 30/09/2019

using System;
using EconSim.Geometry;
using EconSim.EMath;

namespace EconSim.Terrain
{
    public class TerrainGenerator
    {
        private FastNoise noiseGenerator;

        public TerrainGenerator()
        {
            Init();
        }

        private void Init()
        {
            // Create seed
            noiseGenerator = new FastNoise(DateTime.Now.Millisecond * 1024);
        }

        public TerrainChunk CreateTerrainChunk(SquareRect area)
        {
            return new TerrainChunk(area, noiseGenerator);
        }

    }
}