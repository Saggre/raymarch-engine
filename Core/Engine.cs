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
    public class Engine : IDisposable
    {
        private RenderForm renderForm;
        private static int width;
        private static int height;
        private static int fps = 60;
        private static bool isFullscreen = false;

        // The scene that is currently active
        private static Scene currentScene;

        private Stopwatch stopwatch;
        private GameLogic gameLogic;
        private static RenderDevice renderDevice;

        public static Scene CurrentScene => currentScene;

        public static RenderDevice RenderDevice => renderDevice;

        public static int Fps => fps;

        public static bool IsFullscreen => isFullscreen;

        public Engine()
        {
            width = Screen.PrimaryScreen.WorkingArea.Width;
            height = Screen.PrimaryScreen.WorkingArea.Height;

            renderForm = new RenderForm("EconSim");
            renderForm.AutoSize = false;
            renderForm.ClientSize = new Size(width, height);
            renderForm.AllowUserResizing = false;
            renderForm.IsFullscreen = isFullscreen;
            renderForm.StartPosition = FormStartPosition.Manual;
            renderForm.Location = new Point(0, 0);
            renderForm.WindowState = FormWindowState.Maximized;
            renderForm.MinimizeBox = false;
            renderForm.Show();

            renderDevice = new RenderDevice(renderForm);

            // Create main scene
            currentScene = new Scene();

            // Init main game logic script
            gameLogic = new GameLogic();

            // Init stopwatch for deltaTime
            stopwatch = new Stopwatch();
            stopwatch.Start();

            // Init input device
            InputDevice.Init(renderForm);

            int unixTime = Core.Util.ConvertToUnixTimestamp(DateTime.Now);

            // Execute all start methods
            StaticUpdater.ExecuteStartActions(unixTime);

            // Execute each scene GameObject's updateables
            foreach (GameObject mainSceneGameObject in currentScene.GameObjects)
            {
                foreach (IUpdateable updateable in mainSceneGameObject.Updateables)
                {
                    updateable.Start(unixTime);
                }
            }
        }

        public void Run()
        {
            RenderLoop.Run(renderForm, RenderCallback);
        }

        private float lastDeltaTime;
        private void RenderCallback()
        {
            stopwatch.Restart();

            // Execute all update methods
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


        public void Dispose()
        {
            renderForm.Dispose();
        }
    }
}