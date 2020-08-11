// Created by Sakri Koskimies (Github: Saggre) on 11/08/2020

using System.Numerics;

namespace EconSim.Core.Primitives
{
    /// <summary>
    /// An infinite plane.
    /// </summary>
    public class Torus : Primitive
    {
        private Vector2 dimensions;

        /// <summary>
        /// 
        /// </summary>
        public Vector2 Dimensions
        {
            get => dimensions;
            set => dimensions = value;
        }

        /// <inheritdoc />
        public Torus()
        {
            dimensions = Vector2.One;
        }

        /// <inheritdoc />
        public Torus(Vector3 position, Quaternion rotation, Vector2 dimensions) : base(position, rotation, Vector3.One)
        {
            this.dimensions = dimensions;
        }

        /// <inheritdoc />
        public override PrimitiveShape GetShapeType()
        {
            return PrimitiveShape.Torus;
        }

        /// <inheritdoc />
        public override Vector4 GetPrimitiveOptions()
        {
            return new Vector4(dimensions, 0f, 0f);
        }
    }
}