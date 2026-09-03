#define MAX_STEPS 512
#define MAX_DIST 100
#define SHADOW_MAX_DIST 50
#define SHADOW_MAX_STEPS 256
#define AO_FALLOFF 40
#define SURF_DIST 1e-4

// ---------------------------------------------------------------------------------------------
// Sun
// ---------------------------------------------------------------------------------------------

#define SUN_AZIMUTH 4.03           // Radians, measured in the xz plane
#define SUN_AZIMUTH_SWEEP 0.85     // Radians either side of that, so the sun never gets behind the scene
#define SUN_SPEED 0.07             // Radians per second
#define SUN_MIN_ELEVATION 26.0     // Degrees. Kept above the horizon so the scene stays lit
#define SUN_MAX_ELEVATION 52.0

#define SUN_INTENSITY 22.0         // Radiance the sky and clouds are lit with
#define SUN_LIGHT_INTENSITY 1.15   // Separate gain for surface shading, so the scene keeps its exposure
#define SUN_ANGULAR_RADIUS 0.45    // Degrees. The real sun is 0.27, this reads better at this resolution
#define SUN_DISK_BRIGHTNESS 260.0
#define SUN_GLOW_STRENGTH 0.12
#define SUN_GLOW_FALLOFF 900.0

// ---------------------------------------------------------------------------------------------
// Atmosphere
// ---------------------------------------------------------------------------------------------

// Scattering coefficients at sea level in inverse metres, for 680, 550 and 440 nm. Rayleigh
// scattering goes as one over wavelength to the fourth, which is the whole reason the sky is blue
// and the sun turns red through a long air path.
#define RAYLEIGH_BETA float3(5.802e-6, 13.558e-6, 33.100e-6)
#define MIE_BETA float3(4.4e-6, 4.4e-6, 4.4e-6)

// Heights the densities fall off over, in metres. Multiplying a coefficient by these gives the
// optical depth of the whole column straight up.
#define RAYLEIGH_SCALE_HEIGHT 8000.0
#define MIE_SCALE_HEIGHT 1200.0

#define MIE_G 0.76                 // Aerosols scatter strongly forward, which is the haze near the sun

#define SKY_EXPOSURE 1.0
#define SKY_MULTISCATTER float3(0.10, 0.13, 0.17)   // Stands in for the scattering orders this model drops
#define SKY_INSCATTER_SELF_SHADOW 0.03  // Share of the view depth charged against the light reaching the ray
#define SKY_AMBIENT 0.60           // How much of the zenith sky lights the scene
#define GROUND_HAZE_COLOR float3(0.16, 0.15, 0.14)

// ---------------------------------------------------------------------------------------------
// Clouds
// ---------------------------------------------------------------------------------------------

// The slab sits well beyond MAX_DIST, so it is only ever seen by rays that missed the scene
#define CLOUD_BOTTOM 60.0
#define CLOUD_TOP 230.0
#define CLOUD_MAX_DIST 4000.0      // Past this the shell is thinner than a step and is at the horizon
#define CLOUD_PLANET_RADIUS 20000.0 // Curvature of the layer, so it drops away instead of ending

#define CLOUD_NOISE_SIZE 256.0     // Must match the texture CreateCloudNoise builds
#define CLOUD_MIN_RAY_SLOPE 0.012   // Below this a ray crosses more slab than a march can resolve

// Shape: the two cheap octaves that decide where clouds are at all
#define CLOUD_FREQUENCY 0.010     // World units to noise units, so one cell is about 180 units
#define CLOUD_SHAPE_OCTAVES 2
#define CLOUD_COVERAGE 0.55        // Fraction of the sky the clouds are allowed to take
#define CLOUD_WIND float3(0.010, 0.0, 0.006)

// Detail: higher frequency octaves that erode the shape's edges into something cloud like
#define CLOUD_DETAIL_FREQUENCY 0.055
#define CLOUD_DETAIL_OCTAVES 4.0
#define CLOUD_DETAIL_DRIFT 2.5     // Detail is blown along faster than the shape, so cloud tops boil
#define CLOUD_EROSION 0.60         // How much of the shape the detail is allowed to take away
#define CLOUD_DENSITY 1.0

#define CLOUD_STEPS 96
#define CLOUD_STEP_GROWTH 0.0007   // Steps lengthen with distance, where they cover fewer pixels
#define CLOUD_LOD_FALLOFF 0.0008   // Detail octaves dropped per unit of distance
#define CLOUD_MIN_DENSITY 0.006    // Below this a step is not worth lighting
#define CLOUD_MIN_TRANSMITTANCE 0.02

#define CLOUD_EXTINCTION 0.55
#define CLOUD_SUN_GAIN 0.16
#define CLOUD_AMBIENT 0.30
#define CLOUD_LIGHT_STEP 26.0      // World units towards the sun for the directional derivative
#define CLOUD_LIGHT_CONTRAST 0.35  // Shape difference that counts as fully lit
#define CLOUD_LIGHT_ABSORPTION 1.1
#define CLOUD_LIGHT_FLOOR 0.15     // Keeps interiors from going flat black
#define CLOUD_POWDER 0.7

#define CLOUD_PHASE_FORWARD 0.7
#define CLOUD_PHASE_BACK -0.3
#define CLOUD_PHASE_BLEND 0.6      // Weight of the forward lobe
