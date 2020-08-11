// Created by Sakri Koskimies (Github: Saggre) on 11/08/2020

using System.Numerics;

namespace RaymarchEngine.Core.Primitives
{
    /// <summary>
    /// A box.
    /// </summary>
    public class Box : Primitive
    {
        /// <inheritdoc />
        public Box()
        {
        }

        /// <inheritdoc />
        public Box(Vector3 position) : base(position, Quaternion.Identity, Vector3.One)
        {
        }

        /// <inheritdoc />
        public override PrimitiveShape GetShapeType()
        {
            return PrimitiveShape.Box;
        }

        /// <inheritdoc />
        public override Vector4 GetPrimitiveOptions()
        {
            return Vector4.Zero;
        }
    }
}