// Created by Sakri Koskimies (Github: Saggre) on 02/10/2019

using System;
using System.Runtime.InteropServices;
using Math = System.Math;

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

        /// <summary>
        /// Convert from timestamp to DateTime
        /// </summary>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        public static DateTime ConvertFromUnixTimestamp(int timestamp)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return origin.AddSeconds(timestamp);
        }

        /// <summary>
        /// Convert from DateTime to timestamp
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static int ConvertToUnixTimestamp(DateTime date)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan diff = date.ToUniversalTime() - origin;
            return (int)diff.TotalSeconds;
        }
    }
}