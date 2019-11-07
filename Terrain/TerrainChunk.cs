// Created by Sakri Koskimies (Github: Saggre) on 30/09/2019

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using EconSim.Collections;
using EconSim.Core;
using EconSim.Core.Rendering;
using EconSim.Geometry;
using EconSim.EMath;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Color = System.Drawing.Color;
using ComputeShader = EconSim.EMath.ComputeShader;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;
using Vector4 = SharpDX.Vector4;

namespace EconSim.Terrain
{
    using Math = System.Math;

    public struct TerrainMaps
    {
        public Texture2D HeightMap;
    }

    public struct HeightmapInput
    {
        public int resolution;
        public Vector2Int offset;
        public int unused;
    }

    public class TerrainChunk
    {
        // This tile's terrain maps (such as the heightmap)
        public TerrainMaps TerrainMaps;

        private Stopwatch stopwatch;
        private Vector2Int position;

        /*private FastNoise noise;
        private TerrainPlane terrainPlane;
        
        ;

        // tiles indexed by terrain type
        private Dictionary<Util.TerrainType, List<Tile>> tilesIndexedByTerrain;*/

        public TerrainChunk(Vector2Int position, FastNoise noise)
        {
            this.position = position;
            TerrainMaps.HeightMap = CreateVertexMaps();
        }


        /// <summary>
        /// Create a texture with vertex moisture and elevation data
        /// </summary>
        public Texture2D CreateVertexMaps()
        {
            int resolution = 128;
            TextureComputeDevice computer = new TextureComputeDevice(@"Shaders/Terrain", @"Heightmap.hlsl", resolution,
                Format.R8G8B8A8_UNorm);

            HeightmapInput heightmapInput;
            heightmapInput.resolution = resolution;
            heightmapInput.offset = new Vector2Int(position.X, position.Y);
            heightmapInput.unused = 0;

            computer.Begin();
            computer.SendBufferToShader(0, Shader.CreateSingleElementBuffer(ref heightmapInput));
            computer.Start(resolution, resolution, 1);
            computer.End();

            // Get result
            SharpDX.Color[] data = computer.ReadData<SharpDX.Color>(resolution);

            Texture2D t = new Texture2D(Engine.RenderDevice.d3dDevice, new Texture2DDescription()
            {
                BindFlags = BindFlags.UnorderedAccess | BindFlags.ShaderResource,
                Format = Format.R8G8B8A8_UNorm,
                Width = resolution,
                Height = resolution,
                OptionFlags = ResourceOptionFlags.None,
                MipLevels = 1,
                ArraySize = 1,
                SampleDescription = { Count = 1, Quality = 0 }
            }, new DataRectangle(Core.Util.GetDataPtr(data), resolution * 4));

            return t;
        }
    }
}