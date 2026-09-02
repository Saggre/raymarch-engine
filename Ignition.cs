// Created by Sakri Koskimies (Github: Saggre) on 29/09/2019

using System;
using RaymarchEngine.Core;

namespace RaymarchEngine
{
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Ignition
    {
        static void Main(string[] args)
        {
            Start(new GameLogic());
        }

        /// <summary>
        /// The main entry point for the application.
        /// Starts the engine :)
        /// </summary>
        [STAThread]
        public static void Start(AutoUpdateable gameLogic)
        {
            // The using is what makes every component's End method run, and what releases the
            // D3D device. Without it Engine.Dispose is never reached.
            using (Engine gameEngine = new Engine(gameLogic))
            {
                gameEngine.Run();
            }
        }
    }
}