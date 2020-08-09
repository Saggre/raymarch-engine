using System;
using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.Direct3D11;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace EconSim.Core
{
    class RaymarchObjectsBuffer<T> : IDisposable
        where T : struct
    {
        private readonly Device device;
        private Buffer buffer;
        private readonly DataStream dataStream;
        private int elementSize;

        public Buffer Buffer
        {
            get { return buffer; }
        }

        public RaymarchObjectsBuffer(Device device)
        {
            this.device = device;

            // If no specific marshalling is needed, can use
            // SharpDX.Utilities.SizeOf<T>() for better performance.
            elementSize = Marshal.SizeOf(typeof(T));

            dataStream = new DataStream(elementSize, true, true);
        }

        public void UpdateValue(T[] value)
        {
            if (value.Length == 0)
            {
                return;
            }

            buffer = new Buffer(device, new BufferDescription
            {
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.ConstantBuffer,
                SizeInBytes = elementSize * value.Length,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
                StructureByteStride = 0
            });

            // If no specific marshalling is needed, can use 
            // dataStream.Write(value) for better performance.
            //Marshal.StructureToPtr(value, dataStream.DataPointer, false);

            GCHandle handle = GCHandle.Alloc(value, GCHandleType.Pinned);
            try
            {
                IntPtr pointer = handle.AddrOfPinnedObject();
                var dataBox = new DataBox(pointer);
                device.ImmediateContext.UpdateSubresource(dataBox, buffer, 0);
            }
            finally
            {
                if (handle.IsAllocated)
                {
                    handle.Free();
                }
            }

            device.ImmediateContext.PixelShader.SetConstantBuffer(1, buffer);
            device.ImmediateContext.VertexShader.SetConstantBuffer(1, buffer);
        }

        public void Dispose()
        {
            if (dataStream != null)
                dataStream.Dispose();
            if (buffer != null)
                buffer.Dispose();
        }
    }
}