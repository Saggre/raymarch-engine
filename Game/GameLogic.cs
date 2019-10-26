// Created by Sakri Koskimies (Github: Saggre) on 25/10/2019

using System;
using System.Numerics;
using EconSim.Core;
using EconSim.Geometry;
using EconSim.Terrain;
using SharpDX.Direct3D11;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace EconSim.Game
{
    public class GameLogic : AutoUpdateable
    {

        public override void Start(int startTime)
        {
            // Temp
            TerrainGenerator terrainGenerator = new TerrainGenerator();

            InputLayout inputLayout;
            VertexShader vertexShader;
            PixelShader pixelShader;
            Shader.CompileFromFiles("", out inputLayout, out vertexShader, out pixelShader);
            SharedShader shader = new SharedShader(inputLayout, vertexShader, null, null, null, pixelShader);

            EconSim.mainScene.AddGameObject(CreateTile(new Vector3(0, 0, 0), shader, terrainGenerator));
            EconSim.mainScene.AddGameObject(CreateTile(new Vector3(-1, 0, 0), shader, terrainGenerator));
        }

        private GameObject CreateTile(Vector3 position, SharedShader shader, TerrainGenerator terrainGenerator)
        {
            const int size = 128;

            TerrainChunk c = terrainGenerator.CreateTerrainChunk(new SquareRect((int)(size * position.X), (int)(size * position.Y), size));
            Texture2D texture = c.CreateVertexMaps();
            ShaderResourceView textureView = new ShaderResourceView(EconSim.d3dDevice, texture);

            GameObject plane = new GameObject(new Mesh(Primitive.Plane()));
            plane.Position = position;
            plane.Shader = shader;
            // TODO both planes render the same texture
            plane.Shader.SetShaderResource(plane, 0, textureView);
            plane.Shader.SetSampler(plane, 0, ShaderUtils.DefaultSamplerState());

            return plane;
        }

        public override void Update(float deltaTime)
        {

        }

        public override void End(int endTime)
        {

        }
    }
}