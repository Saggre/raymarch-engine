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
        /// How many primitives are allowed in the game
        /// </summary>
        private static Dictionary<Type, int> primitiveCounts;

        /// <summary>
        /// Get the number of primitives by type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static int PrimitiveCount<T>() where T : Primitive
        {
            return primitiveCounts[typeof(T)];
        }

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

            primitiveCounts = new Dictionary<Type, int>
            {
                {typeof(Sphere), 2},
                {typeof(Box), 2},
                {typeof(Plane), 1},
                {typeof(Ellipsoid), 32},
                {typeof(Torus), 32},
                {typeof(CappedTorus), 32}
            };

            // Create main scene
            currentScene = new Scene();

            // Init main game logic script
            gameLogic = new GameLogic();

            // It's important that render device is created after the scene
            renderDevice = new RenderDevice(renderForm);

            // Start physics library
            physics = new PhysicsHandler(PhysicsReady);

            // Start stopwatch for deltaTime
            stopwatch = new Stopwatch();
            stopwatch.Start();

            // Init input device
            InputDevice.Init(renderForm);

            int unixTime = Util.ConvertToUnixTimestamp(DateTime.Now);

            // Execute all start methods
            StaticUpdater.ExecuteStartActions(unixTime);

            // Execute each scene object's updateables' Start method
            foreach (Primitive mainSceneObject in CurrentScene.GroupedPrimitives.GetAllPrimitives()
            )
            {
                foreach (IUpdateable updateable in mainSceneObject.Updateables)
                {
                    updateable.Start(unixTime);
                }
            }
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

            foreach (Primitive currentSceneObject in CurrentScene.GroupedPrimitives.GetAllPrimitives()
            )
            {
                // Execute updates per-object
                foreach (IUpdateable updateable in currentSceneObject.Updateables)
                {
                    updateable.Update(lastDeltaTime);
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
            foreach (Primitive gameObject in CurrentScene.GroupedPrimitives.GetAllPrimitives())
            {
                foreach (IUpdateable updateable in gameObject.Updateables)
                {
                    updateable.End(unixTime);
                }
            }

            renderForm.Dispose();
        }
    }
}