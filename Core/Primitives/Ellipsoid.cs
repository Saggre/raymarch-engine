// Created by Sakri Koskimies (Github: Saggre) on 11/08/2020

using System.Numerics;

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
        public override PrimitiveShape GetShapeType()
        {
            return PrimitiveShape.Ellipsoid;
        }

        /// <inheritdoc />
        public override Vector4 GetPrimitiveOptions()
        {
            return Vector4.Zero;
        }
    }
}