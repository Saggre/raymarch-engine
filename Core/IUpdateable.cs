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
        /// <param name="parent"></param>
        void OnAddedToGameObject(GameObject parent);

        /// <summary>
        /// Called on engine start
        /// </summary>
        /// <param name="startTime"></param>
        void Start(int startTime);

        /// <summary>
        /// Called every frame
        /// </summary>
        /// <param name="deltaTime"></param>
        void Update(float deltaTime);

        /// <summary>
        /// Called on exit
        /// </summary>
        /// <param name="endTime"></param>
        void End(int endTime);
    }
}