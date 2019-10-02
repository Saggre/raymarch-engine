// Created by Sakri Koskimies (Github: Saggre) on 02/10/2019

using Microsoft.Xna.Framework;

namespace EconSim.Core
{
    public static class Util
    {
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
    }
}