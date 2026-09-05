#include "Common.hlsl"
#include "Draw.hlsl"

static cLight mainLight;

// Calculate surface normal at position
float3 getNormal(in float3 pos)
{
    float2 e = float2(0.01, 0);
    float3 n = getDist(pos) - float3(
        getDist(pos - e.xyy),
        getDist(pos - e.yxy),
        getDist(pos - e.yyx)
    );

    return normalize(n);
}

// Marches a ray until it reaches a surface or runs out of budget.
//
// A macro rather than a function taking its budget as arguments. Passing them turns the loop
// bound into a runtime value, and measured on this scene that cost 40 percent of the frame,
// far more than the smaller reflection budget saves. Expanded per budget they stay constants.
//
// A cone rather than a ray: one pixel covers more of the world the further away it is, so
// converging tighter than the pixel it will be drawn into is work nobody can see. The angle
// constant is what a pixel subtends, so the epsilon tracks the footprint.
#define RAYMARCH_BODY(maxSteps, maxDistance, pixelAngle)                                        \
    raymarchResult.ray = ray;                                                                   \
    float totalDist = 0.0;                                                                      \
    float3 marchPos;                                                                            \
    float curDist;                                                                              \
                                                                                                \
    int i = 0;                                                                                  \
    [loop]                                                                                      \
    while (i < maxSteps)                                                                        \
    {                                                                                           \
        marchPos = ray.origin + totalDist * ray.dir;                                            \
        curDist = getDist(marchPos);                                                            \
        totalDist += curDist;                                                                   \
                                                                                                \
        if (curDist < max(SURF_DIST, totalDist * pixelAngle) || totalDist > maxDistance)        \
        {                                                                                       \
            break;                                                                              \
        }                                                                                       \
        i++;                                                                                    \
    }                                                                                           \
                                                                                                \
    float3 hitPos = ray.origin + totalDist * ray.dir;                                           \
                                                                                                \
    getDist(hitPos, raymarchResult.hitMaterial);                                                \
                                                                                                \
    raymarchResult.hitPos = hitPos;                                                             \
    raymarchResult.hitDistance = totalDist;                                                     \
    raymarchResult.surfaceNormal = getNormal(hitPos);

// The view ray, which gets the full budget
void raymarch(in cRay ray, out cRaymarchResult raymarchResult)
{
    RAYMARCH_BODY(MAX_STEPS, MAX_DIST, PIXEL_ANGLE)
}

// A reflection is attenuated by the surface before it is ever seen, so it converges on a coarser
// epsilon and gives up sooner without the difference showing
void raymarchReflection(in cRay ray, out cRaymarchResult raymarchResult)
{
    RAYMARCH_BODY(REFLECTION_MAX_STEPS, REFLECTION_MAX_DIST, REFLECTION_PIXEL_ANGLE)
}

// Get shadow at position
// lightDir is direction from object surface to light source
float getShadow(in cRaymarchResult raymarchResult, in float3 lightDir, float shadowHardness = 4,
                float shadowIntensity = 0.995)
{
    float res = 1.0;
    float3 rayOrigin = raymarchResult.hitPos + raymarchResult.surfaceNormal * 0.01;
    float mint = SURF_DIST * 2.0;
    float ph = 1e20;

    // t = distance from object surface towards light source.
    // Capped by step count too: at grazing angles h barely advances t.
    float t = mint;
    [loop]
    for (int i = 0; i < SHADOW_MAX_STEPS && t < SHADOW_MAX_DIST; i++)
    {
        float h = getDist(rayOrigin + lightDir * t);
        if (h < 0.001)
        {
            res = 0;
            break;
        }
        float y = h * h / (2.0 * ph);
        float d = sqrt(h * h - y * y);
        res = min(res, shadowHardness * d / max(0.0, t - y));
        ph = h;
        t += h;
    }

    return lerp(1.0 - shadowIntensity, 1.0, res);
}

// focalLength is the distance from the eye to the uv plane, so a larger value narrows the view.
// The basis arrives with the frame constants rather than being rebuilt here.
//
// It used to come from crossing the view direction with world up, which collapses when the two
// are parallel, so there was a branch onto a substitute up near the poles. The substitute is a
// different basis, not a continuation of the one it replaces, so crossing the threshold rolled
// the view by an amount that depended on the heading. MaxPitch is 88 degrees and the threshold
// stood at 89.4, which put the jump inside the range the player can actually look through: aiming
// all the way down flipped the picture. Three orthonormal axes straight from the camera rotation
// have no pole to handle.
float3 getCameraRayDir(float2 uv, float focalLength)
{
    return normalize(uv.x * cameraRight + uv.y * cameraUp + cameraDirection * focalLength);
}

// Surface colour at the hit point, which is the material colour with the checkerboard applied
// where one is asked for.
//
// checkers() derives the square size it is filtering over from the screen space derivatives, so
// the pattern fades to its own average with distance instead of tearing into moire.
float3 getAlbedo(in cRaymarchResult raymarchResult)
{
    float checkerSize = raymarchResult.hitMaterial.checkerSize;

    // Evaluated unconditionally and then masked, rather than guarded by an if.
    //
    // checkers() takes screen space derivatives, and those are only defined when every pixel in
    // the quad reaches them. Two neighbouring pixels landing on different materials took
    // different branches, which left the derivative undefined exactly along every silhouette.
    float square = checkers(raymarchResult.hitPos.xz / max(checkerSize, 1e-4));
    float pattern = lerp(CHECKER_DARK, 1.0, square);

    return raymarchResult.hitMaterial.diffuseColor * lerp(1.0, pattern, step(1e-4, checkerSize));
}

float3 getPhongLight(cRaymarchResult raymarchResult)
{
    float3 normal = raymarchResult.surfaceNormal;
    float3 lightReverseDir = mainLight.direction;
    float3 reverseRayDir = -raymarchResult.ray.dir;
    float3 R = reflect(-lightReverseDir, normal);

    float dotLN = dot(lightReverseDir, normal); // project light direction to surface normal
    float dotRV = dot(R, reverseRayDir); // project light reflection direction to direction to camera

    float3 color = float3(0, 0, 0);

    // Specular only counts where the light reaches the surface, so it nests inside the diffuse test
    if (dotLN > 0.0)
    {
        color += getAlbedo(raymarchResult) * dotLN;

        if (dotRV > 0.0)
        {
            color += raymarchResult.hitMaterial.specularColor * pow(dotRV, raymarchResult.hitMaterial.shininess);
        }
    }

    return color;
}

float3 getColor(cRaymarchResult raymarchResult)
{
    float3 color = float3(0, 0, 0);

    color += getPhongLight(raymarchResult);
    color *= mainLight.color;

    return color;
}

float3 getReflection(cRaymarchResult raymarchResult)
{
    cRay ray;
    cRaymarchResult refRaymarchResult;
    ray.Create(raymarchResult.hitPos + raymarchResult.surfaceNormal * 0.01,
               reflect(raymarchResult.ray.dir, raymarchResult.surfaceNormal));

    raymarchReflection(ray, refRaymarchResult);

    if (refRaymarchResult.hitDistance >= REFLECTION_MAX_DIST)
    {
        return getSkyColor(ray.dir);
    }

    return getPhongLight(refRaymarchResult);
}

// Ambient occlusion, by asking the distance field how enclosed the surface is.
//
// This used to be derived from how many steps the march took. That is a proxy, and it fails
// exactly at silhouettes: a ray grazing an edge takes far more steps than one hitting the same
// surface square on, so every object was drawn with a thin dark rim. Step count measures how hard
// the pixel was to trace, not how much sky the surface can see.
//
// Walking out along the normal and comparing the distance found against the distance walked
// measures the second thing. Open space returns the full step and contributes nothing, a nearby
// surface returns less and darkens.
float getAmbientOcclusion(float3 pos, float3 normal, float noise)
{
    float occlusion = 0.0;
    float weight = 1.0;

    // Unrolled. A fixed five iterations with no early exit is exactly what unrolling is for,
    // and it measures 0.7 ms a frame faster than the looped version.
    [unroll]
    for (int i = 0; i < AO_SAMPLES; i++)
    {
        // Jittered by the noise texture, so the fixed sample heights do not band on curved surfaces
        float height = AO_RADIUS * (i + noise) / AO_SAMPLES;

        occlusion += (height - getDist(pos + normal * height)) * weight;
        weight *= 0.85;
    }

    return saturate(1.0 - AO_STRENGTH * occlusion);
}

float4 main(PS_INPUT input) : SV_Target
{
    float3 noise = noiseTexture.Sample(textureSampler, input.TexCoord).rrr;

    float3 sunDir = getSunDirection();
    mainLight.Create(sunDir, getSunLightColor(sunDir));

    float2 uv = input.TexCoord - (0.5).xx;
    uv.x *= aspectRatio;

    cRay ray;
    ray.Create(cameraPosition, getCameraRayDir(uv, 1.0));

    cRaymarchResult raymarchResult;
    raymarch(ray, raymarchResult);

    // Only rays that missed the scene reach the cloud layer, which sits far past MAX_DIST
    if (raymarchResult.hitDistance >= MAX_DIST)
    {
        return float4(applyHud(toDisplay(getSkyColorWithClouds(ray.origin, ray.dir)),
                               input.TexCoord, debugValues.x), 1);
    }

    // Direct sunlight, which is the only thing the shadow ray occludes
    float3 sceneColor = getColor(raymarchResult) * getShadow(raymarchResult, mainLight.direction);

    // Sky fill. Shadowed surfaces still see the sky, so this is added after the shadow rather than
    // before it, and it scales with the surface colour: a dark material has to come out dark.
    //
    // Occlusion belongs here rather than on the whole result. It says how much of the sky the
    // surface can see, which is exactly what this term is, while direct sunlight is already
    // accounted for by the shadow ray. Applying it to both darkened lit surfaces twice.
    float occlusion = getAmbientOcclusion(raymarchResult.hitPos, raymarchResult.surfaceNormal, noise.x);
    sceneColor += getAlbedo(raymarchResult) * getSkyGradient(float3(0, 1, 0), sunDir) * SKY_AMBIENT * occlusion;

    // Reflection
    float reflectivity = saturate(raymarchResult.hitMaterial.diffraction);
    if (reflectivity > REFLECTION_MIN)
    {
        sceneColor += getReflection(raymarchResult) * reflectivity;
    }

    // Aerial perspective, towards the sky in the direction being looked at rather than a constant.
    // The direction is clamped to the horizon: the haze in front of distant ground is lit like the
    // sky just above it, and sampling below would hand back the unlit ground colour.
    //
    // Applied to linear radiance, before the curve, and falling off exponentially with distance
    // rather than linearly. Linear fog reached halfway to the sky colour by the middle of the
    // view, which is what greyed out everything in the foreground.
    float3 hazeDir = normalize(float3(ray.dir.x, max(ray.dir.y, 0.0), ray.dir.z));
    float fog = 1.0 - exp(-raymarchResult.hitDistance * FOG_DENSITY);

    // The exponential alone is only 45 percent of the way to the sky when the march gives up at
    // MAX_DIST. The floor is an infinite plane, so every ray below the horizon hits it and stops
    // there, and the ground ended in a flat band of its own colour with a hard step to the sky
    // along the top.
    //
    // Closing the rest of the way by MAX_DIST removes the step. It has to start far short of it:
    // ground distance runs to infinity at the horizon, so the last stretch of the range is worth
    // only a couple of dozen rows on screen, and a fade confined to it is still read as a band.
    // Beginning at FOG_HORIZON_START spreads the same fade over several times the pixels, which
    // is what turns the edge into distance.
    fog = max(fog, smoothstep(MAX_DIST * FOG_HORIZON_START, MAX_DIST, raymarchResult.hitDistance));

    sceneColor = lerp(sceneColor, getSkyColor(hazeDir), fog);

    return float4(applyHud(toDisplay(sceneColor), input.TexCoord, debugValues.x), 1);
}
