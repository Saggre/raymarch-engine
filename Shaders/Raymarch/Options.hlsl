#define MAX_STEPS 512
#define MAX_DIST 100
#define SHADOW_MAX_DIST 50
#define SHADOW_MAX_STEPS 256
#define AO_FALLOFF 40
#define SURF_DIST 1e-4
#define PIXEL_ANGLE 3.2e-4         // Roughly half the angle one pixel covers at this resolution

#define EXPOSURE 1.0
#define FOG_DENSITY 0.006          // Per world unit. At MAX_DIST this is about 45 percent haze.

// ---------------------------------------------------------------------------------------------
// Sun
// ---------------------------------------------------------------------------------------------

#define SUN_AZIMUTH 4.03           // Radians, measured in the xz plane
#define SUN_AZIMUTH_SWEEP 0.85     // Radians either side of that, so the sun never gets behind the scene
#define SUN_SPEED 0.07             // Radians per second
#define SUN_MIN_ELEVATION 26.0     // Degrees. Kept above the horizon so the scene stays lit
#define SUN_MAX_ELEVATION 52.0

#define SUN_COLOR_HIGH float3(1.00, 0.97, 0.92)
#define SUN_COLOR_LOW float3(1.00, 0.72, 0.42)   // What it warms to as it drops
#define SUN_COLOR_FALLOFF 2.2
#define SUN_LIGHT_INTENSITY 1.05

#define SUN_ANGULAR_RADIUS 0.5     // Degrees. The real sun is 0.27, this reads better on screen
#define SUN_DISK_BRIGHTNESS 6.0
#define SUN_GLOW_STRENGTH 0.20
#define SUN_GLOW_FALLOFF 700.0

// ---------------------------------------------------------------------------------------------
// Sky
// ---------------------------------------------------------------------------------------------

// A three stop vertical gradient, the shape Unity's procedural skybox produces. These are linear,
// the gamma curve at the end of the pixel shader is what puts them on screen.
#define SKY_ZENITH_COLOR float3(0.09, 0.21, 0.48)
#define SKY_HORIZON_COLOR float3(0.58, 0.70, 0.84)
#define SKY_GROUND_COLOR float3(0.40, 0.44, 0.50)
#define SKY_GRADIENT_POWER 0.55    // Lower pushes the horizon band further up the sky

#define SKY_SUN_HAZE float3(0.45, 0.32, 0.18)   // Warm scattering spread out around the sun
#define SKY_SUN_HAZE_FALLOFF 6.0
#define SKY_DUSK_TINT float3(0.85, 0.62, 0.45)  // What the whole sky tends towards with a low sun
#define SKY_EXPOSURE 1.0
#define SKY_AMBIENT 0.35           // How much of the zenith sky fills unlit surfaces
#define CHECKER_DARK 0.42          // Colour multiplier on the dark squares of a checkerboard

// ---------------------------------------------------------------------------------------------
// Clouds
// ---------------------------------------------------------------------------------------------

// One flat layer, well past MAX_DIST so only rays that missed the scene ever reach it
#define CLOUD_HEIGHT 90.0
#define CLOUD_MAX_DIST 1400.0      // Where the layer has faded out, before it reaches the horizon
#define CLOUD_MIN_RAY_SLOPE 0.02

#define CLOUD_OCTAVES 4
#define CLOUD_FREQUENCY 0.012     // World units to noise units, so one cell is about 220 units
#define CLOUD_WIND float2(0.008, 0.005)
#define CLOUD_COVERAGE 0.60        // Fraction of the sky the clouds are allowed to take
#define CLOUD_OPACITY 2.2          // How fast density turns into full coverage

#define CLOUD_LIGHT_STEP 0.35      // Noise units towards the sun for the shading sample
#define CLOUD_LIGHT_CONTRAST 3.5
#define CLOUD_LIGHT_FLOOR 0.35     // Keeps shadowed cloud from going flat
#define CLOUD_LIT_TINT float3(1.00, 0.98, 0.95)
#define CLOUD_SHADOW_TINT float3(0.70, 0.74, 0.82)
