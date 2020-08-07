// Created by Sakri Koskimies (Github: Saggre) on 22/10/2019

using Matrix = SharpDX.Matrix;
using System.Numerics;
using System;

namespace EconSim.Core
{
    public class Camera : GameObject
    {

        public float nearClipPlane = 0.1f;
        public float farClipPlane = 1000f;
        public float fov = (float)(Math.PI / 2.0f);

        public Camera()
        {
            fov = (float)(Math.PI / 2.0f);
        }

        public Camera(float fov)
        {
            this.fov = fov;
        }

        /// <summary>
        /// Returns the camera view matrix
        /// </summary>
        /// <returns></returns>
        public Matrix ViewMatrix()
        {
            Matrix fovMatrix = Matrix.PerspectiveFovLH(fov, Engine.RenderDevice.AspectRatio(), nearClipPlane, farClipPlane);
            return LookAtLH(Position, Position + Forward, Vector3.UnitY) * fovMatrix;
        }

        /// <summary>
        /// Creates a left-handed, look-at matrix from System.Numerics Vectors
        /// </summary>
        /// <param name="eye">The position of the viewer's eye.</param>
        /// <param name="target">The camera look-at target.</param>
        /// <param name="up">The camera's up vector.</param>
        /// <param name="result">When the method completes, contains the created look-at matrix.</param>
        public static Matrix LookAtLH(Vector3 eye, Vector3 target, Vector3 up)
        {
            Vector3 xaxis, yaxis, zaxis;
            zaxis = Vector3.Subtract(target, eye);
            zaxis = Vector3.Normalize(zaxis);
            xaxis = Vector3.Cross(up, zaxis);
            xaxis = Vector3.Normalize(xaxis);
            yaxis = Vector3.Cross(zaxis, xaxis);

            Matrix result = Matrix.Identity;

            result.M11 = xaxis.X; result.M21 = xaxis.Y; result.M31 = xaxis.Z;
            result.M12 = yaxis.X; result.M22 = yaxis.Y; result.M32 = yaxis.Z;
            result.M13 = zaxis.X; result.M23 = zaxis.Y; result.M33 = zaxis.Z;

            result.M41 = Vector3.Dot(xaxis, eye);
            result.M42 = Vector3.Dot(yaxis, eye);
            result.M43 = Vector3.Dot(zaxis, eye);

            result.M41 = -result.M41;
            result.M42 = -result.M42;
            result.M43 = -result.M43;

            return result;
        }
    }
}