// Created by Sakri Koskimies (Github: Saggre) on 11/08/2020

using System;
using System.Numerics;
using System.Runtime.InteropServices;
using BepuPhysics.Collidables;
using RaymarchEngine.EMath;

namespace RaymarchEngine.Core.Primitives
{
    /// <inheritdoc cref="RaymarchEngine.Core.Primitives.IPrimitive" />
    public abstract class Primitive : GameObject, IPrimitive
    {
        /// <inheritdoc />
        protected Primitive()
        {
        }

        /// <inheritdoc />
        protected Primitive(Vector3 position, Quaternion rotation, Vector3 scale) : base(position, rotation, scale)
        {
        }

        /// <inheritdoc />
        public abstract Vector4 GetPrimitiveOptions();

        /// <inheritdoc />
        public abstract PrimitiveShape GetShapeType();

        /// <inheritdoc />
        public abstract IConvexShape GetColliderShape();

        /// <inheritdoc />
        public RaymarchGameObjectBufferData GetBufferData()
        {
            return new RaymarchGameObjectBufferData(
                GetPrimitiveOptions(),
                Position,
                Rotation.QuaternionToEuler(),
                Scale);
        }

        /// <summary>
        /// Adds this primitive to a scene.
        /// </summary>
        /// <param name="scene"></param>
        /// <returns>Returns this object for chaining</returns>
        public void AddToScene(Scene scene)
        {
            scene.AddGameObject(this);
        }
    }

    /// <summary>
    /// Data that is passed to the raymarch shader.
    /// Euler angles is passed instead of quaternion rotation, because the object can't be/shouldn't be rotated in the shader. Euler angle data will suffice for rendering the shape at different rotations.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct RaymarchGameObjectBufferData
    {
        //public int raymarchShape;
        public Vector4 primitiveOptions;
        public Vector3 position;
        public Vector3 eulerAngles;
        public Vector3 scale;
        public Vector4 color;
        public Vector2 materialOptions;

        public RaymarchGameObjectBufferData(
            Vector4 primitiveOptions,
            Vector3 position,
            Vector3 eulerAngles,
            Vector3 scale
        )
        {
            //this.raymarchShape = (int) raymarchShape;
            this.primitiveOptions = primitiveOptions;
            this.position = position;
            this.eulerAngles = eulerAngles;
            this.scale = scale;
            color = Vector4.One;
            materialOptions = Vector2.Zero;
        }
    }

    /// <summary>
    /// The shape that this object is rendered as.
    /// </summary>
    public enum PrimitiveShape
    {
        /// <summary>
        /// A sphere
        /// </summary>
        Sphere,

        /// <summary>
        /// A box
        /// </summary>
        Box,

        /// <summary>
        /// An infinite plane
        /// </summary>
        Plane,

        /// <summary>
        /// An ellipsoid
        /// </summary>
        Ellipsoid,

        /// <summary>
        /// A torus ;)
        /// </summary>
        Torus,

        /// <summary>
        /// A torus with open ends
        /// </summary>
        CappedTorus
    }
}