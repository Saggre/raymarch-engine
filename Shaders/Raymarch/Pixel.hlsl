#include "Common.hlsl"
#include "Draw.hlsl"

static cLight mainLight;

// Calculate surface normal at position
float3 getNormal(in float3 pos)
{
    cMaterial material;
    float2 e = float2(0.01, 0);
    float3 n = getDist(pos, material) - float3(
        getDist(pos - e.xyy, material),
        getDist(pos - e.yxy, material),
        getDist(pos - e.yyx, material)
    );

    return normalize(n);
}

// Returns distance from rayOrigin to an object in the GetDist() scene, in ray direction rayDir
void raymarch(in cRay ray, out cRaymarchResult raymarchResult)
{
    raymarchResult.ray = ray;
    float totalDist = 0.0;
    float3 marchPos;
    float curDist;

    int i = 0;
    [loop]
    while (i < MAX_STEPS)
    {
        marchPos = ray.origin + totalDist * ray.dir;
        curDist = getDist(marchPos, raymarchResult.hitMaterial);
        totalDist += curDist;
        if (curDist < SURF_DIST || totalDist > MAX_DIST)
        {
            break;
        }
        i++;
    }

    float3 hitPos = ray.origin + totalDist * ray.dir;
    raymarchResult.hitPos = hitPos;
    raymarchResult.stepsTaken = i * 1.0;
    raymarchResult.hitDistance = totalDist;
    raymarchResult.surfaceNormal = getNormal(hitPos);
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

    cMaterial material;

    // t = distance from object surface towards light source.
    // Capped by step count too: at grazing angles h barely advances t.
    float t = mint;
    [loop]
    for (int i = 0; i < SHADOW_MAX_STEPS && t < SHADOW_MAX_DIST; i++)
    {
        float h = getDist(rayOrigin + lightDir * t, material);
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
float3 getCameraRayDir(float2 uv, float focalLength)
{
    float3 camForward = normalize(cameraDirection);

    // cross() collapses to zero when the view direction is parallel to world up, giving NaN rays.
    // The sign keeps the basis handed the same way at both poles.
    float3 worldUp = abs(camForward.y) > 0.999
                         ? float3(0.0, 0.0, -sign(camForward.y))
                         : float3(0.0, 1.0, 0.0);

    float3 camRight = normalize(cross(worldUp, camForward));
    float3 camUp = normalize(cross(camForward, camRight));

    return normalize(uv.x * camRight + uv.y * camUp + camForward * focalLength);
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
        color += raymarchResult.hitMaterial.diffuseColor * dotLN;

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

    raymarch(ray, refRaymarchResult);

    if (refRaymarchResult.hitDistance >= MAX_DIST)
    {
        return getSkyColor(ray.dir);
    }

    return getPhongLight(refRaymarchResult);
}

// AO
// Will be of lower resolution with low MAX_STEPS, because it's calculated from raymarch steps taken
float getAmbientOcclusion(in cRaymarchResult raymarchResult, float noise)
{
    float AO = pow(1.0 - (raymarchResult.stepsTaken / MAX_STEPS), 8 * noise);
    return lerp(AO, 1, saturate(raymarchResult.hitDistance / AO_FALLOFF));
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
        return float4(saturate(pow(getSkyColorWithClouds(ray.origin, ray.dir), 1.0 / 2.2)), 1);
    }

    float3 sceneColor = getColor(raymarchResult);

    float3 shadow = getShadow(raymarchResult, mainLight.direction);

    // Reflection
    sceneColor += getReflection(raymarchResult) * saturate(raymarchResult.hitMaterial.diffraction);

    float3 ambientColor = float3(0.05, 0.05, 0.06);
    sceneColor += ambientColor;

    sceneColor *= shadow;

    // Apply AO
    sceneColor *= getAmbientOcclusion(raymarchResult, noise);

    // Gamma correction
    sceneColor = pow(sceneColor, 1.0 / 2.2);

    // Aerial perspective, towards the sky in the direction being looked at rather than a constant.
    // The direction is clamped to the horizon: the haze in front of distant ground is lit like the
    // sky just above it, and sampling below would hand back the unlit ground colour.
    float3 hazeDir = normalize(float3(ray.dir.x, max(ray.dir.y, 0.0), ray.dir.z));
    float fogIntensity = raymarchResult.hitDistance / MAX_DIST;
    sceneColor = lerp(sceneColor, pow(getSkyColor(hazeDir), 1.0 / 2.2), fogIntensity);

    return float4(saturate(sceneColor), 1);
}
