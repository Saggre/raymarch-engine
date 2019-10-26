// Created by Sakri Koskimies (Github: Saggre) on 30/09/2019

using System.Numerics;
using SharpDX;

namespace EconSim.Geometry
{

    /// <summary>
    /// A rectangle, where everything is represented by integers
    /// </summary>
    public class SquareRect
    {
        private int x;
        private int y;
        private int size;

        public SquareRect(int x, int y, int size)
        {
            this.x = x;
            this.y = y;
            this.size = size;
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

        public int Size
        {
            get => size;
            set => size = value;
        }
    }
}