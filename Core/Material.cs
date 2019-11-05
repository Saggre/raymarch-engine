// Created by Sakri Koskimies (Github: Saggre) on 05/11/2019


using System.Numerics;
using SharpDX.Direct3D11;

namespace EconSim.Core
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
        /// Ambient Color (RGBA)
        /// </summary>
        public Vector4 AmbientColor { get; set; }

        /// <summary>
        /// Diffuse Color (RGBA)
        /// </summary>
        public Vector4 DiffuseColor { get; set; }

        /// <summary>
        /// Specular Color (RGBA)
        /// </summary>
        public Vector4 SpecularColor { get; set; }

        /// <summary>
        /// Specular Power
        /// </summary>
        public int SpecularPower { get; set; }

        /// <summary>
        /// Emissive Color (RGBA)
        /// </summary>
        public Vector4 EmissiveColor { get; set; }

        /// <summary>
        /// Index Start inside IndexBuffer
        /// </summary>
        public int StartIndex { get; set; }

        /// <summary>
        /// Number of indices to draw
        /// </summary>
        public int IndexCount { get; set; }
    }
}