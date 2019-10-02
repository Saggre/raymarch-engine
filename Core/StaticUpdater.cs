// Created by Sakri Koskimies (Github: Saggre) on 02/10/2019

using System;
using System.Collections.Generic;

namespace EconSim.Core
{
    public static class StaticUpdater
    {
        public static List<IUpdateable> updateables;

        public static void CheckList()
        {
            if (updateables == null)
            {
                updateables = new List<IUpdateable>();
            }
        }

        public static void ExecuteStartActions()
        {
            CheckList();
            foreach (var updateable in updateables)
            {
                updateable.Start();
            }
        }

        public static void ExecuteUpdateActions()
        {
            CheckList();
            foreach (var updateable in updateables)
            {
                updateable.Update();
            }
        }

        public static void ExecuteEndActions()
        {
            CheckList();
            foreach (var updateable in updateables)
            {
                updateable.End();
            }
        }

        public static void Add(IUpdateable updateable)
        {
            CheckList();
            updateables.Add(updateable);
        }

    }
}