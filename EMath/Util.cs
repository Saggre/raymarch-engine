// Created by Sakri Koskimies (Github: Saggre) on 29/09/2019

using System;
using SharpDX;
using Quaternion = System.Numerics.Quaternion;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;
using Vector4 = System.Numerics.Vector4;


namespace RaymarchEngine.EMath
{
    public static class Util
    {
        public static float MinComponent(this Vector3 vec)
        {
            return Math.Min(Math.Min(vec.X, vec.Y), vec.Z);
        }

        public static float MaxComponent(this Vector3 vec)
        {
            return Math.Max(Math.Max(vec.X, vec.Y), vec.Z);
        }

        public static Vector4 ToVector4(this Vector3 xyz, float w)
        {
            return new Vector4(xyz.X, xyz.Y, xyz.Z, w);
        }

        public static Vector4 ToVector4(this Vector2 xy, float z, float w)
        {
            return new Vector4(xy.X, xy.Y, z, w);
        }


        #region Inter-namespace

        /// <summary>
        /// Creates a SharpDX 3D affine transformation matrix from Numerics components
        /// </summary>
        /// <param name="scaling">Scaling factor.</param>
        /// <param name="rotation">The rotation of the transformation.</param>
        /// <param name="translation">The translation factor of the transformation.</param>
        public static Matrix AffineTransformation(Vector3 scaling, Quaternion rotation, Vector3 translation)
        {
            return Matrix.Scaling(scaling.X, scaling.Y, scaling.Z) * RotationQuaternion(rotation) *
                   Matrix.Translation(translation.X, translation.Y, translation.Z);
        }

        /// <summary>
        /// Creates a SharpDX rotation matrix from a Numerics Quaternion.
        /// </summary>
        /// <param name="rotation">The quaternion to use to build the matrix.</param>
        public static Matrix RotationQuaternion(Quaternion rotation)
        {
            Matrix result = Matrix.Identity;

            float xx = rotation.X * rotation.X;
            float yy = rotation.Y * rotation.Y;
            float zz = rotation.Z * rotation.Z;
            float xy = rotation.X * rotation.Y;
            float zw = rotation.Z * rotation.W;
            float zx = rotation.Z * rotation.X;
            float yw = rotation.Y * rotation.W;
            float yz = rotation.Y * rotation.Z;
            float xw = rotation.X * rotation.W;

            result.M11 = 1.0f - (2.0f * (yy + zz));
            result.M12 = 2.0f * (xy + zw);
            result.M13 = 2.0f * (zx - yw);
            result.M21 = 2.0f * (xy - zw);
            result.M22 = 1.0f - (2.0f * (zz + xx));
            result.M23 = 2.0f * (yz + xw);
            result.M31 = 2.0f * (zx + yw);
            result.M32 = 2.0f * (yz - xw);
            result.M33 = 1.0f - (2.0f * (yy + xx));

            return result;
        }

        #endregion

        #region Quaternions

        public static Vector3 Multiply(this Vector3 vector, Quaternion rotation)
        {
            return Multiply(rotation, vector);
        }

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
            float cosYawOver2 = (float) System.Math.Cos(yawOver2);
            float sinYawOver2 = (float) System.Math.Sin(yawOver2);
            double pitchOver2 = eulerAngles.Y * 0.5f;
            float cosPitchOver2 = (float) System.Math.Cos(pitchOver2);
            float sinPitchOver2 = (float) System.Math.Sin(pitchOver2);
            double rollOver2 = eulerAngles.Z * 0.5f;
            float cosRollOver2 = (float) System.Math.Cos(rollOver2);
            float sinRollOver2 = (float) System.Math.Sin(rollOver2);
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
                euler.Y = 2f * (float) System.Math.Atan2(q.Y, q.X);
                euler.Z = 0;
            }
            else if (test < -0.4995f * unit) // singularitY at south pole
            {
                euler.X = -PI / 2;
                euler.Y = -2f * (float) System.Math.Atan2(q.Y, q.X);
                euler.Z = 0;
            }
            else // no singularitY - this is the majoritY of cases
            {
                euler.X = (float) System.Math.Asin(2f * (q.W * q.X - q.Y * q.Z));
                euler.Y = (float) System.Math.Atan2(2f * q.W * q.Y + 2f * q.Z * q.X, 1 - 2f * (q.X * q.X + q.Y * q.Y));
                euler.Z = (float) System.Math.Atan2(2f * q.W * q.Z + 2f * q.X * q.Y, 1 - 2f * (q.Z * q.Z + q.X * q.X));
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
            rotation *= AngleAxis(angle, ref axis);
        }

        /// <summary>
        /// Returns the angle in degrees between two rotations a and b
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public static float Angle(this Quaternion a, Quaternion b)
        {
            float f = Quaternion.Dot(a, b);
            return (float) System.Math.Acos(System.Math.Min(System.Math.Abs(f), 1f)) * 2f * Rad2Deg;
        }

        /// <summary>
        /// Returns a quaternion rotated degrees around axis
        /// </summary>
        /// <param name="degrees"></param>
        /// <param name="axis"></param>
        /// <returns></returns>
        public static Quaternion AngleAxis(float degrees, ref Vector3 axis)
        {
            if (AlmostZero(axis.SqrMagnitude()))
                return Quaternion.Identity;

            Quaternion result = Quaternion.Identity;
            var radians = degrees * Deg2Rad;
            radians *= 0.5f;
            axis = Vector3.Normalize(axis);
            axis = axis * (float) System.Math.Sin(radians);
            result.X = axis.X;
            result.Y = axis.Y;
            result.Z = axis.Z;
            result.W = (float) System.Math.Cos(radians);

            result = Quaternion.Normalize(result);
            return result;
        }

        /// <summary>
        /// Creates a rotation which rotates from fromDirection to toDirection
        /// </summary>
        /// <param name="fromDirection"></param>
        /// <param name="toDirection"></param>
        public static Quaternion FromToRotation(Vector3 fromDirection, Vector3 toDirection)
        {
            return RotateTowards(LookRotation(fromDirection), LookRotation(toDirection), float.MaxValue);
        }

        /// <summary>
        /// Spherically interpolates between a and b by t. The parameter t is not clamped.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="t"></param>
        private static Quaternion SlerpUnclamped(ref Quaternion a, ref Quaternion b, float t)
        {
            // if either input is zero, return the other.
            if (AlmostZero(a.LengthSquared()))
            {
                if (AlmostZero(b.LengthSquared()))
                {
                    return Quaternion.Identity;
                }

                return b;
            }
            else if (AlmostZero(b.LengthSquared()))
            {
                return a;
            }

            float cosHalfAngle = a.W * b.W + Vector3.Dot(a.XYZ(), b.XYZ());

            if (cosHalfAngle >= 1.0f || cosHalfAngle <= -1.0f)
            {
                // angle = 0.0f, so just return one input.
                return a;
            }
            else if (cosHalfAngle < 0.0f)
            {
                b.XYZ(-b.XYZ());
                b.W = -b.W;
                cosHalfAngle = -cosHalfAngle;
            }

            float blendA;
            float blendB;
            if (cosHalfAngle < 0.99f)
            {
                // do proper slerp for big angles
                float halfAngle = (float) System.Math.Acos(cosHalfAngle);
                float sinHalfAngle = (float) System.Math.Sin(halfAngle);
                float oneOverSinHalfAngle = 1.0f / sinHalfAngle;
                blendA = (float) System.Math.Sin(halfAngle * (1.0f - t)) * oneOverSinHalfAngle;
                blendB = (float) System.Math.Sin(halfAngle * t) * oneOverSinHalfAngle;
            }
            else
            {
                // do lerp if angle is really small.
                blendA = 1.0f - t;
                blendB = t;
            }

            Quaternion result = new Quaternion(blendA * a.XYZ() + blendB * b.XYZ(), blendA * a.W + blendB * b.W);
            if (!AlmostZero(result.LengthSquared()))
                return Quaternion.Normalize(result);
            else
                return Quaternion.Identity;
        }

        /// <summary>
        /// Returns only the xyz component of quaternion
        /// </summary>
        /// <param name="quaternion"></param>
        private static Vector3 XYZ(this Quaternion quaternion) => new Vector3(quaternion.X, quaternion.Y, quaternion.Z);

        /// <summary>
        /// Sets the xyz component of a quaternion
        /// </summary>
        /// <param name="quaternion"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        private static void XYZ(this Quaternion quaternion, float x, float y, float z)
        {
            quaternion.X = x;
            quaternion.Y = y;
            quaternion.Z = z;
        }

        /// <summary>
        /// Sets the xyz component of a quaternion
        /// </summary>
        /// <param name="quaternion"></param>
        /// <param name="vector"></param>
        private static void XYZ(this Quaternion quaternion, Vector3 vector)
        {
            XYZ(quaternion, vector.X, vector.Y, vector.Z);
        }

        /// <summary>
        /// Rotates a quaternion from towards to
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="maxDegreesDelta"></param>
        public static Quaternion RotateTowards(Quaternion from, Quaternion to, float maxDegreesDelta)
        {
            float num = from.Angle(to);
            if (AlmostZero(num))
            {
                return to;
            }

            float t = System.Math.Min(1f, maxDegreesDelta / num);
            return SlerpUnclamped(ref from, ref to, t);
        }


        /// <summary>
        /// Creates a quaternion that looks at forward vector
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public static Quaternion LookRotation(float x, float y, float z)
        {
            Vector3 forward = new Vector3(x, y, z);
            return LookRotation(forward);
        }

        /// <summary>
        /// Creates a quaternion that looks at forward vector
        /// </summary>
        /// <param name="forward"></param>
        /// <returns></returns>
        public static Quaternion LookRotation(Vector3 forward)
        {
            Vector3 up = Vector3.UnitY;
            return LookRotation(ref forward, ref up);
        }

        /// <summary>
        /// Creates a quaternion that looks at forward vector rotated along up vector
        /// </summary>
        /// <param name="forward"></param>
        /// <param name="up"></param>
        /// <returns></returns>
        private static Quaternion LookRotation(ref Vector3 forward, ref Vector3 up)
        {
            forward = Vector3.Normalize(forward);
            Vector3 right = Vector3.Normalize(Vector3.Cross(up, forward));
            up = Vector3.Cross(forward, right);
            var m00 = right.X;
            var m01 = right.Y;
            var m02 = right.Z;
            var m10 = up.X;
            var m11 = up.Y;
            var m12 = up.Z;
            var m20 = forward.X;
            var m21 = forward.Y;
            var m22 = forward.Z;

            float num8 = (m00 + m11) + m22;
            var quaternion = new Quaternion();
            if (num8 > 0f)
            {
                var num = (float) System.Math.Sqrt(num8 + 1f);
                quaternion.W = num * 0.5f;
                num = 0.5f / num;
                quaternion.X = (m12 - m21) * num;
                quaternion.Y = (m20 - m02) * num;
                quaternion.Z = (m01 - m10) * num;
                return quaternion;
            }

            if ((m00 >= m11) && (m00 >= m22))
            {
                var num7 = (float) System.Math.Sqrt(((1f + m00) - m11) - m22);
                var num4 = 0.5f / num7;
                quaternion.X = 0.5f * num7;
                quaternion.Y = (m01 + m10) * num4;
                quaternion.Z = (m02 + m20) * num4;
                quaternion.W = (m12 - m21) * num4;
                return quaternion;
            }

            if (m11 > m22)
            {
                var num6 = (float) System.Math.Sqrt(((1f + m11) - m00) - m22);
                var num3 = 0.5f / num6;
                quaternion.X = (m10 + m01) * num3;
                quaternion.Y = 0.5f * num6;
                quaternion.Z = (m21 + m12) * num3;
                quaternion.W = (m20 - m02) * num3;
                return quaternion;
            }

            var num5 = (float) System.Math.Sqrt(((1f + m22) - m00) - m11);
            var num2 = 0.5f / num5;
            quaternion.X = (m20 + m02) * num2;
            quaternion.Y = (m21 + m12) * num2;
            quaternion.Z = 0.5f * num5;
            quaternion.W = (m01 - m10) * num2;
            return quaternion;
        }

        private static void Internal_ToAxisAngleRad(Quaternion q, out Vector3 axis, out float angle)
        {
            if (System.Math.Abs(q.W) > 1.0f)
                q = Quaternion.Normalize(q);

            angle = 2.0f * (float) System.Math.Acos(q.W); // angle
            float den = (float) System.Math.Sqrt(1.0 - q.W * q.W);
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
            return System.Math.Abs(a.X - b.X) + System.Math.Abs(a.Y - b.Y) + System.Math.Abs(a.Z - b.Z) +
                   System.Math.Abs(a.W - b.W);
        }

        #endregion

        #region Math and constants

        public static int Clamp(this int i, int min, int max)
        {
            if (i >= min && i <= max)
            {
                return i;
            }

            return i < min ? min : max;
        }

        public static float Clamp(this float i, float min, float max)
        {
            if (i >= min && i <= max)
            {
                return i;
            }

            return i < min ? min : max;
        }

        public static bool AlmostZero(float f)
        {
            return System.Math.Abs(f) < float.Epsilon;
        }

        public static float PI => (float) System.Math.PI;

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