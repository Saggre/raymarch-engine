// Created by Sakri Koskimies (Github: Saggre) on 22/10/2019

using System.Numerics;
using System;

namespace EconSim.Core
{
    /// <summary>
    /// Class that is used to represent a camera
    /// </summary>
    public class Camera : GameObject
    {
        float aspectRatio;
        float nearClipPlane;
        float farClipPlane;
        float fieldOfView;

        public Camera()
        {
            aspectRatio = Engine.AspectRatio();
            nearClipPlane = 0.1f;
            farClipPlane = 1000f;
            fieldOfView = (float) (Math.PI / 4.0f);
        }

        /// <summary>
        /// Returns the camera view matrix
        /// </summary>
        /// <returns></returns>
        public Matrix4x4 ViewMatrix()
        {
            return Matrix4x4.Transpose(Matrix4x4.CreateLookAt(Position, Position + Forward, Vector3.UnitY));
        }
    }
}