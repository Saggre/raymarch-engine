// Created by Sakri Koskimies (Github: Saggre) on 29/09/2019

using System;
using SharpDX.Direct3D11;
using Resource = SharpDX.DXGI.Resource;
using Texture2D = Microsoft.Xna.Framework.Graphics.Texture2D;
using DXTexture2D = SharpDX.Direct3D11.Texture2D;

namespace EconSim
{

    public class BufferData
    {
        private int shaderBufferIndex;
        private dynamic inputData;
        private dynamic computeResource;
        private dynamic stagingResource;
        private UnorderedAccessView uav;
        private Type inputDataType;

        public BufferData(int shaderBufferIndex, dynamic inputData, Type inputDataType)
        {
            this.shaderBufferIndex = shaderBufferIndex;
            this.inputData = inputData;
            this.inputDataType = inputDataType;
        }

        public int GetShaderBufferIndex()
        {
            return shaderBufferIndex;
        }

        public dynamic GetComputeResource()
        {
            return computeResource;
        }

        public dynamic GetStagingResource()
        {
            return stagingResource;
        }

        public dynamic GetInputData()
        {
            return inputData;
        }

        public UnorderedAccessView GetUnorderedAccessView()
        {
            return uav;
        }

        public void SetUnorderedAccessView(UnorderedAccessView unorderedAccessView)
        {
            uav = unorderedAccessView;
        }

        public Type GetInputDataType()
        {
            return inputDataType;
        }

        public void SetComputeResource(dynamic resource)
        {
            computeResource = resource;
        }

        public void SetStagingResource(dynamic resource)
        {
            stagingResource = resource;
        }
    }
}