// Sky, sun and clouds, built for cost rather than for physical accuracy.
//
// The sky is a vertical gradient between three colours with a warm band near the sun, which is the
// shape Unity's procedural skybox produces and is a few instructions rather than a scattering
// integral. The clouds are a single flat layer: the view ray is intersected with one plane and the
// noise is sampled there, so there is no volume and no march. A whole sky pixel costs two fBM
// evaluations, and nothing here reads a texture or needs a sampler.
//
// The tradeoff this makes, on purpose: clouds have no thickness. They cannot be flown into, they
// do not self shadow properly, and looking straight up shows a flat sheet rather than a base. In
// exchange the whole file is close to free.

// ---------------------------------------------------------------------------------------------
// Sun
// ---------------------------------------------------------------------------------------------

// Direction from the scene towards the sun.
//
// It swings rather than circling. A full revolution puts the sun behind the scene for half of it,
// which leaves every camera facing surface backlit, and the elevation stays well above the horizon
// for the same reason. The two periods do not divide evenly, so the lighting does not repeat.
float3 getSunDirection()
{
    float azimuth = SUN_AZIMUTH + SUN_AZIMUTH_SWEEP * sin(time * SUN_SPEED);
    float elevation = radians(lerp(SUN_MIN_ELEVATION, SUN_MAX_ELEVATION,
                                   0.5 + 0.5 * sin(time * SUN_SPEED * 0.37)));

    float cosE = cos(elevation);
    return float3(cosE * cos(azimuth), sin(elevation), cosE * sin(azimuth));
}

// Sunlight colour, warming as the sun drops. A real atmosphere gets this from wavelength dependent
// extinction, one lerp gets close enough to sell it.
float3 getSunLightColor(float3 sunDir)
{
    return lerp(SUN_COLOR_LOW, SUN_COLOR_HIGH, saturate(sunDir.y * SUN_COLOR_FALLOFF)) * SUN_LIGHT_INTENSITY;
}

// ---------------------------------------------------------------------------------------------
// Sky
// ---------------------------------------------------------------------------------------------

// Ground below, a bright band at the horizon, sky tint above, and a warm glow spreading out from
// the sun along the horizon. All of it is lerps, so it costs the same wherever the ray points.
float3 getSkyGradient(float3 rayDir, float3 sunDir)
{
    float height = rayDir.y;

    float3 sky = lerp(SKY_HORIZON_COLOR, SKY_ZENITH_COLOR, pow(saturate(height), SKY_GRADIENT_POWER));
    sky = lerp(SKY_GROUND_COLOR, sky, smoothstep(-0.12, 0.005, height));

    // The haze is strongest at the horizon and towards the sun, which is where a real sky has the
    // longest air path and the most forward scattering
    float towardsSun = saturate(dot(rayDir, sunDir));
    sky += SKY_SUN_HAZE * pow(towardsSun, SKY_SUN_HAZE_FALLOFF) * (1.0 - saturate(height * 2.0));

    // The whole gradient follows the sun down, so a low sun leaves a dimmer, warmer sky
    return sky * lerp(SKY_DUSK_TINT, float3(1, 1, 1), saturate(sunDir.y * 2.2)) * SKY_EXPOSURE;
}

// The disk plus the glare around it
float3 getSunDisk(float3 rayDir, float3 sunDir)
{
    float cosTheta = dot(rayDir, sunDir);

    float cosInner = cos(radians(SUN_ANGULAR_RADIUS));
    float cosOuter = cos(radians(SUN_ANGULAR_RADIUS * 1.4));
    float disk = smoothstep(cosOuter, cosInner, cosTheta);

    float glow = pow(saturate(cosTheta), SUN_GLOW_FALLOFF) * SUN_GLOW_STRENGTH;

    // Below the horizon there is ground in the way
    float horizon = smoothstep(-0.02, 0.03, rayDir.y);

    return (disk * SUN_DISK_BRIGHTNESS + glow) * getSunLightColor(sunDir) * horizon;
}

// ---------------------------------------------------------------------------------------------
// Cloud noise
// ---------------------------------------------------------------------------------------------

// Procedural, so this needs no texture, no sampler and no engine side change. A 2D lattice is four
// hashes per octave, which is cheap enough here only because the clouds are a plane and not a
// volume: this gets evaluated twice per sky pixel rather than a hundred times.
float cloudHash(float2 p)
{
    p = frac(p * float2(123.34, 456.21));
    p += dot(p, p + 45.32);
    return frac(p.x * p.y);
}

float cloudValueNoise(float2 p)
{
    float2 cell = floor(p);
    float2 f = p - cell;
    f = f * f * (3.0 - 2.0 * f); // smoothstep, so the lattice does not show as creases

    float a = cloudHash(cell);
    float b = cloudHash(cell + float2(1.0, 0.0));
    float c = cloudHash(cell + float2(0.0, 1.0));
    float d = cloudHash(cell + float2(1.0, 1.0));

    return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
}

// Rotate and scale between octaves. An axis aligned doubling lines the octaves up on the same
// lattice lines and leaves a visible grid, this shears them apart.
static const float2x2 CLOUD_OCTAVE_STEP = float2x2(1.6, 1.2, -1.2, 1.6);

// Fractal brownian motion, normalised back to [0, 1] so the coverage threshold means the same
// thing whatever the octave count is
float cloudFbm(float2 p)
{
    float sum = 0.0;
    float amplitude = 0.5;
    float total = 0.0;

    [unroll]
    for (int i = 0; i < CLOUD_OCTAVES; i++)
    {
        sum += amplitude * cloudValueNoise(p);
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

// Remapping against coverage rather than subtracting it keeps the surviving cloud at full density,
// so raising coverage grows the clouds instead of fogging the whole sky
float cloudDensity(float2 uv)
{
    return saturate(remap(cloudFbm(uv), 1.0 - CLOUD_COVERAGE, 1.0, 0.0, 1.0));
}

// ---------------------------------------------------------------------------------------------
// Cloud layer
// ---------------------------------------------------------------------------------------------

// Where the view ray crosses the cloud plane, sampled once. Returns colour in rgb, coverage in a.
float4 renderClouds(float3 rayOrigin, float3 rayDir, float3 sunDir, float3 sunColor, float3 skyColor)
{
    // A ray this flat crosses the plane past the fade distance anyway, and dividing by it is what
    // would send the sample coordinates to infinity
    if (rayDir.y < CLOUD_MIN_RAY_SLOPE)
    {
        return float4(0, 0, 0, 0);
    }

    float t = (CLOUD_HEIGHT - rayOrigin.y) / rayDir.y;
    if (t <= 0.0 || t > CLOUD_MAX_DIST)
    {
        return float4(0, 0, 0, 0);
    }

    float2 uv = (rayOrigin.xz + rayDir.xz * t) * CLOUD_FREQUENCY + CLOUD_WIND * time;

    float density = cloudDensity(uv);
    if (density <= 0.0)
    {
        return float4(0, 0, 0, 0);
    }

    // The one lighting cue a flat layer can give: sample again a short way towards the sun, and
    // where the cloud is thicker there this point is behind it. That is the whole shading model.
    float towardsSun = cloudDensity(uv + sunDir.xz * CLOUD_LIGHT_STEP);
    float lit = saturate((density - towardsSun) * CLOUD_LIGHT_CONTRAST + CLOUD_LIGHT_FLOOR);

    float3 color = lerp(skyColor * CLOUD_SHADOW_TINT, sunColor * CLOUD_LIT_TINT, lit);

    // Towards the horizon the plane runs away to infinity, so fade it out before it gets there
    float fade = 1.0 - saturate(t / CLOUD_MAX_DIST);
    fade *= fade;

    return float4(color, saturate(density * CLOUD_OPACITY) * fade);
}

// ---------------------------------------------------------------------------------------------
// Entry points
// ---------------------------------------------------------------------------------------------

// Sky and sun only, for reflection rays and the colour distant geometry fogs towards
float3 getSkyColor(float3 rayDir)
{
    float3 sunDir = getSunDirection();
    return getSkyGradient(rayDir, sunDir) + getSunDisk(rayDir, sunDir);
}

// The whole background: sky, sun disk, and the cloud layer over both
float3 getSkyColorWithClouds(float3 rayOrigin, float3 rayDir)
{
    float3 sunDir = getSunDirection();

    float3 sky = getSkyGradient(rayDir, sunDir);
    float3 color = sky + getSunDisk(rayDir, sunDir);

    float4 clouds = renderClouds(rayOrigin, rayDir, sunDir, getSunLightColor(sunDir), sky);

    return lerp(color, clouds.rgb, clouds.a);
}
