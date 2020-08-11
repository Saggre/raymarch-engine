// Created by Sakri Koskimies (Github: Saggre) on 06/11/2019

using System;

namespace RaymarchEngine.EMath
{
    public struct Vector2Int : IEquatable<Vector2Int>
    {
        private int x;
        private int y;

        public Vector2Int(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        /// <summary>
        /// Rounds to nearest int
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public Vector2Int(float x, float y)
        {
            this.x = (int)Math.Round(x);
            this.y = (int)Math.Round(y);
        }

        public int X
        {
            get => x;
            set => x = value;
        }

        public int Y
        {
            get => y;
            set => y = value;
        }

        public bool Equals(Vector2Int other)
        {
            return X == other.X && Y == other.Y;
        }

        public static bool operator ==(Vector2Int a, Vector2Int b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Vector2Int a, Vector2Int b)
        {
            return !a.Equals(b);
        }
    }
}