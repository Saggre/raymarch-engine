// Created by Sakri Koskimies (Github: Saggre) on 11/08/2020

using System.Numerics;

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
        public override PrimitiveShape GetShapeType()
        {
            return PrimitiveShape.Plane;
        }

        /// <inheritdoc />
        public override Vector4 GetPrimitiveOptions()
        {
            return Vector4.Zero;
        }
    }
}