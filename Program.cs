// Created by Sakri Koskimies (Github: Saggre) on 29/09/2019

using System;

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
        static void Main()
        {
            using EconSim game = new EconSim();
            game.Run();
        }
    }
//#endif
}
