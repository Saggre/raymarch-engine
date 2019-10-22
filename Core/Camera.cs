// Created by Sakri Koskimies (Github: Saggre) on 22/10/2019

using System.Numerics;

namespace EconSim.Core
{
    public class Camera : GameObject
    {
        /// <summary>
        /// Returns the camera view matrix
        /// </summary>
        /// <returns></returns>
        public Matrix4x4 ViewMatrix()
        {
            return ModelMatrix();
        }
    }
}