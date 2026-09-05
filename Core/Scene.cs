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

        /// <summary>
        /// The scene that is rendered and updated. The engine sets this on startup.
        /// </summary>
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
        /// <param name="gameObject">The gameobject to add</param>
        public void AddGameObject(GameObject gameObject)
        {
            gameObjects.Add(gameObject);
        }

        /// <summary>
        /// The gameobjects in this scene, as a copy so the scene can be modified while iterating
        /// </summary>
        public IEnumerable<GameObject> GameObjects => gameObjects.ToArray();

        /// <summary>
        /// Collects every component of type T attached to this scene's gameobjects
        /// </summary>
        /// <typeparam name="T">Component type to look for</typeparam>
        /// <returns>The matching components, empty if there are none</returns>
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