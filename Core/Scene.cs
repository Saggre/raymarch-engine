// Created by Sakri Koskimies (Github: Saggre) on 24/10/2019

using System.Collections.Generic;

namespace EconSim.Core
{
    public class Scene
    {
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
    }
}