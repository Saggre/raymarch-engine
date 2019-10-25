// Created by Sakri Koskimies (Github: Saggre) on 02/10/2019

using System.Collections.Generic;
using System.Numerics;
using EconSim.Math;
using SharpDX;
using Quaternion = System.Numerics.Quaternion;
using Vector3 = System.Numerics.Vector3;

namespace EconSim.Core
{
    public class GameObject
    {
        private Vector3 position;
        private Quaternion rotation;
        private Vector3 scale;

        private Mesh mesh;
        private Shader shader;
        private List<IUpdateable> updateables;

        public GameObject()
        {
            position = Vector3.Zero;
            rotation = Quaternion.Identity;
            scale = Vector3.One;
            updateables = new List<IUpdateable>();
        }

        public GameObject(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
            updateables = new List<IUpdateable>();
        }

        public GameObject(Vector3 position, Quaternion rotation, Vector3 scale, Mesh mesh)
        {
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
            this.mesh = mesh;
            updateables = new List<IUpdateable>();
        }

        public GameObject(Mesh mesh)
        {
            this.mesh = mesh;
            position = Vector3.Zero;
            rotation = Quaternion.Identity;
            scale = Vector3.One;
            updateables = new List<IUpdateable>();
        }
        public Shader Shader
        {
            get => shader;
            set => shader = value;
        }

        /// <summary>
        /// Get the Updateables of this GameObject
        /// </summary>
        public List<IUpdateable> Updateables => updateables;

        /// <summary>
        /// Add a Updateable to this GameObject
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
            return Math.Util.AffineTransformation(scale, rotation, position);
        }

        public Mesh Mesh
        {
            get => mesh;
            set => mesh = value;
        }

        public Vector3 Position
        {
            get => position;
            set => position = value;
        }

        public void Move(Vector3 direction, float speed)
        {
            position += direction * speed;
        }

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