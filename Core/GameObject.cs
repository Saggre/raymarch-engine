// Created by Sakri Koskimies (Github: Saggre) on 09/08/2020

using System.Collections.Generic;
using System.Linq;
using RaymarchEngine.EMath;
using SharpDX;
using Quaternion = System.Numerics.Quaternion;
using Vector3 = System.Numerics.Vector3;

namespace RaymarchEngine.Core
{
    /// <summary>
    /// A positioned object in the scene. What it does comes from the components attached to it,
    /// for example a RaymarchRenderer to give it a shape the raymarcher can draw.
    /// </summary>
    public class GameObject
    {
        private readonly List<GameObject> children;
        private GameObject parent;

        private bool active;

        private readonly Movement movement;

        private readonly List<IComponent> components;

        /// <summary>
        /// Create new GameObject
        /// </summary>
        public GameObject()
        {
            components = new List<IComponent>();
            movement = new Movement();
            AddComponent(movement);
            
            active = true;
            children = new List<GameObject>();
        }

        /// <summary>
        /// Create new GameObject at position
        /// </summary>
        /// <param name="position">World space position</param>
        public GameObject(Vector3 position)
        {
            components = new List<IComponent>();
            movement = new Movement(position, Quaternion.Identity, Vector3.One);
            AddComponent(movement);
            
            active = true;
            children = new List<GameObject>();
        }

        /// <summary>
        /// Create new GameObject with components
        /// </summary>
        /// <param name="components">Components to attach</param>
        public GameObject(params IComponent[] components)
        {
            this.components = components.ToList();
            movement = new Movement();
            AddComponent(movement);
            
            active = true;
            children = new List<GameObject>();
            foreach (IComponent component in this.components)
            {
                component.OnAddedToGameObject(this);
            }
        }

        /// <summary>
        /// Create new GameObject with components at position
        /// </summary>
        /// <param name="position">World space position</param>
        /// <param name="components">Components to attach</param>
        public GameObject(Vector3 position, params IComponent[] components)
        {
            this.components = components.ToList();
            movement = new Movement(position, Quaternion.Identity, Vector3.One);
            AddComponent(movement);
            
            active = true;
            children = new List<GameObject>();
            foreach (IComponent component in this.components)
            {
                component.OnAddedToGameObject(this);
            }
        }

        /// <summary>
        /// Create new GameObject with a full transform
        /// </summary>
        /// <param name="position">World space position</param>
        /// <param name="rotation">World space rotation</param>
        /// <param name="scale">Scale along each axis</param>
        public GameObject(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            components = new List<IComponent>();
            movement = new Movement(position, rotation, scale);
            AddComponent(movement);
            
            active = true;
            children = new List<GameObject>();
        }

        /// <summary>
        /// Gets the Movement component
        /// </summary>
        public Movement Movement => movement;

        /// <summary>
        /// Get this GameObject's children
        /// </summary>
        public IEnumerable<GameObject> Children => children.ToArray();

        /// <summary>
        /// Get this GameObject's parent
        /// </summary>
        public GameObject Parent => parent;

        /// <summary>
        /// Called by child GameObject when it sets this as its parent
        /// </summary>
        /// <param name="child">The GameObject that adopted this one as its parent</param>
        private void OnSetParent(GameObject child)
        {
            children.Add(child);
        }

        /// <summary>
        /// Called by parent GameObject when it sets this as its child
        /// </summary>
        /// <param name="parent">The new parent</param>
        private void OnAddedChild(GameObject parent)
        {
            this.parent = parent;
        }

        /// <summary>
        /// Called by parent GameObject when it removes this as its child
        /// </summary>
        /// <param name="parent">The GameObject this one was detached from</param>
        private void OnRemovedChild(GameObject parent)
        {
            this.parent = null;
        }

        /// <summary>
        /// Set this GameObject's parent, and add this to the parent's children
        /// </summary>
        /// <param name="parent">The GameObject to attach to</param>
        public void SetParent(GameObject parent)
        {
            this.parent = parent;
            parent.OnSetParent(this);
        }

        /// <summary>
        /// Remove a child GameObject and clear its parent
        /// </summary>
        /// <param name="child">The child to detach</param>
        public void RemoveChild(GameObject child)
        {
            children.Remove(child);
            child.OnRemovedChild(this);
        }

        /// <summary>
        /// Adds a child GameObject and sets this as its parent
        /// </summary>
        /// <param name="child">The GameObject to attach</param>
        public void AddChild(GameObject child)
        {
            children.Add(child);
            child.OnAddedChild(this);
        }

        /// <summary>
        /// Enable or disable the GameObject
        /// </summary>
        public bool Active
        {
            get => active;
            set => active = value;
        }

        /// <summary>
        /// Get this GameObject's components
        /// </summary>
        public IEnumerable<IComponent> Components => components.ToArray();

        /// <summary>
        /// Gets the first attached component assignable to T
        /// </summary>
        /// <typeparam name="T">Component type to look for</typeparam>
        /// <returns>The matching component</returns>
        /// <exception cref="System.InvalidOperationException">No component of type T is attached</exception>
        public T GetComponent<T>()
        {
            foreach (IComponent component in components)
            {
                if (component is T)
                {
                    return (T) component;
                }
            }

            throw new System.InvalidOperationException($"No component {typeof(T).Name} in GameObject");
        }

        /// <summary>
        /// Add a component to this GameObject
        /// </summary>
        /// <param name="component">Component to add</param>
        public void AddComponent(IComponent component)
        {
            components.Add(component);
            component.OnAddedToGameObject(this);
        }
    }
}