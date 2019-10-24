// Created by Sakri Koskimies (Github: Saggre) on 24/10/2019

namespace EconSim.Terrain
{
    public class TerrainColor
    {
        private float r;
        private float g;
        private float b;
        private float a;
        public TerrainColor(float r, float g, float b, float a)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }

        public TerrainColor(float r, float g, float b)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            a = 1;
        }

        public float R
        {
            get => r;
            set => r = value;
        }

        public float G
        {
            get => g;
            set => g = value;
        }

        public float B
        {
            get => b;
            set => b = value;
        }

        public float A
        {
            get => a;
            set => a = value;
        }
    }
}