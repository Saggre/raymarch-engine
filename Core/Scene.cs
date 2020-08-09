// Created by Sakri Koskimies (Github: Saggre) on 24/10/2019

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
        /// List of objects in the scene.
        /// May contain any subtype of GameObject, like RaymarchGameObject
        /// </summary>
        private List<BaseObject> objects;

        /// <summary>
        /// Initiates an empty scene
        /// </summary>
        public Scene()
        {
            objects = new List<BaseObject>();
        }

        /// <summary>
        /// Adds a GameObject to the scene
        /// </summary>
        /// <param name="baseObject"></param>
        public void AddObject(BaseObject baseObject)
        {
            objects.Add(baseObject);
        }

        /// <summary>
        /// Gets the scene's GameObjects
        /// </summary>
        public List<BaseObject> Objects => objects;

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