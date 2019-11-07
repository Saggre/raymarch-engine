// Created by Sakri Koskimies (Github: Saggre) on 06/11/2019

using SharpDX.Direct3D11;

namespace EconSim.Core
{
    public interface IShader
    {
        /// <summary>
        /// Send the shader resource view to all shader stages
        /// </summary>
        /// <param name="slot"></param>
        /// <param name="resourceView"></param>
        void SendResourceViewToShader(int slot, ShaderResourceView resourceView);

        /// <summary>
        /// Send the buffer to all shader stages
        /// </summary>
        /// <param name="slot"></param>
        /// <param name="constantBuffer"></param>
        void SendBufferToShader(int slot, Buffer constantBuffer);
    }
}