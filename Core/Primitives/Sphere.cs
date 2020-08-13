// Created by Sakri Koskimies (Github: Saggre) on 11/08/2020

using System;
using System.Numerics;
using BepuPhysics.Collidables;
using ColliderShape = BepuPhysics.Collidables.Sphere;

namespace RaymarchEngine.Core.Primitives
{
    /// <summary>
    /// A sphere. What about it?
    /// </summary>
    public class Sphere : Primitive
    {
        private float radius;

        /// <summary>
        /// The radius of this sphere.
        /// </summary>
        public float Radius
        {
            get => radius;
            set => radius = value; // TODO update also collider radius?
        }

        /// <inheritdoc />
        public Sphere() : base()
        {
            radius = 1f;
        }

        /// <inheritdoc />
        public Sphere(Vector3 position, Quaternion rotation, float radius) : base(position, rotation, Vector3.One)
        {
            this.radius = radius;
        }

        /// <inheritdoc />
        public override IConvexShape GetColliderShape()
        {
            return new ColliderShape(radius);
        }

        /// <inheritdoc />
        public override Vector4 GetPrimitiveOptions()
        {
            return new Vector4(radius, 0f, 0f, 0f);
        }
    }
}