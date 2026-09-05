// Created by Sakri Koskimies (Github: Saggre) on 22/10/2019

using System;

namespace RaymarchEngine.Core
{
    /// <summary>
    /// The point the scene is raymarched from. Its position and forward vector come from the
    /// inherited Movement component.
    /// </summary>
    public class Camera : GameObject
    {
        /// <summary>
        /// Vertical field of view in radians. TODO send to shader
        /// </summary>
        public float FieldOfView { get; set; } = (float) (Math.PI / 4.0);

        /// <summary>
        /// The render window's current aspect ratio. Not cached: the window is unsized at construction.
        /// </summary>
        public float AspectRatio => Engine.AspectRatio();
    }
}