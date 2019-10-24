// Created by Sakri Koskimies (Github: Saggre) on 02/10/2019

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace EconSim.Core
{
    /// <summary>
    /// Updates scripts implementing IUpdateable
    /// </summary>
    public static class StaticUpdater
    {
        public static List<IUpdateable> updateables;

        public static void CheckInit()
        {
            if (updateables == null)
            {
                updateables = new List<IUpdateable>();
            }
        }

        public static void ExecuteStartActions(DateTime startTime)
        {
            CheckInit();
            foreach (IUpdateable updateable in updateables)
            {
                updateable.Start(startTime);
            }
        }

        public static void ExecuteUpdateActions(float deltaTime)
        {
            CheckInit();
            foreach (IUpdateable updateable in updateables)
            {
                updateable.Update(deltaTime);
            }
        }

        public static void ExecuteEndActions(DateTime endTime)
        {
            CheckInit();
            foreach (IUpdateable updateable in updateables)
            {
                updateable.End(endTime);
            }
        }

        public static void Add(IUpdateable updateable)
        {
            CheckInit();
            updateables.Add(updateable);
        }

    }
}