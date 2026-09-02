// Created by Sakri Koskimies (Github: Saggre) on 11/08/2020

using System;
using System.Numerics;
using BepuPhysics.Collidables;

namespace RaymarchEngine.Core.Primitives
{
    /// <summary>
    /// Interface for raymarched primitive shapes.
    /// </summary>
    public interface IPrimitive
    {
        // Planned members, kept as plain comments because XML docs on a commented-out
        // declaration are not attached to anything (CS1587):
        //
        // Vector4 GetPrimitiveOptions();
        //   Options used by the shader's signed distance field functions, like a sphere's radius.
        //
        // PrimitiveBufferData GetBufferData();
        //   Data sent to the raymarch shader to represent this object.
    }
}