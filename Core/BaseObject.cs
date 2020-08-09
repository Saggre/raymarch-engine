// Created by Sakri Koskimies (Github: Saggre) on 09/08/2020

using System.Collections.Generic;
using EconSim.EMath;
using SharpDX;
using Quaternion = System.Numerics.Quaternion;
using Vector3 = System.Numerics.Vector3;

namespace EconSim.Core
{
    /// <summary>
    /// A class that represents a physical, visible object in the scene
    /// Must be extended to represent an object with a mesh (GameObject) or a raymarched object (RaymarchGameObject) or something else
    /// </summary>
    public abstract class BaseObject
    {
        protected Vector3 position;
        protected Quaternion rotation;
        protected Vector3 scale;

        protected bool active;

        protected List<IUpdateable> updateables;

        public BaseObject()
        {
            active = true;
            position = Vector3.Zero;
            rotation = Quaternion.Identity;
            scale = Vector3.One;
            updateables = new List<IUpdateable>();
        }

        public BaseObject(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            active = true;
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
            updateables = new List<IUpdateable>();
        }

        public bool Active
        {
            get => active;
            set => active = value;
        }

        /// <summary>
        /// Get the Updateables of this object
        /// </summary>
        public List<IUpdateable> Updateables => updateables;

        /// <summary>
        /// Add a Updateable to this object
        /// </summary>
        /// <param name="updateable"></param>
        public void AddUpdateable(IUpdateable updateable)
        {
            updateables.Add(updateable);
        }

        /// <summary>
        /// Creates this object's model matrix
        /// </summary>
        /// <returns></returns>
        public Matrix ModelMatrix()
        {
            return EMath.Util.AffineTransformation(scale, rotation, position);
        }

        /// <summary>
        /// Get or set the object's position
        /// </summary>
        public Vector3 Position
        {
            get => position;
            set => position = value;
        }

        /// <summary>
        /// Get or set the object's scale
        /// </summary>
        public Vector3 Scale
        {
            get => scale;
            set => scale = value;
        }

        /// <summary>
        /// Moves the object
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="speed"></param>
        public void Move(Vector3 direction, float speed)
        {
            position += direction * speed;
        }

        /// <summary>
        /// The GameObject's rotation in world space
        /// </summary>
        public Quaternion Rotation
        {
            get => rotation;
            set => rotation = value;
        }

        /// <summary>
        /// Add eulerAngles to the current rotation
        /// </summary>
        /// <param name="eulerAngles"></param>
        public void Rotate(Vector3 eulerAngles)
        {
            Quaternion eulerRot = eulerAngles.EulerToQuaternion();
            rotation *= eulerRot;
        }

        /// <summary>
        /// Add eulerAngles to the current rotation
        /// </summary>
        public void Rotate(float x, float y, float z)
        {
            Rotate(new Vector3(x, y, x));
        }

        public Vector3 Right => rotation.Multiply(Vector3.UnitX);

        public Vector3 Up => rotation.Multiply(Vector3.UnitY);

        public Vector3 Forward => rotation.Multiply(Vector3.UnitZ);
    }
}