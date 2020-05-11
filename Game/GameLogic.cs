// Created by Sakri Koskimies (Github: Saggre) on 25/10/2019

using System;
using System.Collections.Generic;
using System.Numerics;
using EconSim.Collections;
using EconSim.Core;
using EconSim.Core.Input;
using EconSim.Geometry;
using EconSim.Terrain;
using EconSim.EMath;
using SharpDX.Direct3D11;

namespace EconSim.Game
{
    /// <summary>
    /// Main class for the game logic separated from the engine itself
    ///
    /// Creates enough tile objects to satisfy the render distance, and updates them with new terrain maps when moving
    /// </summary>
    public class GameLogic : AutoUpdateable
    {
        struct ShaderBuffer
        {
            public Vector4 cameraPosition;
        }

        private Camera camera;
        private Vector2 lookVector;
        private PlayerMovement playerMovement;

        private GameObject[,] visibleTiles;
        private Vector2Int visibleTilesCenter;

        private TerrainGenerator terrainGenerator;

        public override void Start(int startTime)
        {
            // Init movement manager
            playerMovement = new PlayerMovement();

            // Set camera initial pos
            camera = new Camera();
            camera.Position = new Vector3(0, 2, 3);
            Engine.CurrentScene.ActiveCamera = camera; // TODO more elegantly
            lookVector = new Vector2(0, 140);

            terrainGenerator = new TerrainGenerator();
            InitVisibleTiles(out visibleTiles);
        }

        private void CameraLook(float deltaTime)
        {
            // Move camera
            camera.Move(playerMovement.MovementInput.Multiply(Engine.CurrentScene.ActiveCamera.Rotation), deltaTime);

            // Rotate camera
            // 180, 171
            lookVector.X += InputDevice.Mouse.DeltaPosition.X * 0.02f;
            lookVector.Y -= InputDevice.Mouse.DeltaPosition.Y * 0.02f;

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

        /// <summary>
        /// Initialize visible tiles
        /// </summary>
        /// <param name="tiles"></param>
        private void InitVisibleTiles(out GameObject[,] tiles)
        {
            SharedShader shader = SharedShader.CompileFromFiles(@"Shaders\Tessellation");

            int renderDistance = 4; // How many tiles to render in each direction
            tiles = new GameObject[1 + 2 * renderDistance, 1 + 2 * renderDistance].Populate2D((int x, int y) =>
            {
                GameObject gameObject = new GameObject(Mesh.CreateQuad())
                {
                    Shader = shader
                };
                return gameObject;
            });

            visibleTilesCenter = new Vector2Int(0, 0);
            PositionVisibleTiles(ref tiles, visibleTilesCenter);

            // Add to scene
            foreach (GameObject gameObject in visibleTiles)
            {
                Engine.CurrentScene.AddGameObject(gameObject);
            }
        }

        /// <summary>
        /// Position tiles to surround center position
        /// Assumes that each tile is of size 1x1
        /// </summary>
        /// <param name="tiles"></param>
        /// <param name="centerPosition"></param>
        private void PositionVisibleTiles(ref GameObject[,] tiles, Vector2Int centerPosition)
        {
            // Note: tile x or y length must be odd
            int firstTileOffset = (tiles.GetLength(0) - 1) / 2;

            tiles.ForEach2D((GameObject tile, int x, int y) =>
            {
                Vector2Int tileIndexPosition = new Vector2Int(centerPosition.X + x - firstTileOffset, centerPosition.Y + y - firstTileOffset);
                tile.Position = new Vector3(tileIndexPosition.X, 0, tileIndexPosition.Y);

                ShaderResourceView textureView = new ShaderResourceView(Engine.RenderDevice.d3dDevice, terrainGenerator.GetTerrainChunkAt(tileIndexPosition.X, tileIndexPosition.Y).TerrainMaps.HeightMap);
                tile.Shader.SetShaderResource(tile, 0, textureView);
            });
        }

        public override void Update(float deltaTime)
        {
            CameraLook(deltaTime);

            ShaderBuffer tessellationShaderBuffer;
            tessellationShaderBuffer.cameraPosition = new Vector4(camera.Position, 0f);

            foreach (GameObject gameObject in visibleTiles)
            {
                // Send distance to camera to tessellation shader
                gameObject.Shader.SetConstantBuffer(gameObject, 1,
                    Shader.CreateSingleElementBuffer(ref tessellationShaderBuffer)
                );
            }

            Vector2Int cameraPositionInt = new Vector2Int(camera.Position.X - 0.5f, camera.Position.Z - 0.5f);
            if (visibleTilesCenter != cameraPositionInt)
            {
                visibleTilesCenter = cameraPositionInt;
                PositionVisibleTiles(ref visibleTiles, visibleTilesCenter);
            }

        }

        public override void End(int endTime)
        {

        }
    }
}