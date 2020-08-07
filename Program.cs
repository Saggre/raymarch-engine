// Created by Sakri Koskimies (Github: Saggre) on 29/09/2019

using System;
using EconSim.Core;

namespace EconSim
{
    //#if WINDOWS || LINUX
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {
            using Engine gameEngine = new Engine();
            gameEngine.Run();
        }
    }
//#endif
}
