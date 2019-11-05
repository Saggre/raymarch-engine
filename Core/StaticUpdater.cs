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
        // Updateables that are deferred until no action loop is running, as lists cannot be edited while looping through them
        private static List<IUpdateable> updateablesToBeAdded;
        public static List<IUpdateable> updateables;

        public static void CheckInit()
        {
            if (updateables == null)
            {
                updateablesToBeAdded = new List<IUpdateable>();
                updateables = new List<IUpdateable>();
            }

            // Add deferred Updateables
            foreach (IUpdateable updateable in updateablesToBeAdded)
            {
                updateables.Add(updateable);
            }
            updateablesToBeAdded.Clear();

        }

        public static void ExecuteStartActions(int startTime)
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

        public static void ExecuteEndActions(int endTime)
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
            updateablesToBeAdded.Add(updateable);
        }

    }
}