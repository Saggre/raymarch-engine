// Created by Sakri Koskimies (Github: Saggre) on 21/10/2019

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using WindowsInput.Native;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Windows;

using EconSim.Core;
using EconSim.Core.Input;
using EconSim.Core.Rendering;
using EconSim.Game;
using EconSim.EMath;
using Matrix = SharpDX.Matrix;

namespace EconSim
{

    public class Engine : IDisposable
    {
        private RenderForm renderForm;
        private const int Width = 1280;
        private const int Height = 720;

        // The scene that is currently active
        private static Scene currentScene;

        private Stopwatch stopwatch;
        private GameLogic gameLogic;
        private static RenderDevice renderDevice;

        public static Scene CurrentScene => currentScene;

        public static RenderDevice RenderDevice => renderDevice;

        public Engine()
        {
            renderForm = new RenderForm("EconSim");
            renderForm.ClientSize = new Size(Width, Height);
            renderForm.AllowUserResizing = false;

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