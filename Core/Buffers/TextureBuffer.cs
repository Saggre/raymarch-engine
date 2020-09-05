// Created by Sakri Koskimies (Github: Saggre) on 05/09/2020

using System;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Device = SharpDX.Direct3D11.Device;

namespace RaymarchEngine.Core.Buffers
{
    /// <summary>
    /// With this class you can effortlessly add a texture buffer
    /// </summary>
    public class TextureBuffer<T> : IDisposable
    {
        private readonly Device device;
        private readonly DeviceContext deviceContext;
        private ShaderResourceView shaderResourceView;
        private readonly int textureSize;
        private readonly int slot;
        private readonly Format format;

        /// <summary>
        /// Create new ConstantBuffer
        /// </summary>
        /// <param name="device"></param>
        /// <param name="data"></param>
        /// <param name="textureSize"></param>
        /// <param name="format"></param>
        /// <param name="slot"></param>
        public TextureBuffer(Device device, T[] data, int textureSize, Format format = Format.R8G8B8A8_UNorm,
            int slot = 0)
        {
            this.slot = slot;
            this.device = device;
            deviceContext = device.ImmediateContext;
            this.textureSize = textureSize;
            this.format = format;
            CreateBuffer(data);
        }

        private void CreateBuffer(T[] data)
        {
            Texture2D texture = new Texture2D(device, new Texture2DDescription()
            {
                BindFlags = BindFlags.ShaderResource,
                Format = format,
                Width = textureSize,
                Height = textureSize,
                OptionFlags = ResourceOptionFlags.None,
                MipLevels = 1,
                ArraySize = 1,
                SampleDescription = {Count = 1, Quality = 0}
            }, new DataRectangle(Util.GetDataPtr(data), textureSize * format.SizeOfInBytes()));
            
            //Console.WriteLine(textureSize * format.SizeOfInBytes());
            shaderResourceView = new ShaderResourceView(device, texture);

            deviceContext.VertexShader.SetShaderResource(slot, shaderResourceView);
            deviceContext.PixelShader.SetShaderResource(slot, shaderResourceView);
        }

        /// <summary>
        /// Clear resources used by this buffer
        /// </summary>
        public void Dispose()
        {
            shaderResourceView?.Dispose();
        }
    }
}