// Created by Sakri Koskimies (Github: Saggre) on 02/10/2019

using System.Runtime.InteropServices;

namespace RaymarchEngine.Core
{
    /// <summary>
    /// Handing managed arrays to the graphics API, which wants a plain address
    /// </summary>
    public static class Interop
    {
        /// <summary>
        /// Pins an object so the garbage collector cannot move it, and hands back the handle.
        ///
        /// The caller has to free the handle, and has to keep it alive for as long as the address
        /// is in use. This used to be a GetDataPtr that pinned, read the address, freed the handle
        /// and returned the address, which is a pointer the collector is free to invalidate before
        /// anything reads through it.
        /// </summary>
        /// <param name="data">Object to pin, normally an array</param>
        /// <returns>The pinned handle, which the caller frees</returns>
        public static GCHandle Pin(object data)
        {
            return GCHandle.Alloc(data, GCHandleType.Pinned);
        }
    }
}
