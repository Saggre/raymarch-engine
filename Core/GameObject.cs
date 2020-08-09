// Created by Sakri Koskimies (Github: Saggre) on 09/08/2020

using System.Collections.Generic;
using EconSim.EMath;
using SharpDX;
using Quaternion = System.Numerics.Quaternion;
using Vector3 = System.Numerics.Vector3;

namespace EconSim.Core
{
    /// <summary>
    /// The shape that this object is rendered as
    /// </summary>
    public enum RaymarchShape
    {
        /// <summary>
        /// A sphere
        /// </summary>
        Sphere,

        /// <summary>
        /// A box
        /// </summary>
        Box,

        /// <summary>
        /// An infinite plane
        /// </summary>
        Plane,

        /// <summary>
        /// An ellipsoid
        /// </summary>
        Ellipsoid,

        /// <summary>
        /// A torus ;)
        /// </summary>
        Torus
    }

    /// <summary>
    /// Data that is passed to the raymarch shader.
    /// Euler angles is passed instead of quaternion rotation, because the object can't be/shouldn't be rotated in the shader. Euler angle data will suffice for rendering the shape at different rotations.
    /// </summary>
    public struct RaymarchGameObjectBufferData
    {
        public int raymarchShape;
        public Vector3 position;
        public Vector3 eulerAngles;
        public Vector3 scale;

        public Vector3 primitiveOptions;
        public Vector4 color;
        public Vector3 materialOptions;

        public RaymarchGameObjectBufferData(RaymarchShape raymarchShape, Vector3 position, Vector3 eulerAngles,
            Vector3 scale)
        {
            this.raymarchShape = (int) raymarchShape;
            this.position = position;
            this.eulerAngles = eulerAngles;
            this.scale = scale;
            primitiveOptions = Vector3.One;
            color = Vector4.One;
            materialOptions = Vector3.Zero;
        }
    }

    /// <summary>
    /// A class that represents a physical, visible object in the scene
    /// Must be extended to represent an object with a mesh (GameObject) or a raymarched object (RaymarchGameObject) or something else
    /// </summary>
    public class GameObject
    {
        private Vector3 position;
        private Quaternion rotation;
        private Vector3 scale;

        private bool active;

        private List<IUpdateable> updateables;

        private RaymarchShape shape;

        RaymarchShape Shape => shape;

        public GameObject()
        {
            shape = RaymarchShape.Sphere;
            active = true;
            position = Vector3.Zero;
            rotation = Quaternion.Identity;
            scale = Vector3.One;
            updateables = new List<IUpdateable>();
        }

        public GameObject(RaymarchShape shape)
        {
            this.shape = shape;
            active = true;
            position = Vector3.Zero;
            rotation = Quaternion.Identity;
            scale = Vector3.One;
            updateables = new List<IUpdateable>();
        }

        public GameObject(RaymarchShape shape, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            this.shape = shape;
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

        /// <summary>
        /// Get data that can be sent to the raymarch shader to represent this object
        /// </summary>
        /// <returns></returns>
        public RaymarchGameObjectBufferData GetBufferData()
        {
            return new RaymarchGameObjectBufferData(shape, Position, Rotation.QuaternionToEuler(), Scale);
        }

        public Vector3 Right => rotation.Multiply(Vector3.UnitX);

        public Vector3 Up => rotation.Multiply(Vector3.UnitY);

        public Vector3 Forward => rotation.Multiply(Vector3.UnitZ);
    }
}