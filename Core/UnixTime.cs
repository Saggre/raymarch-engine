// Created by Sakri Koskimies (Github: Saggre) on 02/10/2019

using System;

namespace RaymarchEngine.Core
{
    /// <summary>
    /// Conversions for the unix timestamps that Start and End are handed
    /// </summary>
    public static class UnixTime
    {
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// Convert from timestamp to DateTime
        /// </summary>
        /// <param name="timestamp">Seconds since the unix epoch</param>
        /// <returns>The same moment as a UTC DateTime</returns>
        public static DateTime ToDateTime(int timestamp)
        {
            return Epoch.AddSeconds(timestamp);
        }

        /// <summary>
        /// Convert from DateTime to timestamp
        /// </summary>
        /// <param name="date">Moment to convert</param>
        /// <returns>Seconds since the unix epoch</returns>
        public static int FromDateTime(DateTime date)
        {
            return (int) (date.ToUniversalTime() - Epoch).TotalSeconds;
        }
    }
}
