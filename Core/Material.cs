// Created by Sakri Koskimies (Github: Saggre) on 05/11/2019


using System.Numerics;
using SharpDX.Direct3D11;

namespace RaymarchEngine.Core
{
    /// <summary>
    /// One drawable range of a Mesh, with the textures and colour it is drawn with
    /// </summary>
    public class Material
    {
        /// <summary>
        /// Diffuse texture, null when the subset is untextured
        /// </summary>
        public ShaderResourceView DiffuseMap { get; set; }

        /// <summary>
        /// Normal map, null when the subset has none
        /// </summary>
        public ShaderResourceView NormalMap { get; set; }

        /// <summary>
        /// Flat diffuse colour, RGBA
        /// </summary>
        public Vector4 DiffuseColor { get; set; }

        /// <summary>
        /// Number of indices this subset draws
        /// </summary>
        public int IndexCount { get; set; }
    }
}