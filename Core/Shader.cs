// Created by Sakri Koskimies (Github: Saggre) on 25/10/2019

using System;
using System.Collections.Generic;
using System.IO;
using EconSim.Geometry;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace EconSim.Core
{
    public class PerObjectResources
    {
        // Dict keys are shader slots
        public Dictionary<int, Buffer> ConstantBuffers;
        public Dictionary<int, ShaderResourceView> ShaderResourceViews;

        public PerObjectResources()
        {
            ConstantBuffers = new Dictionary<int, Buffer>();
            ShaderResourceViews = new Dictionary<int, ShaderResourceView>();
        }
    }

    public static class Shader
    {

        #region Helpers

        /// <summary>
        /// Creates a buffer containing a single instance if a struct
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bufferData"></param>
        /// <returns></returns>
        public static Buffer CreateSingleElementBuffer<T>(ref T bufferData) where T : struct
        {
            return Buffer.Create(Engine.RenderDevice.d3dDevice,
                BindFlags.ConstantBuffer,
                ref bufferData,
                Utilities.SizeOf<T>(),
                ResourceUsage.Default,
                CpuAccessFlags.None,
                ResourceOptionFlags.None,
                0);
        }

        #endregion

    }

}