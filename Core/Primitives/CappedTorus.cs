// Created by Sakri Koskimies (Github: Saggre) on 11/08/2020

using System.Numerics;
using BepuPhysics.Collidables;
using ColliderShape = BepuPhysics.Collidables.Cylinder;

namespace RaymarchEngine.Core.Primitives
{
    /// <summary>
    /// A torus with open ends.
    /// </summary>
    public class CappedTorus : Primitive
    {
        private Vector4 attributes;

        /// <summary>
        /// 
        /// </summary>
        public Vector4 Attributes
        {
            get => attributes;
            set => attributes = value;
        }

        /// <inheritdoc />
        public CappedTorus()
        {
            attributes = Vector4.One;
        }

        /// <inheritdoc />
        public CappedTorus(Vector3 position, Quaternion rotation, Vector3 scale, Vector4 attributes) : base(position,
            rotation, scale)
        {
            this.attributes = attributes;
        }

        /// <inheritdoc />
        public override IConvexShape GetColliderShape()
        {
            return new ColliderShape(attributes.X, attributes.Y); // TODO these values are probably wrong
        }

        /// <inheritdoc />
        public override Vector4 GetPrimitiveOptions()
        {
            return attributes;
        }
    }
}