// Created by Sakri Koskimies (Github: Saggre) on 29/09/2019

using System;
using System.Diagnostics;
using System.Windows.Forms;
using EconSim.Geometry;
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
        SpriteBatch spriteBatch;

        TerrainRenderVertex[] floorVerts;
        Effect effect;
        private Texture2D texture;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this)
            {
                // Required for compute shader
                GraphicsProfile = GraphicsProfile.HiDef
            };

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
            // TODO: Add your initialization logic here

            floorVerts = new TerrainRenderVertex[6];
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

            floorVerts[0].Color = Color.Red;
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

            //effect = new BasicEffect(graphics.GraphicsDevice);
            effect = Content.Load<Effect>("Shaders/Default");

            Debug.WriteLine("Debug");
            Compute();

            base.Initialize();
        }

        void Compute()
        {
            TerrainGenerator terrainGenerator = new TerrainGenerator();
            TerrainChunk c = terrainGenerator.CreateTerrainChunk(new SquareRect(0, 0, 256));
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

            float aspectRatio =
                graphics.PreferredBackBufferWidth / (float)graphics.PreferredBackBufferHeight;
            float fieldOfView = Microsoft.Xna.Framework.MathHelper.PiOver4;
            float nearClipPlane = 1;
            float farClipPlane = 200;

            Matrix world = Matrix.CreateTranslation(0, 0, 0);

            Vector3 boneTransform = new Vector3(0, 0, 0);
            effect.Parameters["World"].SetValue(world);
            //effect.Parameters["World"].SetValue(world * mesh.ParentBone.Transform);
            effect.Parameters["View"].SetValue(Matrix.CreateLookAt(
                cameraPosition, cameraLookAtVector, cameraUpVector));

            effect.Parameters["Projection"].SetValue(Matrix.CreatePerspectiveFieldOfView(
               fieldOfView, aspectRatio, nearClipPlane, farClipPlane));

            //Matrix worldInverseTransposeMatrix = Matrix.Transpose(Matrix.Invert(mesh.ParentBone.Transform * world));
            Matrix worldInverseTransposeMatrix = Matrix.Transpose(Matrix.Invert(world));
            effect.Parameters["WorldInverseTranspose"].SetValue(worldInverseTransposeMatrix);

            //effect.Parameters["AmbientColor"].SetValue(Color.Green.ToVector4());
            //effect.Parameters["AmbientIntensity"].SetValue(0.5f);

            //effect.TextureEnabled = true;
            //effect.Texture = texture;*/
            effect.CurrentTechnique = effect.Techniques["Ambient"];
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
