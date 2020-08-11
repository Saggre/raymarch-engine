// Created by Sakri Koskimies (Github: Saggre) on 11/08/2020

using System;
using SharpDX.Direct3D11;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;

namespace EconSim.Core.Buffers
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
        /// Create new ConstantBuffer
        /// </summary>
        /// <param name="device"></param>
        /// <param name="slot"></param>
        public ConstantBuffer(Device device, int slot = 0)
        {
            this.slot = slot;
            this.device = device;
            deviceContext = device.ImmediateContext;
            elementSize = SharpDX.Utilities.SizeOf<T>();
            CreateBuffer();
        }

        private void CreateBuffer()
        {
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
        /// Update the values of this buffer
        /// </summary>
        /// <param name="value"></param>
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