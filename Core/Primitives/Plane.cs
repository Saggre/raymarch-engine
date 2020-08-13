// Created by Sakri Koskimies (Github: Saggre) on 11/08/2020

using System.Numerics;
using BepuPhysics.Collidables;
using ColliderShape = BepuPhysics.Collidables.Box;

namespace RaymarchEngine.Core.Primitives
{
    /// <summary>
    /// An infinite plane.
    /// </summary>
    public class Plane : Primitive
    {
        /// <inheritdoc />
        public Plane()
        {
        }

        /// <inheritdoc />
        public Plane(Vector3 position) : base(position, Quaternion.Identity, Vector3.One)
        {
        }

        /// <inheritdoc />
        public override IConvexShape GetColliderShape()
        {
            return new ColliderShape(1000, 0.1f, 1000); // TODO there is no plane...
        }

        /// <inheritdoc />
        public override Vector4 GetPrimitiveOptions()
        {
            return Vector4.Zero;
        }
    }
}