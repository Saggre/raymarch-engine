// Created by Sakri Koskimies (Github: Saggre) on 11/08/2020

using System;
using System.Numerics;
using BepuPhysics.Collidables;
using ColliderShape = BepuPhysics.Collidables.Sphere;

namespace RaymarchEngine.Core.Primitives
{
    /// <summary>
    /// An ellipsoid.
    /// </summary>
    public class Ellipsoid : Primitive
    {
        /// <inheritdoc />
        public Ellipsoid()
        {
        }

        /// <inheritdoc />
        public Ellipsoid(Vector3 position, Quaternion rotation, Vector3 radii) : base(position, rotation, radii)
        {
        }

        /// <inheritdoc />
        public override IConvexShape GetColliderShape()
        {
            return new ColliderShape(Math.Max(Math.Max(Scale.X, Scale.Y), Scale.Z)); // TODO collider not exact
        }

        /// <inheritdoc />
        public override Vector4 GetPrimitiveOptions()
        {
            return Vector4.Zero;
        }
    }
}