// Created by Sakri Koskimies (Github: Saggre) on 02/10/2019

using System;
using System.Runtime.InteropServices;

namespace RaymarchEngine.Core
{
    /// <summary>
    /// Helpers for unmanaged memory and unix timestamps
    /// </summary>
    public static class Util
    {
        /// <summary>
        /// Returns a pointer to data.
        /// The handle is freed before returning, so the pointer is only safe to use while the
        /// caller keeps another reference to the object alive.
        /// </summary>
        /// <param name="data">Object to pin</param>
        /// <returns>Address of the pinned object</returns>
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
        /// <param name="timestamp">Seconds since the unix epoch</param>
        /// <returns>The same moment as a UTC DateTime</returns>
        public static DateTime ConvertFromUnixTimestamp(int timestamp)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return origin.AddSeconds(timestamp);
        }

        /// <summary>
        /// Convert from DateTime to timestamp
        /// </summary>
        /// <param name="date">Moment to convert</param>
        /// <returns>Seconds since the unix epoch</returns>
        public static int ConvertToUnixTimestamp(DateTime date)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan diff = date.ToUniversalTime() - origin;
            return (int)diff.TotalSeconds;
        }
    }
}