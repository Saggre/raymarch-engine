// Created by Sakri Koskimies (Github: Saggre) on 30/09/2019

using System;
using EconSim.Geometry;
using EconSim.Math;

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
            noiseGenerator = new FastNoise(DateTime.Now.Millisecond * 1024);
            //CreateTerrainChunk(new SquareRect(0, 0, 10));
            //CreateTerrainChunk(new SquareRect(10, 0, 10));
        }

        public TerrainChunk CreateTerrainChunk(SquareRect area)
        {
            return new TerrainChunk(area, noiseGenerator);
        }

    }
}