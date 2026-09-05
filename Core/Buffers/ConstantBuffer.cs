// Created by Sakri Koskimies (Github: Saggre) on 11/08/2020

using System;
using System.Diagnostics;
using SharpDX.Direct3D11;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;

namespace RaymarchEngine.Core.Buffers
{
    /// <summary>
    /// With this class you can effortlessly add a HLSL cbuffer and update its values.
    /// Constant buffer is a buffer and has a 'b' flag.
    /// https://docs.microsoft.com/en-us/windows/win32/direct3dhlsl/dx-graphics-hlsl-constants
    /// </summary>
    /// <typeparam name="T">Type of buffer element</typeparam>
    public class ConstantBuffer<T> : IDisposable where T : struct
    {
        private readonly Device device;
        private readonly DeviceContext deviceContext;
        private Buffer buffer;
        private readonly int elementSize;
        private readonly int slot;

        /// <summary>
        /// Creates the buffer and binds it to the vertex and pixel stages
        /// </summary>
        /// <param name="device">Device the buffer is created on</param>
        /// <param name="slot">Constant buffer register slot, b0 upwards</param>
        /// <exception cref="InvalidOperationException">T is not a multiple of 16 bytes</exception>
        public ConstantBuffer(Device device, int slot = 0)
        {
            this.slot = slot;
            this.device = device;
            deviceContext = device.ImmediateContext;
            elementSize = SharpDX.Utilities.SizeOf<T>();
            Debug.WriteLine(elementSize);
            CreateBuffer();
        }

        private void CreateBuffer()
        {
            // D3D11 requires a multiple of 16. Padding the buffer instead of the struct would make
            // UpdateValue read past the end of its stack local, so fail loudly here.
            if (elementSize % 16 != 0)
            {
                throw new InvalidOperationException(
                    $"Constant buffer struct {typeof(T).Name} is {elementSize} bytes. D3D11 requires " +
                    "a multiple of 16, so pad the struct itself.");
            }

            buffer = new Buffer(device, new BufferDescription
            {
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.ConstantBuffer,
                SizeInBytes = elementSize,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
                StructureByteStride = 0
            });

            deviceContext.VertexShader.SetConstantBuffer(slot, buffer);
            deviceContext.PixelShader.SetConstantBuffer(slot, buffer);
        }

        /// <summary>
        /// Upload a new value. The buffer is a fixed size, so this never reallocates.
        /// </summary>
        /// <param name="value">Value to copy into the buffer</param>
        public void UpdateValue(T value)
        {
            deviceContext.UpdateSubresource(ref value, buffer);
        }

        /// <summary>
        /// Clear resources used by this buffer
        /// </summary>
        public void Dispose()
        {
            buffer?.Dispose();
        }
    }
}