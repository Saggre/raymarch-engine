// Created by Sakri Koskimies (Github: Saggre) on 21/10/2019

using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using WindowsInput.Native;
using RaymarchEngine.Core.Input;
using RaymarchEngine.Core.Rendering;
using RaymarchEngine.Physics;
using SharpDX.Windows;

namespace RaymarchEngine.Core
{
    /// <summary>
    /// A class that handles initiating the rendering class (RenderDevice), rendering loop and input devices
    /// </summary>
    internal class Engine : IDisposable
    {
        private static RenderForm renderForm;
        private static bool isFullscreen = false;

        private static float elapsedTime;
        private Stopwatch stopwatch;
        private AutoUpdateable gameLogic;
        private static RenderDevice renderDevice;
        private static PhysicsHandler physics;

        /// <summary>
        /// The render device, available only after the engine has been constructed
        /// </summary>
        public static RenderDevice RenderDevice => renderDevice;

        /// <summary>
        /// Time elapsed since starting the engine
        /// </summary>
        public static float ElapsedTime => elapsedTime;

        /// <summary>
        /// Is the window full screen?
        /// </summary>
        public static bool IsFullscreen => isFullscreen;

        /// <summary>
        /// Get the window's width
        /// </summary>
        public static int Width => renderForm.ClientSize.Width;

        /// <summary>
        /// Get the window's height
        /// </summary>
        public static int Height => renderForm.ClientSize.Height;

        /// <summary>
        /// Get the window's aspect ratio
        /// </summary>
        /// <returns>Width / Height</returns>
        public static float AspectRatio()
        {
            return (float) (Width * 1.0 / Height);
        }

        /// <summary>
        /// Opens the window, creates the scene, physics and input, runs every Start method and
        /// then creates the render device. The render device comes last because it reads the
        /// scene's primitives to size its buffers.
        /// </summary>
        /// <param name="gameLogic">Game logic to run, its Start is called during construction</param>
        public Engine(AutoUpdateable gameLogic)
        {
            {
                // Init window
                renderForm = new RenderForm("RaymarchEngine");
                renderForm.AutoSize = false;
                renderForm.ClientSize = new Size(
                    Screen.PrimaryScreen.WorkingArea.Width,
                    Screen.PrimaryScreen.WorkingArea.Height
                );
                renderForm.AllowUserResizing = false;
                renderForm.IsFullscreen = isFullscreen;
                renderForm.StartPosition = FormStartPosition.Manual;
                renderForm.Location = new Point(0, 0);
                renderForm.WindowState = FormWindowState.Maximized;
                renderForm.MinimizeBox = false;
                renderForm.Show();
            }

            this.gameLogic = gameLogic;

            // Create main scene
            Scene.CurrentScene = new Scene();

            // Start physics library
            physics = new PhysicsHandler(PhysicsReady);

            // Init input device
            InputDevice.Init(renderForm);

            int unixTime = Util.ConvertToUnixTimestamp(DateTime.Now);

            // Execute all start methods
            StaticUpdater.ExecuteStartActions(unixTime);

            // Execute each scene object's components' Start method
            foreach (GameObject gameObject in Scene.CurrentScene.GameObjects)
            {
                foreach (IComponent component in gameObject.Components)
                {
                    component.Start(unixTime);
                }
            }

            // It's important that render device is created after scene and game logic start
            renderDevice = new RenderDevice(renderForm, new Resolution(2560, 1440));

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

            // Before anything reads it, so no component acts on the previous frame's input
            InputDevice.Update(lastDeltaTime);

            // Execute all Update methods
            StaticUpdater.ExecuteUpdateActions(lastDeltaTime);
            
            // Render on each frame
            renderDevice.Draw();

            foreach (GameObject gameObject in Scene.CurrentScene.GameObjects)
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
        /// Runs every End method and releases the render device and the window
        /// </summary>
        public void Dispose()
        {
            int unixTime = Util.ConvertToUnixTimestamp(DateTime.Now);

            // Execute all end methods
            StaticUpdater.ExecuteEndActions(unixTime);

            // Execute each scene GameObject's end methods
            foreach (GameObject gameObject in Scene.CurrentScene.GameObjects)
            {
                foreach (IComponent component in gameObject.Components)
                {
                    component.End(unixTime);
                }
            }

            InputDevice.Dispose();

            renderDevice?.Dispose();
            renderForm.Dispose();
        }
    }
}