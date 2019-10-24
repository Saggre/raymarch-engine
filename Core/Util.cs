// Created by Sakri Koskimies (Github: Saggre) on 02/10/2019

using System;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using EconSim.Math;

namespace EconSim.Core
{
  public static class Util
  {
    /// <summary>
    /// Returns IntPtr to data
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static IntPtr GetDataPtr(object data)
    {
      GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
      IntPtr ptr = handle.AddrOfPinnedObject();
      handle.Free();
      return ptr;
    }
  }
}