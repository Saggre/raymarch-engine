// Created by Sakri Koskimies (Github: Saggre) on 29/09/2019

using System;

namespace EconSim.EMath
{

  public class ComputeBuffer
  {
    private int count;
    private int stride;

    private Array data;

    public ComputeBuffer(int count, int stride)
    {
      this.count = count;
      this.stride = stride;
    }

    public void SetData(in Array inputData)
    {
      data = inputData;
    }

    public Array GetData()
    {
      return data;
    }

    public Type GetDataType()
    {
      return data.GetType();
    }

    public int Count => count;

    public int Stride => stride;
  }
}