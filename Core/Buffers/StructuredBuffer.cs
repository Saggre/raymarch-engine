// Created by Sakri Koskimies (Github: Saggre) on 11/08/2020

using System;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;

namespace RaymarchEngine.Core.Buffers
{
    /// <summary>
    /// With this class you can effortlessly add a HLSL StructuredBuffer&lt;T&gt; and update its values.
    /// Structured buffer is a shader resource view and has a 't' flag.
    /// https://docs.microsoft.com/en-us/windows/win32/direct3dhlsl/sm5-object-structuredbuffer
    /// </summary>
    /// <typeparam name="T">Type of buffer array</typeparam>
    public class StructuredBuffer<T> : IDisposable where T : struct
    {
        private readonly Device device;
        private readonly DeviceContext deviceContext;
        private Buffer buffer;
        private ShaderResourceView shaderResource;
        private readonly int elementSize;
        private int objectCount;
        private readonly int slot;

        /// <summary>
        /// Records where the buffer will live. The GPU buffer itself is created on the first
        /// UpdateValue, once the element count is known.
        /// </summary>
        /// <param name="device">Device the buffer is created on</param>
        /// <param name="slot">Shader resource register slot, t0 upwards</param>
        public StructuredBuffer(Device device, int slot = 0)
        {
            this.slot = slot;
            this.device = device;
            deviceContext = device.ImmediateContext;
            elementSize = SharpDX.Utilities.SizeOf<T>();
        }

        /// <summary>
        /// Upload an array. The buffer and its view are recreated whenever the length changes,
        /// and an empty array is ignored because D3D11 rejects a zero sized buffer.
        /// </summary>
        /// <param name="value">Elements to copy into the buffer</param>
        public void UpdateValue(T[] value)
        {
            if (value.Length == 0)
            {
                return;
            }

            // Create new buffer if elements were added or removed
            if (objectCount != value.Length)
            {
                objectCount = value.Length;

                buffer?.Dispose();
                buffer = new Buffer(device, new BufferDescription
                {
                    Usage = ResourceUsage.Default,
                    BindFlags = BindFlags.ShaderResource,
                    SizeInBytes = elementSize * objectCount,
                    CpuAccessFlags = CpuAccessFlags.None,
                    OptionFlags = ResourceOptionFlags.BufferStructured,
                    StructureByteStride = elementSize
                });

                shaderResource?.Dispose();
                shaderResource = new ShaderResourceView(device, buffer, new ShaderResourceViewDescription()
                {
                    Format = Format.Unknown,
                    Dimension = ShaderResourceViewDimension.Buffer,
                    Buffer = new ShaderResourceViewDescription.BufferResource()
                    {
                        ElementWidth = objectCount
                    }
                });

                deviceContext.VertexShader.SetShaderResource(slot, shaderResource);
                deviceContext.PixelShader.SetShaderResource(slot, shaderResource);
            }

            deviceContext.UpdateSubresource(value, buffer);
        }

        /// <summary>
        /// Clear resources used by this buffer
        /// </summary>
        public void Dispose()
        {
            buffer?.Dispose();
            shaderResource?.Dispose();
        }
    }
}