using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;
using MapFlags = SharpDX.Direct3D11.MapFlags;
using Texture2D = SharpDX.Direct3D11.Texture2D;

namespace EconSim
{

    public class ComputeShader<T> where T : struct
    {
        private Device _d3dDevice;
        private DeviceContext _d3dContext;
        private UnorderedAccessView _accessView;
        private ComputeShader _shader;

        private Buffer _buffer;
        private Texture2D _texture;
        public Texture2D stagingTexture;
        private Buffer _resultBuffer;
        private GraphicsDevice _g;

        public ComputeShader(GraphicsDevice MNGDevice, string filename, string functionName, int count)
        {
            if (MNGDevice == null)
            {
                Debug.WriteLine("Error");
                return;
            }

            _g = MNGDevice;
            _d3dDevice = (MNGDevice.Handle as SharpDX.Direct3D11.Device);
            _d3dContext = (MNGDevice.Handle as SharpDX.Direct3D11.Device).ImmediateContext;

            _accessView = CreateUAV(count, out _buffer);
            _resultBuffer = CreateStaging(count);

            var computeShaderByteCode = ShaderBytecode.CompileFromFile(filename, functionName, "cs_5_0");
            _shader = new ComputeShader(_d3dDevice, computeShaderByteCode);
        }

        private UnorderedAccessView CreateUAV(int count, out Buffer buffer)
        {
            int size = SharpDX.Utilities.SizeOf<T>();

            buffer = new Buffer(_d3dDevice, new BufferDescription()
            {
                BindFlags = BindFlags.UnorderedAccess | BindFlags.ShaderResource,
                Usage = ResourceUsage.Default,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.BufferStructured,
                StructureByteStride = size,
                SizeInBytes = size * count
            });

            UnorderedAccessViewDescription uavDescription = new UnorderedAccessViewDescription()
            {
                Buffer = new UnorderedAccessViewDescription.BufferResource() { FirstElement = 0, Flags = UnorderedAccessViewBufferFlags.None, ElementCount = count },
                Format = SharpDX.DXGI.Format.Unknown,
                Dimension = UnorderedAccessViewDimension.Buffer
            };

            //--

            _texture = new Texture2D(_d3dDevice, new Texture2DDescription()
            {
                BindFlags = BindFlags.UnorderedAccess | BindFlags.ShaderResource,
                Format = Format.R8G8B8A8_UNorm,
                Width = 1024,
                Height = 1024,
                OptionFlags = ResourceOptionFlags.None,
                MipLevels = 1,
                ArraySize = 1,
                SampleDescription = { Count = 1, Quality = 0 }
            });

            stagingTexture = new Texture2D(_d3dDevice, new Texture2DDescription
            {
                CpuAccessFlags = CpuAccessFlags.Read,
                BindFlags = BindFlags.None,
                Format = Format.R8G8B8A8_UNorm,
                Width = 1024,
                Height = 1024,
                OptionFlags = ResourceOptionFlags.None,
                MipLevels = 1,
                ArraySize = 1,
                SampleDescription = { Count = 1, Quality = 0 },
                Usage = ResourceUsage.Staging
            });

            return new UnorderedAccessView(_d3dDevice, _texture, new UnorderedAccessViewDescription()
            {
                Format = Format.R8G8B8A8_UNorm,
                Dimension = UnorderedAccessViewDimension.Texture2D,
                Texture2D = { MipSlice = 0 }
            });

        }

        public Microsoft.Xna.Framework.Graphics.Texture2D GetTexture()
        {
            //_d3dDevice.ImmediateContext.ComputeShader.Set(computeShader);
            //device.ImmediateContext.ComputeShader.SetUnorderedAccessView(0, view);

            //device.ImmediateContext.Dispatch(32, 32, 1);
            _d3dContext.CopyResource(_texture, stagingTexture);
            var mapSource = _d3dContext.MapSubresource(stagingTexture, 0, MapMode.Read, MapFlags.None);

            //Debug.WriteLine(Marshal.ReadInt32(IntPtr.Add(mapSource.DataPointer, 0)));

            try
            {
                // Copy pixels from screen capture Texture to GDI bitmap
                Microsoft.Xna.Framework.Graphics.Texture2D t = new Microsoft.Xna.Framework.Graphics.Texture2D(_g, 1024, 1024);


                var sourcePtr = mapSource.DataPointer;

                //set the color
                Color[] cols = new Color[1024 * 1024];

                for (int y = 0; y < 1024 * 1024; y++)
                {
                    cols[y] = Utilities.Read<Color>(sourcePtr);

                    // Copy a single line
                    //Utilities.CopyMemory(destPtr, sourcePtr, 1024 * 4);

                    // Advance pointers
                    sourcePtr = IntPtr.Add(sourcePtr, 4);
                }

                t.SetData(cols);

                return t;

            }
            finally
            {
                _d3dContext.UnmapSubresource(stagingTexture, 0);
            }
        }

        private Buffer CreateStaging(int count)
        {
            int size = SharpDX.Utilities.SizeOf<T>() * count;
            BufferDescription bufferDescription = new BufferDescription()
            {
                SizeInBytes = size,
                BindFlags = BindFlags.None,
                CpuAccessFlags = CpuAccessFlags.Read | CpuAccessFlags.Write,
                Usage = ResourceUsage.Staging,
                OptionFlags = ResourceOptionFlags.None,
            };

            return new Buffer(_d3dDevice, bufferDescription);
        }

        public void Begin()
        {
            _d3dContext.ComputeShader.SetUnorderedAccessView(0, _accessView);
            _d3dContext.ComputeShader.Set(_shader);
        }
        public void Start(int threadGroupCountX, int threadGroupCountY, int threadGroupCountZ)
        {
            _d3dContext.Dispatch(threadGroupCountX, threadGroupCountY, threadGroupCountZ);
        }

        public void End()
        {
            _d3dContext.CopyResource(_buffer, _resultBuffer);
            _d3dContext.Flush();
            _d3dContext.ComputeShader.SetUnorderedAccessView(0, null);
            _d3dContext.ComputeShader.Set(null);
        }

        public T[] ReadData(int count)
        {
            SharpDX.DataStream stream;
            SharpDX.DataBox box = _d3dContext.MapSubresource(_resultBuffer, 0, MapMode.Read, MapFlags.None, out stream);
            T[] result = stream.ReadRange<T>(count);
            _d3dContext.UnmapSubresource(_buffer, 0);
            return result;

        }
    }
}