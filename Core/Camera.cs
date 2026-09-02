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
        /// The render window's current aspect ratio. Read on demand rather than cached at
        /// construction, when the window has not been sized yet.
        /// </summary>
        public float AspectRatio => Engine.AspectRatio();
    }
}