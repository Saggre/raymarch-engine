// Created by Sakri Koskimies (Github: Saggre) on 02/10/2019

using Microsoft.Xna.Framework;
using EconSim.Math;

namespace EconSim.Core
{
    public class GameObject
    {
        private Vector3 position;
        private Quaternion rotation;

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
            //localRotation = localRotation * eulerRot;

            rotation *= (Quaternion.Inverse(rotation) * eulerRot * rotation);
        }

        /// <summary>
        /// Add eulerAngles to the current rotation
        /// </summary>
        public void Rotate(float x, float y, float z)
        {
            Rotate(new Vector3(x, y, x));
        }

        public Vector3 Right => rotation.Multiply(Vector3.Right);

        public Vector3 Up => rotation.Multiply(Vector3.Up);

        public Vector3 Forward => rotation.Multiply(Vector3.Forward);
    }
}