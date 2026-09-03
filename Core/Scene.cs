// Created by Sakri Koskimies (Github: Saggre) on 24/10/2019

using System.Collections.Generic;

namespace RaymarchEngine.Core
{
    /// <summary>
    /// A class that represents the physical space where objects can exist
    /// </summary>
    public class Scene
    {
        private static Scene currentScene;

        public static Scene CurrentScene
        {
            get => currentScene;
            set => currentScene = value;
        }

        /// <summary>
        /// The camera that is currently used in rendering this scene
        /// </summary>
        private Camera activeCamera;

        private readonly List<GameObject> gameObjects;

        /// <summary>
        /// Initiates an empty scene.
        /// Creates a camera as well.
        /// </summary>
        public Scene()
        {
            gameObjects = new List<GameObject>();

            activeCamera = new Camera();

            AddGameObject(activeCamera);
        }

        /// <summary>
        /// Adds a gameobject to the scene
        /// </summary>
        /// <param name="gameObject"></param>
        public void AddGameObject(GameObject gameObject)
        {
            gameObjects.Add(gameObject);
        }

        /// <summary>
        /// Gets gameobjects in this scene
        /// </summary>
        /// <returns></returns>
        public IEnumerable<GameObject> GameObjects => gameObjects.ToArray();

        /// <summary>
        /// Get a list of components in this scene's gameobjects
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T[] Components<T>() where T : IComponent
        {
            List<T> components = new List<T>();

            foreach (GameObject gameObject in gameObjects)
            {
                foreach (IComponent component in gameObject.Components)
                {
                    if (component is T)
                    {
                        components.Add((T) component);
                    }
                }
            }

            return components.ToArray();
        }

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