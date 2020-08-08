// Created by Sakri Koskimies (Github: Saggre) on 21/10/2019

using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using EconSim.Core.Input;
using EconSim.Core.Rendering;
using EconSim.Game;
using SharpDX.Windows;

namespace EconSim.Core
{
    /// <summary>
    /// A class that handles initating the rendering class (RenderDevice), rendering loop and input devices
    /// </summary>
    public class Engine : IDisposable
    {
        private static RenderForm renderForm;
        private static int fps = 60;
        private static bool isFullscreen = false;

        // The scene that is currently active
        private static Scene currentScene;

        private Stopwatch stopwatch;
        private GameLogic gameLogic;
        private static RenderDevice renderDevice;

        public static Scene CurrentScene => currentScene;

        public static RenderDevice RenderDevice => renderDevice;

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
        public static int Width => renderForm.Width; // TODO width and height should update on window size changes such as fullscreen entry

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
            return (float)(Width * 1.0 / Height);
        }

        /// <summary>
        /// 
        /// </summary>
        public Engine()
        {
            {
                // Init window
                renderForm = new RenderForm("EconSim");
                renderForm.AutoSize = false;
                renderForm.ClientSize = new Size(Screen.PrimaryScreen.WorkingArea.Width, Screen.PrimaryScreen.WorkingArea.Height);
                renderForm.AllowUserResizing = false;
                renderForm.IsFullscreen = isFullscreen;
                renderForm.StartPosition = FormStartPosition.Manual;
                renderForm.Location = new Point(0, 0);
                renderForm.WindowState = FormWindowState.Maximized;
                renderForm.MinimizeBox = false;
                renderForm.Show();
            }


            renderDevice = new RenderDevice(renderForm);

            // Create main scene
            currentScene = new Scene();

            // Init main game logic script
            gameLogic = new GameLogic();

            // Start stopwatch for deltaTime
            stopwatch = new Stopwatch();
            stopwatch.Start();

            // Init input device
            InputDevice.Init(renderForm);

            int unixTime = Util.ConvertToUnixTimestamp(DateTime.Now);

            // Execute all start methods
            StaticUpdater.ExecuteStartActions(unixTime);

            // Execute each scene GameObject's updateables' Start method
            foreach (GameObject mainSceneGameObject in currentScene.GameObjects)
            {
                foreach (IUpdateable updateable in mainSceneGameObject.Updateables)
                {
                    updateable.Start(unixTime);
                }
            }
        }

        /// <summary>
        /// Starts the rendering loop
        /// </summary>
        public void Run()
        {
            RenderLoop.Run(renderForm, RenderCallback);
        }

        private float lastDeltaTime;

        /// <summary>
        /// This method runs on every frame
        /// </summary>
        private void RenderCallback()
        {
            stopwatch.Restart();

            // Execute all Update methods
            StaticUpdater.ExecuteUpdateActions(lastDeltaTime);

            // Render on each frame
            renderDevice.Draw(lastDeltaTime, gameObject =>
            {
                // Execute updates per-object
                foreach (IUpdateable updateable in gameObject.Updateables)
                {
                    updateable.Update(lastDeltaTime);
                }
            });

            stopwatch.Stop();
            lastDeltaTime = (float)stopwatch.Elapsed.TotalSeconds;
        }

        /// <summary>
        /// Called on program close
        /// </summary>
        public void Dispose()
        {
            renderForm.Dispose();
        }
    }
}