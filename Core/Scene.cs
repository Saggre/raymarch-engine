// Created by Sakri Koskimies (Github: Saggre) on 24/10/2019

using System;
using System.Collections.Generic;
using System.Numerics;
using RaymarchEngine.Core.Primitives;
using Plane = RaymarchEngine.Core.Primitives.Plane;

namespace RaymarchEngine.Core
{
    /*public interface GPUReservedPrimitives
    {
        public bool[] UserControlled();
        public Primitive[] Primitives();
        public void SetUserControlled(int index);
    }

    public class GPUReservedPrimitives<T> : GPUReservedPrimitives where T : Primitive, new()
    {
        public bool[] userControlled;
        public T[] primitives;

        public GPUReservedPrimitives()
        {
            int size = Engine.PrimitiveCount<T>();

            primitives = new T[size];
            userControlled = new bool[size];

            for (int i = 0; i < size; i++)
            {
                primitives[i] = new T();
            }
        }

        public bool[] UserControlled()
        {
            return userControlled;
        }

        public Primitive[] Primitives()
        {
            return primitives;
        }

        public void SetUserControlled(int index)
        {
            userControlled[index] = true;
        }
    }

    public class GPUReservedGroupedPrimitives
    {
        private Dictionary<Type, GPUReservedPrimitives> primitives;
        private List<Primitive> ungroudpedPrimitives;

        public GPUReservedGroupedPrimitives()
        {
            ungroudpedPrimitives = new List<Primitive>();
            primitives = new Dictionary<Type, GPUReservedPrimitives>
            {
                {typeof(Sphere), new GPUReservedPrimitives<Sphere>()},
                {typeof(Box), new GPUReservedPrimitives<Box>()},
                {typeof(Plane), new GPUReservedPrimitives<Plane>()},
                {typeof(Ellipsoid), new GPUReservedPrimitives<Ellipsoid>()},
                {typeof(Torus), new GPUReservedPrimitives<Torus>()},
                {typeof(CappedTorus), new GPUReservedPrimitives<CappedTorus>()}
            };
        }

        public List<Primitive> UngroudpedPrimitives => ungroudpedPrimitives;

        public void AddPrimitive(Primitive primitive)
        {
            Type primitiveType = primitive.GetType();
            GPUReservedPrimitives primitiveShapes = primitives[primitiveType];

            for (int i = 0; i < primitiveShapes.UserControlled().Length; i++)
            {
                if (primitiveShapes.UserControlled()[i])
                {
                    continue;
                }

                primitiveShapes.Primitives()[i] = primitive;
                primitiveShapes.SetUserControlled(i);
                ungroudpedPrimitives.Add(primitive);
                return;
            }

            // No available spaces
            throw new System.InvalidOperationException(
                "No space left for another sphere. Add more room for spheres!");
        }

        public T[] GetPrimitivesOfType<T>() where T : Primitive
        {
            return primitives[typeof(T)].Primitives() as T[];
        }

        public Primitive[] GetAllPrimitives()
        {
            return ungroudpedPrimitives.ToArray();
        }
    }*/

    /// <summary>
    /// A class that represents the physical space where objects can exist
    /// </summary>
    public class Scene
    {
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