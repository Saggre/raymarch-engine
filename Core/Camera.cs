// Created by Sakri Koskimies (Github: Saggre) on 22/10/2019

using System;

namespace RaymarchEngine.Core
{
    /// <summary>
    /// Class that is used to represent a camera
    /// </summary>
    public class Camera : GameObject
    {
        float aspectRatio; // TODO send to shader
        float fieldOfView;

        public Camera()
        {
            aspectRatio = Engine.AspectRatio();
            fieldOfView = (float) (Math.PI / 4.0f);
        }
    }
}