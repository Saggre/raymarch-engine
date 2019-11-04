// Created by Sakri Koskimies (Github: Saggre) on 22/10/2019

using Matrix = SharpDX.Matrix;
using System.Numerics;
using EconSim.EMath;
using System;

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
            return LookAtLH(Position, Position + Forward, Vector3.UnitY);

            Console.WriteLine(EMath.Util.QuaternionToEuler(Rotation));
            //return RotationQuaternion(Rotation);

            Matrix lookAt = LookAtLH(Position, Vector3.Zero, Vector3.UnitY);
            SharpDX.Quaternion rot = new SharpDX.Quaternion();
            SharpDX.Vector3 v3 = new SharpDX.Vector3();
            lookAt.Decompose(out v3, out rot, out v3);
            Console.WriteLine(EMath.Util.QuaternionToEuler(new Quaternion(rot.X, rot.Y, rot.Z, rot.W)));

            return lookAt;
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