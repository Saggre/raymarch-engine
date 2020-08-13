// Created by Sakri Koskimies (Github: Saggre) on 21/10/2019

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using WindowsInput.Native;
using RaymarchEngine.Core.Input;
using RaymarchEngine.Core.Primitives;
using RaymarchEngine.Core.Rendering;
using RaymarchEngine.Game;
using RaymarchEngine.Physics;
using SharpDX.Windows;

namespace RaymarchEngine.Core
{
    /// <summary>
    /// A class that handles initiating the rendering class (RenderDevice), rendering loop and input devices
    /// </summary>
    public class Engine : IDisposable
    {
        private static RenderForm renderForm;
        private static int fps = 60;
        private static bool isFullscreen = false;

        // The scene that is currently active
        private static Scene currentScene;

        private static float elapsedTime;
        private Stopwatch stopwatch;
        private GameLogic gameLogic;
        private static RenderDevice renderDevice;
        private static PhysicsHandler physics;

        public static Scene CurrentScene => currentScene;

        public static RenderDevice RenderDevice => renderDevice;

        /// <summary>
        /// Time elapsed since starting the engine
        /// </summary>
        public static float ElapsedTime => elapsedTime;

        /// <summary>
        /// Get the program's frames per second
        /// </summary>
        public static int Fps => fps;

        /// <summary>
        /// Is the window full screen?
        /// </summary>
        public static bool IsFullscreen => isFullscreen;

        /// <summary>
        /// Get the window's width
        /// </summary>
        public static int Width =>
            renderForm.Width; // TODO width and height should update on window size changes such as fullscreen entry

        // TODO check height and width are right

        /// <summary>
        /// Get the window's height
        /// </summary>
        public static int Height => renderForm.Height;

        /// <summary>
        /// Get the window's aspect ratio
        /// </summary>
        /// <returns>Width / Height</returns>
        public static float AspectRatio()
        {
            return (float) (Width * 1.0 / Height);
        }

        /// <summary>
        /// 
        /// </summary>
        public Engine()
        {
            {
                // Init window
                renderForm = new RenderForm("RaymarchEngine");
                renderForm.AutoSize = false;
                renderForm.ClientSize = new Size(Screen.PrimaryScreen.WorkingArea.Width,
                    Screen.PrimaryScreen.WorkingArea.Height);
                renderForm.AllowUserResizing = false;
                renderForm.IsFullscreen = isFullscreen;
                renderForm.StartPosition = FormStartPosition.Manual;
                renderForm.Location = new Point(0, 0);
                renderForm.WindowState = FormWindowState.Maximized;
                renderForm.MinimizeBox = false;
                renderForm.Show();
            }

            RaymarchRenderer.Init();

            // Create main scene
            currentScene = new Scene();

            // Init main game logic script
            gameLogic = new GameLogic();

            // Start physics library
            physics = new PhysicsHandler(PhysicsReady);

            // Init input device
            InputDevice.Init(renderForm);

            int unixTime = Util.ConvertToUnixTimestamp(DateTime.Now);

            // Execute all start methods
            StaticUpdater.ExecuteStartActions(unixTime);

            // Execute each scene object's components' Start method
            foreach (GameObject gameObject in currentScene.GameObjects)
            {
                foreach (IComponent component in gameObject.Components)
                {
                    component.Start(unixTime);
                }
            }

            // It's important that render device is created after scene and game logic start
            renderDevice = new RenderDevice(renderForm, new Resolution(1920, 1080));

            // Start stopwatch for deltaTime
            stopwatch = new Stopwatch();
            stopwatch.Start();
        }

        private void PhysicsReady()
        {
        }

        /// <summary>
        /// Starts the rendering loop
        /// </summary>
        public void Run()
        {
            RenderLoop.Run(renderForm, GameLoop);
        }

        private float lastDeltaTime;

        /// <summary>
        /// This method runs on every frame
        /// </summary>
        private void GameLoop()
        {
            // Close program with esc
            if (InputDevice.Keyboard.IsKeyDown(VirtualKeyCode.ESCAPE))
            {
                renderForm.Dispose();
            }

            stopwatch.Restart();

            // Execute all Update methods
            StaticUpdater.ExecuteUpdateActions(lastDeltaTime);

            // Render on each frame
            renderDevice.Draw();

            foreach (GameObject gameObject in currentScene.GameObjects)
            {
                // Execute updates per-object
                foreach (IComponent component in gameObject.Components)
                {
                    component.Update(lastDeltaTime);
                }
            }

            // TODO create separate physics loop
            PhysicsHandler.Simulation.Timestep(lastDeltaTime);

            stopwatch.Stop();
            lastDeltaTime = (float) stopwatch.Elapsed.TotalSeconds;
            elapsedTime += lastDeltaTime;
        }

        /// <summary>
        /// Called on program close
        /// </summary>
        public void Dispose()
        {
            int unixTime = Util.ConvertToUnixTimestamp(DateTime.Now);

            // Execute all end methods
            StaticUpdater.ExecuteEndActions(unixTime);

            // Execute each scene GameObject's end methods
            foreach (GameObject gameObject in currentScene.GameObjects)
            {
                foreach (IComponent component in gameObject.Components)
                {
                    component.End(unixTime);
                }
            }

            renderForm.Dispose();
        }
    }
}