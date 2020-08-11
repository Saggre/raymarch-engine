// Created by Sakri Koskimies (Github: Saggre) on 11/08/2020

using System.Numerics;

namespace EconSim.Core.Primitives
{
    public class Sphere : GameObject, IPrimitive
    {
        private float radius;

        public float Radius
        {
            get => radius;
            set => radius = value;
        }

        public Sphere()
        {
            this.radius = 1f;
        }

        public Sphere(Vector3 position, Quaternion rotation, float radius) : base(position, rotation, Vector3.One)
        {
            this.radius = radius;
        }

        public PrimitiveShape GetShapeType()
        {
            return PrimitiveShape.Sphere;
        }

        public Vector3 GetPrimitiveOptions()
        {
            return new Vector3(radius, 0f, 0f);
        }

        public RaymarchGameObjectBufferData GetBufferData()
        {
            return new RaymarchGameObjectBufferData(
                GetShapeType(),
                GetPrimitiveOptions(),
                Position,
                new Vector3(0.5f, 1, 0.1f),
                Scale);
        }
    }
}