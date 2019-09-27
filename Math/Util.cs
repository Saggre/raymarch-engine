using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EconSim.Math
{
    public static class Util
    {

        #region Vectors

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

        #region Math

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