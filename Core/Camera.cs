// Created by Sakri Koskimies (Github: Saggre) on 22/10/2019

using SharpDX;
using System.Numerics;

namespace EconSim.Core
{
    public class Camera : GameObject
    {
        /// <summary>
        /// Returns the camera view matrix
        /// </summary>
        /// <returns></returns>
        public Matrix ViewMatrix()
        {
            SharpDX.Vector3 pos = new SharpDX.Vector3(Position.X, Position.Y, Position.Z);
            return Matrix.LookAtLH(pos, pos + SharpDX.Vector3.ForwardLH, SharpDX.Vector3.Up);
            return Matrix.RotationQuaternion(new SharpDX.Quaternion(Rotation.X, Rotation.Y, Rotation.Z, Rotation.W));
        }
    }
}