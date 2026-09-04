using System;
using System.Numerics;
using RaymarchEngine.EMath;

namespace RaymarchEngine.Core
{
    /// <summary>
    /// The scene's directional light.
    ///
    /// This lives here rather than in the shader because its direction and colour are the same for
    /// every pixel in a frame. Computing them per pixel meant six transcendentals and a pow to
    /// arrive at one value, and several times over, since every path that needed the sun
    /// recomputed it. They are uploaded in the constant buffer instead.
    /// </summary>
    public static class Sun
    {
        /// <summary>
        /// Radians in the xz plane the sweep is centred on
        /// </summary>
        private const float Azimuth = 4.03f;

        /// <summary>
        /// Radians either side of that.
        ///
        /// It swings rather than circling. A full revolution puts the sun behind the scene for
        /// half of it, which leaves every camera facing surface backlit and unreadable.
        /// </summary>
        private const float AzimuthSweep = 0.85f;

        private const float Speed = 0.07f;

        /// <summary>
        /// Degrees. Kept well above the horizon so the scene is never left unlit.
        /// </summary>
        private const float MinElevation = 26f;

        private const float MaxElevation = 52f;

        /// <summary>
        /// The elevation period is deliberately not a whole multiple of the azimuth one, so the
        /// lighting does not visibly repeat
        /// </summary>
        private const float ElevationSpeedRatio = 0.37f;

        private const float LightIntensity = 1.05f;

        private static readonly Vector3 ColorHigh = new Vector3(1.00f, 0.97f, 0.92f);

        /// <summary>
        /// What the light warms to as the sun drops. A real atmosphere gets this from wavelength
        /// dependent extinction, one lerp gets close enough to sell it.
        /// </summary>
        private static readonly Vector3 ColorLow = new Vector3(1.00f, 0.72f, 0.42f);

        private const float ColorFalloff = 2.2f;

        /// <summary>
        /// Direction from the scene towards the sun
        /// </summary>
        /// <param name="elapsedTime">Seconds since the engine started</param>
        /// <returns>A unit vector pointing at the sun</returns>
        public static Vector3 GetDirection(float elapsedTime)
        {
            float azimuth = Azimuth + AzimuthSweep * (float) Math.Sin(elapsedTime * Speed);

            float elevationDegrees = MinElevation + (MaxElevation - MinElevation) *
                (0.5f + 0.5f * (float) Math.Sin(elapsedTime * Speed * ElevationSpeedRatio));

            float elevation = elevationDegrees * EMath.Util.Deg2Rad;
            float cosElevation = (float) Math.Cos(elevation);

            return new Vector3(
                cosElevation * (float) Math.Cos(azimuth),
                (float) Math.Sin(elevation),
                cosElevation * (float) Math.Sin(azimuth));
        }

        /// <summary>
        /// Colour the sun lights surfaces with, warming as it drops
        /// </summary>
        /// <param name="direction">Direction towards the sun</param>
        /// <returns>Linear light colour</returns>
        public static Vector3 GetLightColor(Vector3 direction)
        {
            return Vector3.Lerp(ColorLow, ColorHigh, (direction.Y * ColorFalloff).Clamp(0f, 1f)) * LightIntensity;
        }
    }
}
