using System;

namespace RaymarchEngine.Core
{
    /// <summary>
    /// The scale the movement is expressed in.
    ///
    /// The movement constants are whole numbers on a scale where sixteen units make a foot: a top
    /// speed of 320, a gravity of 800, a jump of 268. Written in world units those become
    /// unrecognisable decimals that cannot be checked against anything, so the controller keeps
    /// the round numbers and converts here.
    ///
    /// The conversion is fixed by the player rather than by a real world length. The eye sits at
    /// 64 of these units, so 40 of them to the world unit makes this engine's 1.6 unit eye height
    /// exactly that, and everything else falls out in proportion.
    /// </summary>
    public static class MovementUnits
    {
        /// <summary>
        /// How many movement units make up one world unit
        /// </summary>
        public const float PerWorldUnit = 40f;

        /// <summary>
        /// Converts a length or a speed from movement units into world units
        /// </summary>
        /// <param name="movementUnits">A distance in movement units, or a speed in them per second</param>
        /// <returns>The same quantity in world units</returns>
        public static float ToWorld(float movementUnits)
        {
            return movementUnits / PerWorldUnit;
        }

        /// <summary>
        /// Converts a length or a speed from world units into movement units
        /// </summary>
        /// <param name="worldUnits">A distance in world units, or a speed in them per second</param>
        /// <returns>The same quantity in movement units</returns>
        public static float FromWorld(float worldUnits)
        {
            return worldUnits * PerWorldUnit;
        }
    }
}
