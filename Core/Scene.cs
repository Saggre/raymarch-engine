﻿// Created by Sakri Koskimies (Github: Saggre) on 24/10/2019

using System.Collections.Generic;

namespace EconSim.Core
{
    /// <summary>
    /// A class that represents the physical space where objects can exist
    /// </summary>
    public class Scene
    {
        /// <summary>
        /// The camera that is currently used in rendering this scene
        /// </summary>
        private Camera activeCamera = null;

        /// <summary>
        /// List of GameObjects in the scene.
        /// May contain any subtype of GameObject, like RaymarchGameObject
        /// </summary>
        private List<GameObject> gameObjects;

        /// <summary>
        /// Initiates an empty scene
        /// </summary>
        public Scene()
        {
            gameObjects = new List<GameObject>();
        }

        /// <summary>
        /// Adds a GameObject to the scene
        /// </summary>
        /// <param name="gameObject"></param>
        public void AddGameObject(GameObject gameObject)
        {
            gameObjects.Add(gameObject);
        }

        /// <summary>
        /// Gets the scene's GameObjects
        /// </summary>
        public List<GameObject> GameObjects => gameObjects;

        /// <summary>
        /// Sets or gets the active camera, currently used for rendering.
        /// Active cameras are per-scene, and if you were to change the active scene, the new scene's active camera would be used.
        /// </summary>
        public Camera ActiveCamera
        {
            get => activeCamera;
            set => activeCamera = value;
        }
    }
}