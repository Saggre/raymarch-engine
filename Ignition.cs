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
        [STAThread]
        static void Main(string[] args)
        {
            Start(new GameLogic());
        }

        /// <summary>
        /// Starts the engine with the given game logic
        /// </summary>
        public static void Start(AutoUpdateable gameLogic)
        {
            // Engine.Dispose runs every component's End and releases the D3D device
            using (Engine gameEngine = new Engine(gameLogic))
            {
                gameEngine.Run();
            }
        }
    }
}