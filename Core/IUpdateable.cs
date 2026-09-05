// Created by Sakri Koskimies (Github: Saggre) on 02/10/2019

using System;

namespace RaymarchEngine.Core
{
    /// <summary>
    /// Calls different methods on start, every frame and on dispose
    /// </summary>
    public interface IUpdateable
    {
        /// <summary>
        /// Called when this updateable is added to a gameobject
        /// </summary>
        /// <param name="gameObject">The gameobject this updateable was added to</param>
        void OnAddedToGameObject(GameObject gameObject);

        /// <summary>
        /// Called on engine start
        /// </summary>
        /// <param name="startTime">Unix timestamp of the moment the engine started</param>
        void Start(int startTime);

        /// <summary>
        /// Called every frame
        /// </summary>
        /// <param name="deltaTime">Seconds elapsed since the previous frame</param>
        void Update(float deltaTime);

        /// <summary>
        /// Called on exit
        /// </summary>
        /// <param name="endTime">Unix timestamp of the moment the engine stopped</param>
        void End(int endTime);
    }
}