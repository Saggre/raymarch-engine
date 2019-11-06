// Created by Sakri Koskimies (Github: Saggre) on 25/10/2019

using System.Collections.Generic;
using System.Numerics;
using EconSim.Core;
using EconSim.Core.Input;
using EconSim.Geometry;
using EconSim.Terrain;
using EconSim.EMath;
using SharpDX.Direct3D11;

namespace EconSim.Game
{
    public class GameLogic : AutoUpdateable
    {
        struct ShaderBuffer
        {
            public Vector4 cameraPosition;
        }

        public static Camera camera;
        private Vector2 lookVector;
        private PlayerMovement playerMovement;

        private List<GameObject> tiles;

        public override void Start(int startTime)
        {
            // Init movement manager
            playerMovement = new PlayerMovement();

            tiles = new List<GameObject>();

            // Set camera initial pos
            camera = new Camera();
            camera.Position = new Vector3(0, 2, 3);
            Engine.CurrentScene.ActiveCamera = camera; // TODO more elegantly
            lookVector = new Vector2(0, 140);

            // Temp
            TerrainGenerator terrainGenerator = new TerrainGenerator();

            SharedShader shader = (SharedShader)Shader.CompileFromFiles(@"Shaders\Tessellation");

            tiles.Add(CreateTile(new Vector3(0, 0, 0), shader, terrainGenerator));

            // Neighbors
            tiles.Add(CreateTile(new Vector3(1, 0, 0), shader, terrainGenerator));
            tiles.Add(CreateTile(new Vector3(-1, 0, 0), shader, terrainGenerator));
            tiles.Add(CreateTile(new Vector3(0, 0, 1), shader, terrainGenerator));
            tiles.Add(CreateTile(new Vector3(0, 0, -1), shader, terrainGenerator));

            foreach (GameObject gameObject in tiles)
            {
                Engine.CurrentScene.AddGameObject(gameObject);
            }
        }

        private void CameraLook(float deltaTime)
        {
            // Move camera
            camera.Move(playerMovement.MovementInput.Multiply(Engine.CurrentScene.ActiveCamera.Rotation), deltaTime);

            // Rotate camera
            // 180, 171
            lookVector.X += InputDevice.Mouse.DeltaPosition.X * deltaTime;
            lookVector.Y -= InputDevice.Mouse.DeltaPosition.Y * deltaTime;

            if (lookVector.Y < 100)
            {
                lookVector.Y = 100;
            }
            else if (lookVector.Y > 260 - float.Epsilon)
            {
                lookVector.Y = 260;
            }

            camera.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, lookVector.X * EMath.Util.Deg2Rad) *
                                  Quaternion.CreateFromAxisAngle(Vector3.UnitX, lookVector.Y * EMath.Util.Deg2Rad);
        }

        private GameObject CreateTile(Vector3 position, SharedShader shader, TerrainGenerator terrainGenerator)
        {
            const int size = 128;

            TerrainChunk c = terrainGenerator.CreateTerrainChunk(new SquareRect((int)(size * position.X), (int)(size * position.Z), size));
            Texture2D texture = c.CreateVertexMaps();
            ShaderResourceView textureView = new ShaderResourceView(Engine.RenderDevice.d3dDevice, texture);

            GameObject plane = new GameObject(Mesh.CreateQuad());
            plane.Position = position;
            plane.Shader = shader;
            // TODO both planes render the same texture
            plane.Shader.SetShaderResource(plane, 0, textureView);

            return plane;
        }

        public override void Update(float deltaTime)
        {
            CameraLook(deltaTime);

            ShaderBuffer tessellationShaderBuffer;
            tessellationShaderBuffer.cameraPosition = new Vector4(camera.Position, 0f);

            foreach (GameObject gameObject in tiles)
            {
                gameObject.Shader.SetConstantBuffer(gameObject, 1,
                    Shader.CreateSingleElementBuffer(ref tessellationShaderBuffer)
                );
            }

        }

        public override void End(int endTime)
        {

        }
    }
}