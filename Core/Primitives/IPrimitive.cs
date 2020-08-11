// Created by Sakri Koskimies (Github: Saggre) on 11/08/2020

using System.Numerics;
using System.Runtime.InteropServices;

namespace EconSim.Core.Primitives
{
    public interface IPrimitive
    {
        public Vector3 GetPrimitiveOptions();

        public PrimitiveShape GetShapeType();

        // TODO move out of class
        /// <summary>
        /// Get data that can be sent to the raymarch shader to represent this object
        /// </summary>
        /// <returns></returns>
        public RaymarchGameObjectBufferData GetBufferData();
    }

    /// <summary>
    /// The shape that this object is rendered as
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
        Torus
    }

    /// <summary>
    /// Data that is passed to the raymarch shader.
    /// Euler angles is passed instead of quaternion rotation, because the object can't be/shouldn't be rotated in the shader. Euler angle data will suffice for rendering the shape at different rotations.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct RaymarchGameObjectBufferData
    {
        public int raymarchShape;
        public Vector3 primitiveOptions;
        public Vector3 position;
        public Vector3 eulerAngles;
        public Vector3 scale;
        public Vector4 color;
        public Vector3 materialOptions;

        public RaymarchGameObjectBufferData(
            PrimitiveShape raymarchShape,
            Vector3 primitiveOptions,
            Vector3 position,
            Vector3 eulerAngles,
            Vector3 scale
        )
        {
            this.raymarchShape = (int) raymarchShape;
            this.primitiveOptions = primitiveOptions;
            this.position = position;
            this.eulerAngles = eulerAngles;
            this.scale = scale;
            color = Vector4.One;
            materialOptions = Vector3.Zero;
        }
    }
}