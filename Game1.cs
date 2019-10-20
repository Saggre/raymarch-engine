// Created by Sakri Koskimies (Github: Saggre) on 29/09/2019

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using EconSim.Core;
using EconSim.Geometry;
using EconSim.Input;
using EconSim.Math;
using EconSim.Terrain;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace EconSim
{

    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        public static GraphicsDeviceManager graphics;
        private Input.Mouse mouse;
        TerrainRenderVertex[] floorVerts;
        Effect effect;
        private Texture2D texture;
        private Player player;

        // TODO create UI manager
        private SpriteFont font;
        private SpriteBatch spriteBatch;

        // Rendering
        private Matrix world = Matrix.CreateTranslation(0, 0, 0);
        Matrix view;
        Matrix projection;

        Vector3 viewVector;
        private Vector2 lookVector;

        float angle = 0;
        float distance = 20;

        public Game1()
        {
            mouse = new Input.Mouse();

            graphics = new GraphicsDeviceManager(this)
            {
                // Required for compute shader
                GraphicsProfile = GraphicsProfile.HiDef
            };

            lookVector = new Vector2();

            player = new Player();
            player.Position = new Vector3(0, 20, 20);
            // Look at the ground
            player.Rotation = Math.Util.LookRotation(Vector3.Zero - player.Position);

            GraphicOptions();

            Content.RootDirectory = "Content";
        }

        private void GraphicOptions()
        {
            int screenWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            int screenHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;

            Form form = (Form)Control.FromHandle(Window.Handle);
            form.WindowState = FormWindowState.Maximized;
            Window.AllowUserResizing = true;

            //graphics.IsFullScreen = true;
            //graphics.PreferredBackBufferWidth = screenWidth;
            //graphics.PreferredBackBufferHeight = screenHeight;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {

            floorVerts = new TerrainRenderVertex[6];
            floorVerts[0].Position = new Vector3(10, 0, -10);
            floorVerts[1].Position = new Vector3(-10, 0, 10);
            floorVerts[2].Position = new Vector3(-10, 0, -10);
            floorVerts[3].Position = floorVerts[1].Position;
            floorVerts[4].Position = floorVerts[0].Position;
            floorVerts[5].Position = new Vector3(10, 0, 10);

            floorVerts[0].TextureCoordinate = new Vector2(1, 0);
            floorVerts[1].TextureCoordinate = new Vector2(0, 1);
            floorVerts[2].TextureCoordinate = new Vector2(0, 0);
            floorVerts[3].TextureCoordinate = floorVerts[1].TextureCoordinate;
            floorVerts[4].TextureCoordinate = floorVerts[0].TextureCoordinate;
            floorVerts[5].TextureCoordinate = new Vector2(1, 1);

            floorVerts[0].Color = Color.White;
            floorVerts[1].Color = Color.White;
            floorVerts[2].Color = Color.White;
            floorVerts[3].Color = Color.White;
            floorVerts[4].Color = Color.White;
            floorVerts[5].Color = Color.White;

            floorVerts[0].Normal = Vector3.Up;
            floorVerts[1].Normal = Vector3.Up;
            floorVerts[2].Normal = Vector3.Up;
            floorVerts[3].Normal = Vector3.Up;
            floorVerts[4].Normal = Vector3.Up;
            floorVerts[5].Normal = Vector3.Up;

            effect = Content.Load<Effect>("Shaders/Default");

            font = Content.Load<SpriteFont>("UI/Main");
            spriteBatch = new SpriteBatch(GraphicsDevice);

            Compute();

            StaticUpdater.ExecuteStartActions();
            base.Initialize();
        }

        void Compute()
        {
            TerrainGenerator terrainGenerator = new TerrainGenerator();
            TerrainChunk c = terrainGenerator.CreateTerrainChunk(new SquareRect(0, 0, 128));
            texture = c.CreateVertexMaps();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            StaticUpdater.ExecuteUpdateActions();

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // Rotate the player's view
            lookVector.X -= mouse.DeltaPosition.Y * deltaTime * 1;
            lookVector.Y += mouse.DeltaPosition.X * deltaTime * 1;

            if (lookVector.X < 90)
            {
                lookVector.X = 90;
            }
            else if (lookVector.X > 270)
            {
                lookVector.X = 270;
            }

            player.Rotation = Math.Util.EulerToQuaternion(lookVector.X, lookVector.Y, 0);

            KeyboardState state = Keyboard.GetState();

            float playerSpeed = 1f;
            if (state.IsKeyDown(Keys.LeftShift))
                playerSpeed = 2f;

            if (state.IsKeyDown(Keys.D))
                player.Move(-player.Right, playerSpeed);
            if (state.IsKeyDown(Keys.A))
                player.Move(player.Right, playerSpeed);
            if (state.IsKeyDown(Keys.W))
                player.Move(player.Forward, playerSpeed);
            if (state.IsKeyDown(Keys.S))
                player.Move(-player.Forward, playerSpeed);
            if (state.IsKeyDown(Keys.Space))
                player.Move(Vector3.Up, playerSpeed);
            if (state.IsKeyDown(Keys.LeftControl))
                player.Move(-Vector3.Up, playerSpeed);

            // TODO: Add your update logic here

            float aspectRatio =
                graphics.PreferredBackBufferWidth / (float)graphics.PreferredBackBufferHeight;
            float fieldOfView = MathHelper.PiOver4;
            float nearClipPlane = 0.1f;
            float farClipPlane = 200;

            projection = Matrix.CreatePerspectiveFieldOfView(
                fieldOfView, aspectRatio, nearClipPlane, farClipPlane);

            Vector3 cameraLocation = player.Position;
            Vector3 cameraTarget = cameraLocation + player.Forward;
            viewVector = Vector3.Transform(cameraTarget - cameraLocation, Matrix.CreateRotationY(0));
            viewVector.Normalize();
            view = Matrix.CreateLookAt(cameraLocation, cameraTarget, Vector3.UnitY);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();

            string str = "Player pos: " + player.Position;
            str += "\nPlayer rot:" + player.Rotation.QuaternionToEuler();
            str += "\nMouse Delta: " + mouse.DeltaPosition;
            str += "\nForward vector: " + player.Forward;
            str += "\nCamera Angle: " + lookVector;
            spriteBatch.DrawString(font, str, new Vector2(100, 100), Color.Black);

            spriteBatch.End();

            DrawModelWithEffect(world, view, projection);

            base.Draw(gameTime);
        }

        void DrawModelWithEffect(Matrix world, Matrix view, Matrix projection)
        {
            Matrix transform = Matrix.Identity;

            effect.CurrentTechnique = effect.Techniques["Diffuse"];

            effect.Parameters["World"].SetValue(world * transform);
            effect.Parameters["View"].SetValue(view);
            effect.Parameters["Projection"].SetValue(projection);
            //effect.Parameters["ViewVector"].SetValue(viewVector);
            effect.Parameters["ModelTexture"].SetValue(texture);

            Matrix worldInverseTransposeMatrix = Matrix.Transpose(Matrix.Invert(transform * world));
            effect.Parameters["WorldInverseTranspose"].SetValue(worldInverseTransposeMatrix);

            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                graphics.GraphicsDevice.DrawUserPrimitives(
                    // We’ll be rendering two trinalges
                    PrimitiveType.TriangleList,
                    // The array of verts that we want to render
                    floorVerts,
                    // The offset, which is 0 since we want to start
                    // at the beginning of the floorVerts array
                    0,
                    // The number of triangles to draw
                    2);
            }
        }

    }
}
