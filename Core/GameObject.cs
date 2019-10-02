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

        public void Rotate(Vector3 eulerAngles)
        {
            Quaternion eulerRot = Math.Util.Euler(eulerAngles);
            //localRotation = localRotation * eulerRot;

            rotation *= (Quaternion.Inverse(rotation) * eulerRot * rotation);
        }

        public Vector3 Right => rotation.Multiply(Vector3.Right);

        public Vector3 Up => rotation.Multiply(Vector3.Up);

        public Vector3 Forward => rotation.Multiply(Vector3.Forward);
    }
}