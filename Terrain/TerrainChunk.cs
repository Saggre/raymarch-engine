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

    public class TerrainChunk
    {
        // This tile's terrain maps (such as the heightmap)
        public TerrainMaps TerrainMaps;

        private Stopwatch stopwatch;
        private SquareRect bounds;

        /*private FastNoise noise;
        private TerrainPlane terrainPlane;
        
        ;

        // tiles indexed by terrain type
        private Dictionary<Util.TerrainType, List<Tile>> tilesIndexedByTerrain;*/

        public TerrainChunk(SquareRect bounds, FastNoise noise)
        {
            this.bounds = bounds;
            TerrainMaps.HeightMap = CreateVertexMaps();
        }


        /// <summary>
        /// Create a texture with vertex moisture and elevation data
        /// </summary>
        public Texture2D CreateVertexMaps()
        {

            // Render
            /*ComputeShader computer = new ComputeShader(@"Shaders/Terrain", @"MapGenerator.hlsl");

            // Input texture
            Texture2D computeResource = new Texture2D(Engine.RenderDevice.d3dDevice, new Texture2DDescription()
            {
                BindFlags = BindFlags.UnorderedAccess | BindFlags.ShaderResource,
                Format = Format.R8G8B8A8_UNorm,
                Width = 1024,
                Height = 1024,
                OptionFlags = ResourceOptionFlags.None,
                MipLevels = 1,
                ArraySize = 1,
                SampleDescription = { Count = 1, Quality = 0 },
                Usage = ResourceUsage.Default //?
            });

            computer.SetTexture(computeResource, 0);
            computer.SetComputeBuffer(vertexBuffer, 1);
            computer.SetComputeBuffer(tileBuffer, 2);

            computer.Begin();
            computer.Dispatch(32, 32, 1);

            // Get output
            Texture2D texture = computer.GetTexture(0);

            computer.End();*/


            int repetition = 1024 * 1024;
            ComputeDevice<float> computer = new ComputeDevice<float>(@"Shaders/Terrain/Heightmap.hlsl", "main", repetition);

            //execute compute shader
            computer.Begin();
            computer.Start(32, 32, 1);
            computer.End();

            //get result
            float[] data = computer.ReadData(repetition);

            Texture2D t = new Texture2D(Engine.RenderDevice.d3dDevice, new Texture2DDescription()
            {
                BindFlags = BindFlags.UnorderedAccess | BindFlags.ShaderResource,
                Format = Format.R32_Float,
                Width = 1024,
                Height = 1024,
                OptionFlags = ResourceOptionFlags.None,
                MipLevels = 1,
                ArraySize = 1,
                SampleDescription = { Count = 1, Quality = 0 }
            }, new DataRectangle(EconSim.Core.Util.GetDataPtr(data), 1024)); // TODO try to get dynamic tex directly

            return t;
        }
    }
}