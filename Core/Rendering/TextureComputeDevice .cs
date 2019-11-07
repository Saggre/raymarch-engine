// Created by Sakri Koskimies (Github: Saggre) on 06/11/2019

using System.IO;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Buffer11 = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;
using MapFlags = SharpDX.Direct3D11.MapFlags;

namespace EconSim.Core.Rendering
{

    /// <summary>
    /// Execute Compute Shader outside Regular Graphics Device
    /// </summary>
    public class TextureComputeDevice : IShader
    {

        /// <summary>
        /// Device Pointer
        /// </summary>
        public Device Device
        {
            get { return _device; }
        }

        /// <summary>
        /// Device Context Pointer
        /// </summary>
        public DeviceContext DeviceContext
        {
            get { return _context; }
        }

        private Device _device;
        private DeviceContext _context;
        private Texture2D _buffer;
        private UnorderedAccessView _accessView;
        private Texture2D _resultBuffer;
        private ComputeShader _shader;

        private Format textureFormat;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="folderPath"></param>
        /// <param name="fileName"></param>
        /// <param name="resolution">Texture resolution</param>
        public TextureComputeDevice(string folderPath, string fileName, int resolution, Format textureFormat)
        {
            ShaderFlags shaderFlags = ShaderFlags.Debug;

            _device = new Device(SharpDX.Direct3D.DriverType.Hardware, DeviceCreationFlags.SingleThreaded);
            _context = _device.ImmediateContext;

            this.textureFormat = textureFormat;

            _accessView = CreateUAV(resolution, out _buffer);
            _resultBuffer = CreateStaging(resolution);

            HLSLFileIncludeHandler includeHandler = new HLSLFileIncludeHandler(folderPath);

            string path = Path.Combine(folderPath, fileName);

            CompilationResult byteCode = ShaderBytecode.CompileFromFile(path, "main", "cs_5_0", shaderFlags,
                EffectFlags.None, null, includeHandler);
            _shader = new ComputeShader(Device, byteCode);
        }

        /// <summary>
        /// Prepare Device to compute shader
        /// </summary>
        public void Begin()
        {
            DeviceContext.ComputeShader.SetUnorderedAccessView(0, _accessView);
            DeviceContext.ComputeShader.Set(_shader);
        }

        /// <summary>
        /// End compute shader processing
        /// </summary>
        public void End()
        {
            DeviceContext.CopyResource(_buffer, _resultBuffer);
            DeviceContext.Flush();
            DeviceContext.ComputeShader.SetUnorderedAccessView(0, null);
            DeviceContext.ComputeShader.Set(null);
        }

        /// <summary>
        /// Execute Compute Shader code
        /// </summary>
        /// <param name="threadGroupCountX">Number of thread Group on X Axis</param>
        /// <param name="threadGroupCountY">Number of thread Group on Y Axis</param>
        /// <param name="threadGroupCountZ">Number of thread Group on Z Axis</param>
        public void Start(int threadGroupCountX, int threadGroupCountY, int threadGroupCountZ)
        {
            DeviceContext.Dispatch(threadGroupCountX, threadGroupCountY, threadGroupCountZ);
        }

        /// <summary>
        /// Create a resource that can be read from
        /// </summary>
        /// <param name="resolution"></param>
        /// <returns></returns>
        private Texture2D CreateStaging(int resolution)
        {
            return new Texture2D(_device, new Texture2DDescription
            {
                CpuAccessFlags = CpuAccessFlags.Read,
                BindFlags = BindFlags.None,
                Format = textureFormat,
                Width = resolution,
                Height = resolution,
                OptionFlags = ResourceOptionFlags.None,
                MipLevels = 1,
                ArraySize = 1,
                SampleDescription = { Count = 1, Quality = 0 },
                Usage = ResourceUsage.Staging
            });
        }

        /// <summary>
        /// Create the resource send to the shader
        /// </summary>
        /// <param name="resolution"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        private UnorderedAccessView CreateUAV(int resolution, out Texture2D buffer)
        {

            buffer = new Texture2D(Device, new Texture2DDescription
            {
                BindFlags = BindFlags.UnorderedAccess | BindFlags.ShaderResource,
                Format = textureFormat,
                Width = resolution,
                Height = resolution,
                OptionFlags = ResourceOptionFlags.None,
                MipLevels = 1,
                ArraySize = 1,
                SampleDescription = { Count = 1, Quality = 0 }
            });

            UnorderedAccessViewDescription uavDescription = new UnorderedAccessViewDescription()
            {
                Format = textureFormat,
                Dimension = UnorderedAccessViewDimension.Texture2D,
                Texture2D = { MipSlice = 0 }
            };

            return new UnorderedAccessView(Device, buffer, uavDescription);

        }

        /// <summary>
        /// Send the buffer to all shader stages
        /// </summary>
        /// <param name="slot"></param>
        /// <param name="constantBuffer"></param>
        public void SendBufferToShader(int slot, Buffer11 constantBuffer)
        {
            DeviceContext.ComputeShader.SetConstantBuffer(slot, constantBuffer);
        }

        /// <summary>
        /// Send the shader resource view to all shader stages
        /// </summary>
        /// <param name="slot"></param>
        /// <param name="resourceView"></param>
        public void SendResourceViewToShader(int slot, ShaderResourceView resourceView)
        {
            DeviceContext.ComputeShader.SetShaderResource(slot, resourceView);
        }

        /// <summary>
        /// Return Executed Data
        /// </summary>
        /// <param name="resolution">Number of element to read</param>
        /// <returns>Result</returns>
        public T[] ReadData<T>(int resolution) where T : struct
        {
            DataStream stream;
            DataBox box = DeviceContext.MapSubresource(_resultBuffer, 0, MapMode.Read, MapFlags.None, out stream);
            T[] result = stream.ReadRange<T>(resolution * resolution);
            DeviceContext.UnmapSubresource(_buffer, 0);
            return result;
        }
    }
}