// Created by Sakri Koskimies (Github: Saggre) on 09/08/2020

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using EconSim.EMath;

namespace EconSim.Core
{
    /// <summary>
    /// The shape that this object is rendered as
    /// </summary>
    public enum RaymarchShape
    {
        sphere,
        box,
        plane,
        ellipsoid,
        torus
    }

    /// <summary>
    /// A class that represents a raymarched object in the scene
    /// </summary>
    public class RaymarchGameObject : Object
    {
        /// <summary>
        /// Data that is passed to the raymarch shader.
        /// Euler angles is passed instead of quaternion rotation, because the object can't be/shouldn't be rotated in the shader. Euler angle data will suffice for rendering the shape at different rotations.
        /// </summary>
        public struct RaymarchGameObjectBufferData
        {
            public int raymarchShape;
            public Vector3 position;
            public Vector3 eulerAngles;
            public Vector3 scale;

            public Vector2 blank;
            // TODO color

            public RaymarchGameObjectBufferData(RaymarchShape raymarchShape, Vector3 position, Vector3 eulerAngles,
                Vector3 scale)
            {
                this.raymarchShape = (int) raymarchShape;
                this.position = position;
                this.eulerAngles = eulerAngles;
                this.scale = scale;
                blank = new Vector2();
            }
        }

        private RaymarchShape shape;

        RaymarchShape Shape => shape;

        public RaymarchGameObject(RaymarchShape shape)
        {
            this.shape = shape;
        }

        /// <summary>
        /// Get data that can be sent to the raymarch shader to represent this object
        /// </summary>
        /// <returns></returns>
        public RaymarchGameObjectBufferData GetBufferData()
        {
            return new RaymarchGameObjectBufferData(shape, Position, Rotation.QuaternionToEuler(), Scale);
        }
    }
}