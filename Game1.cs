﻿// Created by Sakri Koskimies (Github: Saggre) on 29/09/2019

using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace EconSim
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        VertexPositionTexture[] floorVerts;
        BasicEffect effect;
        private Texture2D _t;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.GraphicsProfile = GraphicsProfile.HiDef; // Required for computeshader
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            floorVerts = new VertexPositionTexture[6];
            floorVerts[0].Position = new Vector3(-20, -20, 0);
            floorVerts[1].Position = new Vector3(-20, 20, 0);
            floorVerts[2].Position = new Vector3(20, -20, 0);
            floorVerts[3].Position = floorVerts[1].Position;
            floorVerts[4].Position = new Vector3(20, 20, 0);
            floorVerts[5].Position = floorVerts[2].Position;

            floorVerts[0].TextureCoordinate = new Vector2(0, 0);
            floorVerts[1].TextureCoordinate = new Vector2(0, 1);
            floorVerts[2].TextureCoordinate = new Vector2(1, 0);
            floorVerts[3].TextureCoordinate = floorVerts[1].TextureCoordinate;
            floorVerts[4].TextureCoordinate = new Vector2(1, 1);
            floorVerts[5].TextureCoordinate = floorVerts[2].TextureCoordinate;

            effect = new BasicEffect(graphics.GraphicsDevice);

            Debug.WriteLine("Debug");
            Compute();

            base.Initialize();
        }

        void Compute()
        {
            ComputeShader computer = new ComputeShader(graphics.GraphicsDevice, "Shader/test.hlsl", "CS");

            ComputeBuffer cb = new ComputeBuffer(1, 4);
            cb.SetData(new[] { 0.3f });
            _t = new Texture2D(graphics.GraphicsDevice, 1024, 1024);

            computer.SetTexture(_t, 0);
            computer.SetComputeBuffer(cb, 1);

            computer.Begin();
            computer.Dispatch(32, 32, 1);

            _t = computer.GetTexture(0);

            computer.End();
            /*
            //Start Compute Shader Algorithm
            Debug.WriteLine("\nCompute Shader Algorithm");

            //start timer
            Stopwatch st = new Stopwatch();
            st.Start();

            //execute compute shader
            computer.Begin();
            computer.Start(32, 32, 1);
            _t = computer.GetTexture();
            //computer.End();

            //stop timer
            st.Stop();

            //get result
            //ResultData[] data = computer.ReadData(repetition);


            int csTime = (int)st.ElapsedMilliseconds;

            //Debug.WriteLine(string.Format("Time: {0} ms", csTime));
            */
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
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here

            DrawGround();

            base.Draw(gameTime);
        }

        void DrawGround()
        {
            // The assignment of effect.View and effect.Projection
            // are nearly identical to the code in the Model drawing code.
            var cameraPosition = new Vector3(0, 40, 20);
            var cameraLookAtVector = Vector3.Zero;
            var cameraUpVector = Vector3.UnitZ;

            effect.View = Matrix.CreateLookAt(
                cameraPosition, cameraLookAtVector, cameraUpVector);

            float aspectRatio =
                graphics.PreferredBackBufferWidth / (float)graphics.PreferredBackBufferHeight;
            float fieldOfView = Microsoft.Xna.Framework.MathHelper.PiOver4;
            float nearClipPlane = 1;
            float farClipPlane = 200;

            effect.Projection = Matrix.CreatePerspectiveFieldOfView(
                fieldOfView, aspectRatio, nearClipPlane, farClipPlane);

            effect.TextureEnabled = true;
            effect.Texture = _t;

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
