// Sky, sun and volumetric clouds.
//
// The sky is analytic single scattering: Rayleigh plus Mie, with the optical depth along a ray
// taken from the Kasten and Young relative air mass fit rather than a numerical integration.
// That is a handful of exps per pixel instead of a nested raymarch, and it still reddens the sun
// and warms the horizon on its own because the coefficients are wavelength dependent.
//
// The clouds are a bounded slab raymarch with Beer-Lambert extinction and front to back
// compositing. The pieces worth naming, since they are all standard and the references explain
// them better than a comment can:
//
//   Shape and detail split, so cheap low frequency noise decides where to spend the expensive
//   octaves, and the detail only ever erodes what the shape allowed
//   http://advances.realtimerendering.com/s2015/ (Schneider, Horizon Zero Dawn clouds)
//
//   Energy conserving scattering integration, so a step's result does not depend on its length
//   https://blog.selfshadow.com/publications/s2016-shading-course/ (Hillaire, Frostbite volumetrics)
//
//   Lighting from a directional derivative, one extra density sample instead of a light march
//   https://iquilezles.org/articles/derivative/

// ---------------------------------------------------------------------------------------------
// Phase functions
// ---------------------------------------------------------------------------------------------

// Angular distribution of light scattered by molecules much smaller than the wavelength
float rayleighPhase(float cosTheta)
{
    return 0.05968310365 * (1.0 + cosTheta * cosTheta); // 3 / (16 pi)
}

// Henyey-Greenstein, the standard cheap stand-in for Mie scattering. g near 1 throws light
// forward, which is what puts a halo around the sun and a bright rim on a backlit cloud.
float henyeyGreenstein(float cosTheta, float g)
{
    float gg = g * g;
    float denom = 1.0 + gg - 2.0 * g * cosTheta;
    return 0.07957747155 * (1.0 - gg) / (denom * sqrt(max(denom, 1e-4))); // 1 / (4 pi)
}

// Clouds scatter strongly forward but keep a backward lobe, which is what lights the cloud face
// pointing at the camera when the sun is behind it. One lobe cannot do both. Scaled by 4 pi so an
// isotropic cloud would come out at 1 and the constants below stay readable.
float cloudPhase(float cosTheta)
{
    return 12.56637061 * lerp(henyeyGreenstein(cosTheta, CLOUD_PHASE_BACK),
                              henyeyGreenstein(cosTheta, CLOUD_PHASE_FORWARD),
                              CLOUD_PHASE_BLEND);
}

// ---------------------------------------------------------------------------------------------
// Sun
// ---------------------------------------------------------------------------------------------

// Direction from the scene towards the sun.
//
// It swings rather than circling. A full revolution puts the sun behind the scene for half of it,
// which leaves every camera facing surface backlit and unreadable, and the elevation stays well
// above the horizon for the same reason. The two periods do not divide evenly, so the lighting
// does not visibly repeat.
float3 getSunDirection()
{
    float azimuth = SUN_AZIMUTH + SUN_AZIMUTH_SWEEP * sin(time * SUN_SPEED);
    float elevation = radians(lerp(SUN_MIN_ELEVATION, SUN_MAX_ELEVATION,
                                   0.5 + 0.5 * sin(time * SUN_SPEED * 0.37)));

    float cosE = cos(elevation);
    return float3(cosE * cos(azimuth), sin(elevation), cosE * sin(azimuth));
}

// ---------------------------------------------------------------------------------------------
// Atmosphere
// ---------------------------------------------------------------------------------------------

// Relative air mass, Kasten and Young 1989. It is 1 at the zenith and about 38 at the horizon,
// which is the ratio a numerical integration through an exponential atmosphere would give.
float relativeAirMass(float cosZenith)
{
    float c = max(cosZenith, 0.0);
    float zenithDegrees = degrees(acos(min(c, 1.0)));
    return 1.0 / (c + 0.50572 * pow(max(96.07995 - zenithDegrees, 1e-3), -1.6364));
}

// Optical depth split into its Rayleigh and Mie parts. Scale heights turn the sea level
// coefficients into the depth of the whole column, which is what the air mass then scales.
void atmosphereOpticalDepth(float cosZenith, out float3 tauRayleigh, out float3 tauMie)
{
    float airMass = relativeAirMass(cosZenith);
    tauRayleigh = RAYLEIGH_BETA * RAYLEIGH_SCALE_HEIGHT * airMass;
    tauMie = MIE_BETA * MIE_SCALE_HEIGHT * airMass;
}

// How much of the sun's light survives the trip down to the viewer. This is the only reason the
// sun and everything it lights turn orange as it drops.
float3 getSunTransmittance(float3 sunDir)
{
    float3 tauRayleigh, tauMie;
    atmosphereOpticalDepth(sunDir.y, tauRayleigh, tauMie);
    return exp(-(tauRayleigh + tauMie));
}

// Radiance the sun delivers to the sky and the clouds, already extinguished by the atmosphere
float3 getSunRadiance(float3 sunDir)
{
    return SUN_INTENSITY * getSunTransmittance(sunDir);
}

// Sun colour for shading surfaces. This is the transmittance on its own rather than the full
// radiance, so the scene keeps the exposure it had while still warming and dimming as the sun
// drops towards the horizon.
float3 getSunLightColor(float3 sunDir)
{
    return getSunTransmittance(sunDir) * SUN_LIGHT_INTENSITY;
}

// Sky radiance along a view ray, without the sun disk and without clouds.
//
// Single scattering with the source term held constant along the ray, which integrates in closed
// form to scatteringAlbedo * phase * (1 - exp(-tau)).
float3 getSkyRadiance(float3 rayDir, float3 sunDir)
{
    float cosTheta = dot(rayDir, sunDir);

    float3 tauRayleigh, tauMie;
    atmosphereOpticalDepth(rayDir.y, tauRayleigh, tauMie);

    float3 tau = tauRayleigh + tauMie;
    float3 opacity = 1.0 - exp(-tau);

    float3 tauSunRayleigh, tauSunMie;
    atmosphereOpticalDepth(sunDir.y, tauSunRayleigh, tauSunMie);

    // A constant source term lights the far end of a horizon ray as brightly as the near end,
    // which leaves the horizon white. Charging the source a share of the view depth, as extra air
    // between the sun and the average scattering point, is what warms it instead.
    float3 sunTransmittance = exp(-(tauSunRayleigh + tauSunMie + tau * SKY_INSCATTER_SELF_SHADOW));

    float3 scattered = tauRayleigh * rayleighPhase(cosTheta) +
                       tauMie * henyeyGreenstein(cosTheta, MIE_G);

    float3 sky = (scattered / max(tau, 1e-6)) * opacity * sunTransmittance * SUN_INTENSITY * SKY_EXPOSURE;

    // Stand-in for the scattering orders this model drops. Without it the sky is too dark and too
    // saturated where the air is thickest, because all the blue has been scattered out and none
    // of it ever gets scattered back in.
    sky += SKY_MULTISCATTER * opacity * getSunTransmittance(sunDir) * saturate(sunDir.y + 0.1);

    // Below the horizon the air mass fit is meaningless, so fade into a dull ground haze
    return lerp(GROUND_HAZE_COLOR * saturate(sunDir.y + 0.2), sky, smoothstep(-0.08, 0.02, rayDir.y));
}

// The disk itself plus the forward scattered glow around it. Limb darkening is the reason a real
// solar disk is not a flat white circle, and it is one multiply here.
float3 getSunDisk(float3 rayDir, float3 sunDir)
{
    float cosTheta = dot(rayDir, sunDir);

    float cosInner = cos(radians(SUN_ANGULAR_RADIUS));
    float cosOuter = cos(radians(SUN_ANGULAR_RADIUS * 1.15));
    float disk = smoothstep(cosOuter, cosInner, cosTheta);

    // Fraction of the way from the centre of the disk to its edge
    float edge = saturate((1.0 - cosTheta) / max(1.0 - cosInner, 1e-6));
    float limb = pow(max(1.0 - edge * edge, 0.0), 0.28);

    float glow = pow(saturate(cosTheta), SUN_GLOW_FALLOFF) * SUN_GLOW_STRENGTH;

    // The disk is only visible above the horizon, and it dims through the same air as its light
    float horizon = smoothstep(-0.02, 0.03, rayDir.y);

    return (disk * limb * SUN_DISK_BRIGHTNESS + glow) * getSunRadiance(sunDir) * horizon;
}

// ---------------------------------------------------------------------------------------------
// Cloud noise
// ---------------------------------------------------------------------------------------------

// Value noise on a 3D lattice, read out of a 2D texture.
//
// Slice z of the lattice is the 2D noise offset by (37, 239) texels, so the red channel holds the
// slice at floor(z) and the green channel the one above it, and the two are just an interpolation
// apart. See CreateCloudNoise in RenderDevice for the generator. This costs one bilinear fetch
// per lattice cell instead of the eight hashes a procedural version would need.
float cloudNoise(float3 p)
{
    float3 cell = floor(p);
    float3 f = p - cell;
    f = f * f * (3.0 - 2.0 * f); // smoothstep, so the lattice does not show as creases

    float2 uv = cell.xy + float2(37.0, 239.0) * cell.z + f.xy;
    float2 rg = cloudNoiseTexture.SampleLevel(wrapSampler, (uv + 0.5) / CLOUD_NOISE_SIZE, 0.0).rg;

    return lerp(rg.x, rg.y, f.z);
}

// Rotate and scale between octaves. An axis aligned doubling lines the octaves up on the same
// lattice planes and leaves a visible grid, this shears them apart.
static const float3x3 CLOUD_OCTAVE_STEP = float3x3(
    0.00, 1.60, 1.20,
    -1.60, 0.72, -0.96,
    -1.20, -0.96, 1.28);

// Fractal brownian motion, normalised back to [0, 1] so the octave count does not change the
// range the coverage threshold is compared against
float cloudFbm(float3 p, int octaves)
{
    float sum = 0.0;
    float amplitude = 0.5;
    float total = 0.0;

    [loop]
    for (int i = 0; i < octaves; i++)
    {
        sum += amplitude * cloudNoise(p);
        total += amplitude;

        p = mul(CLOUD_OCTAVE_STEP, p);
        amplitude *= 0.5;
    }

    return sum / total;
}

float remap(float value, float inMin, float inMax, float outMin, float outMax)
{
    return outMin + (value - inMin) * (outMax - outMin) / (inMax - inMin);
}

// Vertical profile of a cumulus: rounded underside, flat spreading top. Multiplying the noise by
// this is what stops the slab reading as a uniform block of fog.
float cloudHeightGradient(float heightFraction)
{
    return saturate(remap(heightFraction, 0.0, 0.18, 0.0, 1.0)) *
           saturate(remap(heightFraction, 0.45, 1.0, 1.0, 0.0));
}

// Where clouds are at all, from the two cheapest octaves. Zero means the march can stride past
// this point without evaluating anything else, which is most of what the march does.
//
// Remapping against coverage rather than subtracting it keeps the surviving cloud at full
// density, so raising coverage grows the clouds instead of fogging the whole slab.
// The cloud layer follows the planet rather than being flat, which is what lets it meet the
// horizon instead of running into it like a ceiling. The centre is far below the scene, so within
// the scene itself this is indistinguishable from a flat layer.
static const float3 CLOUD_PLANET_CENTER = float3(0.0, -CLOUD_PLANET_RADIUS, 0.0);

float cloudAltitude(float3 p)
{
    return length(p - CLOUD_PLANET_CENTER) - CLOUD_PLANET_RADIUS;
}

float cloudShape(float3 p)
{
    float heightFraction = (cloudAltitude(p) - CLOUD_BOTTOM) / (CLOUD_TOP - CLOUD_BOTTOM);
    if (heightFraction < 0.0 || heightFraction > 1.0)
    {
        return 0.0;
    }

    float3 q = p * CLOUD_FREQUENCY + CLOUD_WIND * time;
    float base = cloudFbm(q, CLOUD_SHAPE_OCTAVES);

    return saturate(remap(base, 1.0 - CLOUD_COVERAGE, 1.0, 0.0, 1.0)) *
           cloudHeightGradient(heightFraction);
}

// Detail octaves, which only ever take density away. Keeping erosion one sided is what lets the
// march trust cloudShape as a conservative bound and skip on it.
float cloudDensityFromShape(float3 p, float shape, int detailOctaves)
{
    if (shape <= 0.0)
    {
        return 0.0;
    }

    float3 q = p * CLOUD_DETAIL_FREQUENCY + CLOUD_WIND * time * CLOUD_DETAIL_DRIFT;
    float detail = cloudFbm(q, detailOctaves);

    return shape * lerp(1.0, detail, CLOUD_EROSION) * CLOUD_DENSITY;
}

float cloudDensity(float3 p, int detailOctaves)
{
    return cloudDensityFromShape(p, cloudShape(p), detailOctaves);
}

// ---------------------------------------------------------------------------------------------
// Cloud raymarch
// ---------------------------------------------------------------------------------------------

// Distance to where a ray leaves a sphere centred under the scene, or -1 if it misses. The camera
// sits inside the layer's inner surface, so the far root is always the one that matters.
float raySphereExit(float3 rayOrigin, float3 rayDir, float radius)
{
    float3 toCenter = rayOrigin - CLOUD_PLANET_CENTER;

    float b = dot(toCenter, rayDir);
    float c = dot(toCenter, toCenter) - radius * radius;
    float discriminant = b * b - c;

    if (discriminant < 0.0)
    {
        return -1.0;
    }

    return -b + sqrt(discriminant);
}

// Entry and exit distances for the cloud shell. Returns false when the ray never reaches it,
// which is the cheapest thing this file does and skips the march for every downward ray.
bool cloudSlabRange(float3 rayOrigin, float3 rayDir, out float tMin, out float tMax)
{
    tMin = 0.0;
    tMax = 0.0;

    if (rayDir.y < CLOUD_MIN_RAY_SLOPE)
    {
        return false;
    }

    tMin = raySphereExit(rayOrigin, rayDir, CLOUD_PLANET_RADIUS + CLOUD_BOTTOM);
    tMax = raySphereExit(rayOrigin, rayDir, CLOUD_PLANET_RADIUS + CLOUD_TOP);

    if (tMin < 0.0 || tMax < 0.0)
    {
        return false;
    }

    // Past the cutoff the shell is thinner than a step, and the curvature has already dropped it
    // towards the horizon, so there is nothing left worth marching
    tMax = min(tMax, CLOUD_MAX_DIST);

    return tMax > tMin;
}

// Sunlight reaching a point in the cloud.
//
// A light march would cost several density samples per step. The directional derivative gives the
// same cue from one: comparing the density here with the density a short way towards the sun says
// whether this point sits on a lit face or buried inside, and Beer-Lambert on the local density
// supplies the depth falloff.
float cloudLighting(float3 p, float shape, float density, float3 sunDir)
{
    // Compared on the shape rather than the full density: what shadows a point is the body of
    // cloud above it, which is all in the low frequency octaves, and this halves the fetches.
    float towardsSun = cloudShape(p + sunDir * CLOUD_LIGHT_STEP);
    float gradient = saturate((shape - towardsSun) / CLOUD_LIGHT_CONTRAST);

    float beer = exp(-density * CLOUD_LIGHT_ABSORPTION);

    // Powder: a lit edge is darker than Beer-Lambert predicts, because light scattered towards
    // the eye there has had less chance to scatter back out of the cloud
    float powder = 1.0 - exp(-density * 2.0 * CLOUD_LIGHT_ABSORPTION);

    return (CLOUD_LIGHT_FLOOR + gradient) * beer * lerp(1.0, powder, CLOUD_POWDER);
}

// Marches the slab front to back and returns scattered light in rgb, coverage in a
float4 renderClouds(float3 rayOrigin, float3 rayDir, float3 sunDir, float3 sunColor, float3 skyColor,
                    float dither)
{
    float tMin, tMax;
    if (!cloudSlabRange(rayOrigin, rayDir, tMin, tMax))
    {
        return float4(0, 0, 0, 0);
    }

    float phase = cloudPhase(dot(rayDir, sunDir));

    // Light bouncing in from the rest of the sky, which is what fills the shadowed undersides
    float3 ambient = skyColor * CLOUD_AMBIENT;

    float baseStep = (tMax - tMin) / CLOUD_STEPS;

    // Starting every ray at the same depth turns the step size into visible bands. Offsetting each
    // pixel by a fraction of a step trades those for noise, which reads as grain.
    float t = tMin + baseStep * dither;

    float3 scattering = 0.0;
    float transmittance = 1.0;

    [loop]
    for (int i = 0; i < CLOUD_STEPS; i++)
    {
        if (t > tMax || transmittance < CLOUD_MIN_TRANSMITTANCE)
        {
            break;
        }

        // Steps grow with distance, where a step covers fewer pixels anyway
        float dt = baseStep * (1.0 + t * CLOUD_STEP_GROWTH);

        float3 p = rayOrigin + rayDir * t;
        float shape = cloudShape(p);

        if (shape <= 0.0)
        {
            // Nothing here and, because the detail octaves only erode, nothing the detail could
            // have added either. Two texture fetches was the whole cost of this step.
            t += dt;
            continue;
        }

        // Detail stops being resolvable with distance, so stop paying for it
        int detailOctaves = (int) clamp(CLOUD_DETAIL_OCTAVES - t * CLOUD_LOD_FALLOFF,
                                        1.0, CLOUD_DETAIL_OCTAVES);

        float density = cloudDensityFromShape(p, shape, detailOctaves);

        if (density > CLOUD_MIN_DENSITY)
        {
            float3 luminance = ambient +
                               sunColor * cloudLighting(p, shape, density, sunDir) * phase * CLOUD_SUN_GAIN;

            // Analytic integration of scattering across the step. Dividing out the extinction is
            // what makes the result independent of dt, so the LOD and the growing step size do not
            // change the brightness.
            float extinction = density * CLOUD_EXTINCTION;
            float stepTransmittance = exp(-extinction * dt);

            scattering += transmittance * luminance * density * (1.0 - stepTransmittance) / extinction;
            transmittance *= stepTransmittance;
        }

        t += dt;
    }

    // Aerial perspective. Far clouds sit behind more air than near ones, so they lose contrast
    // against the sky rather than staying crisp out to the horizon.
    float haze = saturate(tMin / CLOUD_MAX_DIST);
    haze *= haze;

    float alpha = (1.0 - transmittance) * (1.0 - haze);

    return float4(scattering * (1.0 - haze), alpha);
}

// ---------------------------------------------------------------------------------------------
// Entry points
// ---------------------------------------------------------------------------------------------

// Sky and sun only. Used where a full cloud march is not worth it, such as reflection rays and
// the colour distant geometry fogs towards.
float3 getSkyColor(float3 rayDir)
{
    float3 sunDir = getSunDirection();
    return getSkyRadiance(rayDir, sunDir) + getSunDisk(rayDir, sunDir);
}

// The whole background: sky, sun disk, and clouds composited over both
float3 getSkyColorWithClouds(float3 rayOrigin, float3 rayDir, float dither)
{
    float3 sunDir = getSunDirection();

    float3 sky = getSkyRadiance(rayDir, sunDir);
    float3 color = sky + getSunDisk(rayDir, sunDir);

    float4 clouds = renderClouds(rayOrigin, rayDir, sunDir, getSunRadiance(sunDir), sky, dither);

    return color * (1.0 - clouds.a) + clouds.rgb;
}
