// Created by Sakri Koskimies (Github: Saggre) on 24/10/2019

using System.Collections.Generic;
using System.Numerics;
using EconSim.Core.Primitives;

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
        /// List of objects in the scene.
        /// </summary>
        private List<IPrimitive> gameObjects;

        /// <summary>
        /// Initiates an empty scene.
        /// Creates a camera as well.
        /// </summary>
        public Scene()
        {
            activeCamera = new Camera
            {
                Position = new Vector3(0, 0, 0)
            };

            gameObjects = new List<IPrimitive>();
        }

        /// <summary>
        /// Adds an object to the scene
        /// </summary>
        /// <param name="gameObject">The object to add</param>
        public void AddGameObject(IPrimitive gameObject)
        {
            gameObjects.Add(gameObject);
        }

        /// <summary>
        /// Gets the scene's GameObjects
        /// </summary>
        public List<IPrimitive> GameObjects => gameObjects;


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