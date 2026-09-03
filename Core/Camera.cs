// Created by Sakri Koskimies (Github: Saggre) on 22/10/2019

using System;

namespace RaymarchEngine.Core
{
    /// <summary>
    /// Class that is used to represent a camera
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