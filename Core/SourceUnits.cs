using System;

namespace RaymarchEngine.Core
{
    /// <summary>
    /// The scale the movement is expressed in.
    ///
    /// Source measures in units where sixteen make a foot, and its movement constants are quoted
    /// in them: sv_maxspeed 320, sv_gravity 800, a jump of 268. Written in world units those
    /// become unrecognisable decimals that cannot be checked against anything, so the controller
    /// keeps them as Source wrote them and converts here.
    ///
    /// The conversion is fixed by the player rather than by a real world length. Source puts the
    /// eye at 64 units, so 40 Source units to the world unit makes this engine's 1.6 unit eye
    /// height exactly that, and everything else falls out in proportion: a player moving at
    /// sv_maxspeed covers the same distance relative to their own height as one in Source does.
    /// </summary>
    public static class SourceUnits
    {
        /// <summary>
        /// How many Source units make up one world unit
        /// </summary>
        public const float PerWorldUnit = 40f;

        /// <summary>
        /// Converts a length or a speed from Source units into world units
        /// </summary>
        /// <param name="sourceUnits">A distance in Source units, or a speed in them per second</param>
        /// <returns>The same quantity in world units</returns>
        public static float ToWorld(float sourceUnits)
        {
            return sourceUnits / PerWorldUnit;
        }

        /// <summary>
        /// Converts a length or a speed from world units into Source units
        /// </summary>
        /// <param name="worldUnits">A distance in world units, or a speed in them per second</param>
        /// <returns>The same quantity in Source units</returns>
        public static float FromWorld(float worldUnits)
        {
            return worldUnits * PerWorldUnit;
        }
    }
}
