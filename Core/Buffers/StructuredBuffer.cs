using System;
using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;

namespace EconSim.Core.Buffers
{
    /// <summary>
    /// With this class you can effortlessly add a HLSL StructuredBuffer<T> and update its values.
    /// Structured buffer is a shader resource view and has a 't' flag.
    /// https://docs.microsoft.com/en-us/windows/win32/direct3dhlsl/sm5-object-structuredbuffer
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class StructuredBuffer<T> : IDisposable where T : struct
    {
        private readonly Device device;
        private Buffer buffer;
        private ShaderResourceView shaderResource;
        private int elementSize;
        private int objectCount;

        private DataBox dataBox;

        public StructuredBuffer(Device device)
        {
            this.device = device;

            // If no specific marshalling is needed, can use
            // SharpDX.Utilities.SizeOf<T>() for better performance.
            elementSize = Marshal.SizeOf(typeof(T));
        }

        private DataStream d;

        public void UpdateValue(T[] value)
        {
            if (value.Length == 0)
            {
                return;
            }

            if (objectCount != value.Length)
            {
                objectCount = value.Length;

                buffer?.Dispose();

                buffer = new Buffer(device, new BufferDescription
                {
                    Usage = ResourceUsage.Default,
                    BindFlags = BindFlags.ShaderResource | BindFlags.UnorderedAccess,
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
                        ElementWidth = objectCount,
                    }
                });

                device.ImmediateContext.VertexShader.SetShaderResource(0, shaderResource);
                device.ImmediateContext.PixelShader.SetShaderResource(0, shaderResource);
            }

            if (buffer != null)
            {
                device.ImmediateContext.UpdateSubresource(value, buffer);
            }
        }

        public void Dispose()
        {
            buffer?.Dispose();
            shaderResource?.Dispose();
        }
    }
}