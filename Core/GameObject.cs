// Created by Sakri Koskimies (Github: Saggre) on 02/10/2019

using System.Numerics;
using EconSim.Math;

namespace EconSim.Core
{
    public class GameObject
    {
        private Vector3 position;
        private Quaternion rotation;
        private Vector3 scale;

        public GameObject()
        {
            position = Vector3.Zero;
            rotation = Quaternion.Identity;
        }

        public GameObject(Vector3 position, Quaternion rotation)
        {
            this.position = position;
            this.rotation = rotation;
        }

        /// <summary>
        /// Creates this object's model matrix;
        /// </summary>
        /// <returns></returns>
        public Matrix4x4 ModelMatrix()
        {
            Matrix4x4 modelMatrix = Matrix4x4.Identity;
            modelMatrix *= Matrix4x4.CreateTranslation(position);
            modelMatrix *= Matrix4x4.CreateFromQuaternion(rotation);
            modelMatrix *= Matrix4x4.CreateScale(scale);
            return modelMatrix;
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