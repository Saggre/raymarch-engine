// Created by Sakri Koskimies (Github: Saggre) on 05/11/2019


using System.Numerics;
using SharpDX.Direct3D11;

namespace RaymarchEngine.Core
{
    public class Material
    {
        /// <summary>
        /// Diffuse map
        /// </summary>
        public ShaderResourceView DiffuseMap { get; set; }

        /// <summary>
        /// Normal Map
        /// </summary>
        public ShaderResourceView NormalMap { get; set; }

        /// <summary>
        /// Diffuse Color (RGBA)
        /// </summary>
        public Vector4 DiffuseColor { get; set; }

        /// <summary>
        /// Number of indices to draw
        /// </summary>
        public int IndexCount { get; set; }
    }
}