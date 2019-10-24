// Created by Sakri Koskimies (Github: Saggre) on 02/10/2019

using System;

namespace EconSim.Core
{

    /// <summary>
    /// Calls different methods on start, every frame and on dispose
    /// </summary>
    public interface IUpdateable
    {
        void Start(DateTime startTime);
        void Update(float deltaTime);
        void End(DateTime endTime);
    }
}