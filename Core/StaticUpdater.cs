// Created by Sakri Koskimies (Github: Saggre) on 02/10/2019

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace RaymarchEngine.Core
{
    /// <summary>
    /// Updates scripts implementing IUpdateable
    /// </summary>
    public static class StaticUpdater
    {
        // Updateables that are deferred until no action loop is running, as lists cannot be edited while looping through them
        private static List<IUpdateable> updateablesToBeAdded;

        /// <summary>
        /// Every updateable that receives start, update and end calls
        /// </summary>
        public static List<IUpdateable> updateables;

        /// <summary>
        /// Creates the lists on first use and flushes anything added while a loop was running
        /// </summary>
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

        /// <summary>
        /// Calls Start on every updateable
        /// </summary>
        /// <param name="startTime">Unix timestamp of the moment the engine started</param>
        public static void ExecuteStartActions(int startTime)
        {
            CheckInit();
            foreach (IUpdateable updateable in updateables)
            {
                updateable.Start(startTime);
            }
        }

        /// <summary>
        /// Calls Update on every updateable
        /// </summary>
        /// <param name="deltaTime">Seconds elapsed since the previous frame</param>
        public static void ExecuteUpdateActions(float deltaTime)
        {
            CheckInit();
            foreach (IUpdateable updateable in updateables)
            {
                updateable.Update(deltaTime);
            }
        }

        /// <summary>
        /// Calls End on every updateable
        /// </summary>
        /// <param name="endTime">Unix timestamp of the moment the engine stopped</param>
        public static void ExecuteEndActions(int endTime)
        {
            CheckInit();
            foreach (IUpdateable updateable in updateables)
            {
                updateable.End(endTime);
            }
        }

        /// <summary>
        /// Registers an updateable. It starts receiving calls on the next CheckInit, so adding
        /// from inside an update loop is safe.
        /// </summary>
        /// <param name="updateable">Updateable to register</param>
        public static void Add(IUpdateable updateable)
        {
            CheckInit();
            updateablesToBeAdded.Add(updateable);
        }

    }
}