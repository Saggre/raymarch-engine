// Created by Sakri Koskimies (Github: Saggre) on 09/08/2020

using System;
using System.Diagnostics;
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
        private int elementSize;
        private int objectCount;

        private DataBox dataBox;

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
        }

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
                    Usage = ResourceUsage.Dynamic,
                    BindFlags = BindFlags.ConstantBuffer,
                    SizeInBytes = elementSize * objectCount,
                    CpuAccessFlags = CpuAccessFlags.Write,
                    OptionFlags = ResourceOptionFlags.None
                });

                device.ImmediateContext.PixelShader.SetConstantBuffer(1, buffer);
                device.ImmediateContext.VertexShader.SetConstantBuffer(1, buffer);
            }

            if (buffer != null)
            {
                // TODO do on start?
                dataBox = device.ImmediateContext.MapSubresource(buffer, 0, MapMode.WriteDiscard, MapFlags.None);

                IntPtr dataPointer = dataBox.DataPointer;
                //Utilities.Write(dataPointer, value,0,objectCount);

                for (int i = 0; i < objectCount; i++)
                {
                    dataPointer = Utilities.WriteAndPosition(dataPointer, ref value[i]);
                }

                device.ImmediateContext.UnmapSubresource(buffer, 0);
            }
        }

        public void Dispose()
        {
            buffer?.Dispose();
        }
    }
}