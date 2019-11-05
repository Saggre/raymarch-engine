// Created by Sakri Koskimies (Github: Saggre) on 24/10/2019

using System.Collections.Generic;

namespace EconSim.Core
{
    public class Scene
    {
        // The camera that is currently used in rendering this scene
        private Camera activeCamera = null;
        private List<GameObject> gameObjects;

        public Scene()
        {
            gameObjects = new List<GameObject>();
        }

        public void AddGameObject(GameObject gameObject)
        {
            gameObjects.Add(gameObject);
        }

        public List<GameObject> GameObjects => gameObjects;

        public Camera ActiveCamera
        {
            get => activeCamera;
            set => activeCamera = value;
        }
    }
}