// Created by Sakri Koskimies (Github: Saggre) on 29/09/2019

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EconSim.Math
{
    public static class Util
    {

        #region Quaternions

        public static Vector3 Multiply(this Quaternion rotation, Vector3 point)
        {
            float num1 = rotation.X * 2f;
            float num2 = rotation.Y * 2f;
            float num3 = rotation.Z * 2f;
            float num4 = rotation.X * num1;
            float num5 = rotation.Y * num2;
            float num6 = rotation.Z * num3;
            float num7 = rotation.X * num2;
            float num8 = rotation.X * num3;
            float num9 = rotation.Y * num3;
            float num10 = rotation.W * num1;
            float num11 = rotation.W * num2;
            float num12 = rotation.W * num3;
            Vector3 vector3;
            vector3.X = (1.0f - (num5 + num6)) * point.X + (num7 - num12) * point.Y + (num8 + num11) * point.Z;
            vector3.Y = (num7 + num12) * point.X + (1.0f - (num4 + num6)) * point.Y + (num9 - num10) * point.Z;
            vector3.Z = (num8 - num11) * point.X + (num9 + num10) * point.Y + (1.0f - (num4 + num5)) * point.Z;
            return vector3;
        }

        /// <summary>
        /// Transforms eulerAngles into a Quaternion
        /// </summary>
        /// <param name="x">Angle in degrees</param>
        /// <param name="y">Angle in degrees</param>
        /// <param name="z">Angle in degrees</param>
        /// <returns></returns>
        public static Quaternion EulerToQuaternion(float x, float y, float z)
        {
            return EulerToQuaternion(new Vector3(x, y, z));
        }

        /// <summary>
        /// Transforms eulerAngles into a Quaternion
        /// </summary>
        /// <param name="eulerAngles">Angles in degrees</param>
        /// <returns></returns>
        public static Quaternion EulerToQuaternion(this Vector3 eulerAngles)
        {
            eulerAngles *= Deg2Rad;
            double yawOver2 = eulerAngles.X * 0.5f;
            float cosYawOver2 = (float)System.Math.Cos(yawOver2);
            float sinYawOver2 = (float)System.Math.Sin(yawOver2);
            double pitchOver2 = eulerAngles.Y * 0.5f;
            float cosPitchOver2 = (float)System.Math.Cos(pitchOver2);
            float sinPitchOver2 = (float)System.Math.Sin(pitchOver2);
            double rollOver2 = eulerAngles.Z * 0.5f;
            float cosRollOver2 = (float)System.Math.Cos(rollOver2);
            float sinRollOver2 = (float)System.Math.Sin(rollOver2);
            Quaternion result;
            result.W = cosYawOver2 * cosPitchOver2 * cosRollOver2 + sinYawOver2 * sinPitchOver2 * sinRollOver2;
            result.X = sinYawOver2 * cosPitchOver2 * cosRollOver2 + cosYawOver2 * sinPitchOver2 * sinRollOver2;
            result.Y = cosYawOver2 * sinPitchOver2 * cosRollOver2 - sinYawOver2 * cosPitchOver2 * sinRollOver2;
            result.Z = cosYawOver2 * cosPitchOver2 * sinRollOver2 - sinYawOver2 * sinPitchOver2 * cosRollOver2;

            return result;
        }

        /// <summary>
        /// Transform quaternion to eulerAngles
        /// </summary>
        /// <param name="q"></param>
        /// <returns></returns>
        public static Vector3 QuaternionToEuler(this Quaternion q)
        {
            Vector3 euler;

            // if the input quaternion is normaliZed, this is eXactlY one. Otherwise, this acts as a correction factor for the quaternion's not-normaliZedness
            float unit = (q.X * q.X) + (q.Y * q.Y) + (q.Z * q.Z) + (q.W * q.W);

            // this will have a magnitude of 0.5 or greater if and onlY if this is a singularitY case
            float test = q.X * q.W - q.Y * q.Z;

            if (test > 0.4995f * unit) // singularitY at north pole
            {
                euler.X = PI / 2;
                euler.Y = 2f * (float)System.Math.Atan2(q.Y, q.X);
                euler.Z = 0;
            }
            else if (test < -0.4995f * unit) // singularitY at south pole
            {
                euler.X = -PI / 2;
                euler.Y = -2f * (float)System.Math.Atan2(q.Y, q.X);
                euler.Z = 0;
            }
            else // no singularitY - this is the majoritY of cases
            {
                euler.X = (float)System.Math.Asin(2f * (q.W * q.X - q.Y * q.Z));
                euler.Y = (float)System.Math.Atan2(2f * q.W * q.Y + 2f * q.Z * q.X, 1 - 2f * (q.X * q.X + q.Y * q.Y));
                euler.Z = (float)System.Math.Atan2(2f * q.W * q.Z + 2f * q.X * q.Y, 1 - 2f * (q.Z * q.Z + q.X * q.X));
            }

            // all the math so far has been done in radians. Before returning, we convert to degrees...
            euler *= Rad2Deg;

            //...and then ensure the degree values are between 0 and 360
            euler.X %= 360;
            euler.Y %= 360;
            euler.Z %= 360;

            return euler;

        }

        public static void RotateAround(this ref Quaternion rotation, Vector3 axis, float angle)
        {
            rotation = rotation * AngleAxis(angle, ref axis);
        }

        /// <summary>
        /// Returns a quaternion rotated degrees around axis
        /// </summary>
        /// <param name="degrees"></param>
        /// <param name="axis"></param>
        /// <returns></returns>
        private static Quaternion AngleAxis(float degrees, ref Vector3 axis)
        {
            if (axis.SqrMagnitude() <= float.Epsilon)
                return Quaternion.Identity;

            Quaternion result = Quaternion.Identity;
            var radians = degrees * Deg2Rad;
            radians *= 0.5f;
            axis.Normalize();
            axis = axis * (float)System.Math.Sin(radians);
            result.X = axis.X;
            result.Y = axis.Y;
            result.Z = axis.Z;
            result.W = (float)System.Math.Cos(radians);

            result.Normalize();
            return result;
        }

        private static void Internal_ToAxisAngleRad(Quaternion q, out Vector3 axis, out float angle)
        {
            if (System.Math.Abs(q.W) > 1.0f)
                q.Normalize();

            angle = 2.0f * (float)System.Math.Acos(q.W); // angle
            float den = (float)System.Math.Sqrt(1.0 - q.W * q.W);
            if (den > 0.0001f)
            {
                axis = new Vector3(q.X, q.Y, q.Z) / den;
            }
            else
            {
                // This occurs when the angle is zero. 
                // Not a problem: just set an arbitrary normalized axis.
                axis = new Vector3(1, 0, 0);
            }
        }

        #endregion

        #region Vectors

        public static float SqrMagnitude(this Vector3 vector)
        {
            return vector.X * vector.X + vector.Y * vector.Y + vector.Z * vector.Z;
        }

        public static float ManhattanDistance(this Vector2 a, Vector2 b)
        {
            return System.Math.Abs(a.X - b.X) + System.Math.Abs(a.Y - b.Y);
        }

        public static float ManhattanDistance(this Vector3 a, Vector3 b)
        {
            return System.Math.Abs(a.X - b.X) + System.Math.Abs(a.Y - b.Y) + System.Math.Abs(a.Z - b.Z);
        }

        public static float ManhattanDistance(this Vector4 a, Vector4 b)
        {
            return System.Math.Abs(a.X - b.X) + System.Math.Abs(a.Y - b.Y) + System.Math.Abs(a.Z - b.Z) + System.Math.Abs(a.W - b.W);
        }

        #endregion

        #region Math and constants

        public static float PI => (float)System.Math.PI;

        /// <summary>
        /// Convert degrees into radians
        /// </summary>
        public static float Deg2Rad => 0.01745329f;

        /// <summary>
        /// Covert radians into degrees
        /// </summary>
        public static float Rad2Deg => 57.29578f;

        /// <summary>
        /// Clamps the input value between 0 and 1
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static float Clamp01(this float a)
        {
            if (a < 0)
            {
                return 0f;
            }

            if (a > 1)
            {
                return 1f;
            }

            return a;
        }

        #endregion

    }
}