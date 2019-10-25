// Created by Sakri Koskimies (Github: Saggre) on 25/10/2019

using System;
using EconSim.Core;
using EconSim.Geometry;
using EconSim.Terrain;
using SharpDX;
using SharpDX.Direct3D11;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace EconSim.Game
{
    public class GameLogic : AutoUpdateable
    {
        public Texture2D texture;
        public GameObject plane;

        public override void Start(int startTime)
        {
            // Temp
            TerrainGenerator terrainGenerator = new TerrainGenerator();
            TerrainChunk c = terrainGenerator.CreateTerrainChunk(new SquareRect(0, 0, 128));
            texture = c.CreateVertexMaps();
            ShaderResourceView textureView = new ShaderResourceView(EconSim.d3dDevice, texture);

            plane = new GameObject(new Mesh(Primitive.Plane()));
            plane.Shader = Shader.CompileFromFiles("");
            plane.Shader.SetShaderResource(0, textureView);
            plane.Shader.SetSampler(0, ShaderUtils.DefaultSamplerState());

            EconSim.mainScene.AddGameObject(plane);
        }

        public override void Update(float deltaTime)
        {

        }

        public override void End(int endTime)
        {

        }
    }
}