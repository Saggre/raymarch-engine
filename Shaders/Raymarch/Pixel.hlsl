#include "Common.hlsl"
#include "Primitives.hlsl"

#define MAX_STEPS 500
#define MAX_DIST 500
#define SHADOW_MAX_DIST 200
#define SURF_DIST 1e-2
#define MAX_OBJECTS 64

// Get pseudo-random number in the range [0, 1).
float random(float2 co)
{
  return frac(sin(dot(co.xy, float2(12.9898, 78.233))) * 43758.5453);
}

// Get distance to an object depending on its shape parameter
float raymarchObjectSd(RaymarchObject shape, float3 p) {
    float d = MAX_DIST;
    float3 np = p - shape.position;

    // TODO replace this with something, it's very slow
    switch (shape.raymarchShape) {
        case 0:
            d = sdSphere(np, shape.primitiveOptions.r); 
            break;
        case 1:
            d = sdBox(np, shape.primitiveOptions.rrr);
            break;
        case 2:
            d = sdPlane(np);
            break;
        case 3:
            d = sdEllipsoid(np, shape.primitiveOptions.rgb);
            break;
        case 4:
            d = sdTorus(np, shape.primitiveOptions.rg);
            break;
        case 5:
            d = sdCappedTorus(np, shape.primitiveOptions.rg, shape.primitiveOptions.b, shape.primitiveOptions.a);
            break;
    }

    return d;
}

// Returns distance from position to the closest point in scene geometry
float getDist(float3 pos) {
    float minDist = MAX_DIST;

    for(int i = 0; i<objectCount && i<MAX_OBJECTS; i++){
        float curDist = raymarchObjectSd(objects[i], pos);
        minDist = min(minDist, curDist);
    }
    
	return minDist;
}

// Calculate surface normal at position
float3 getNormal(in float3 pos) {
	float2 e = float2(0.01, 0);
	float3 n = getDist(pos) - float3(
		getDist(pos - e.xyy),
		getDist(pos - e.yxy),
		getDist(pos - e.yyx)
	);

	return normalize(n);
}

// Returns distance from rayOrigin to an object in the GetDist() scene, in ray direction rayDir
float raymarch(in float3 rayOrigin, in float3 rayDir) {
	float totalDist = 0.0;

	for (int i = 0; i < MAX_STEPS; i++) {
		float3 marchPos = rayOrigin + totalDist * rayDir;
		float curDist = getDist(marchPos);
		totalDist += curDist;
		if (curDist < SURF_DIST || totalDist > MAX_DIST) {
			break;
		}
	}

	return totalDist;
}

// Get shadow at position
// lightDir is direction from object surface to light source
float getShadow(in float3 pos, in float3 lightDir, float shadowHardness = 64, float shadowIntensity = 0.995)
{
    float res = 1.0;
    float3 rayOrigin = pos;
    float mint = SURF_DIST * 2.0;
    float ph = 1e20;
    
    // t = distance from object surface towards light source
    for(float t = mint; t < SHADOW_MAX_DIST;)
    {
        float h = getDist(rayOrigin + lightDir * t);
        if(h<0.001){
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

// Get light at position
float getLight(in float3 pos) {
	float3 lightPos = float3(0, 5, 6);

	// Move light
	lightPos.xz += float2(sin(time), cos(time)) * 2.0;

	float3 lightDir = normalize(lightPos - pos);
	float3 normal = getNormal(pos);

	float dif = clamp(dot(normal, lightDir), 0.0, 1.0);
    dif *= getShadow(pos, lightDir);

	return dif;
}

float4 main(PS_INPUT input) : SV_Target
{
	float2 uv = input.TexCoord - (0.5).xx;
	uv.x *= aspectRatio;
	float3 rayOrigin = cameraPosition; // TODO position from model matrices
	float3 rayDir = normalize(mul(viewMatrix, float4(uv.xy, 1.0, 0.0))).xyz; // FOV comes from uv coords

	float dist = raymarch(rayOrigin, rayDir);
	
	if (dist > MAX_DIST) {
	    discard;
	}

	float3 color = (0).xxx;

	float3 hitPos = rayOrigin + rayDir * dist;

	float diffuse = getLight(hitPos);
	color = diffuse.xxx;

	float gamma = 0.4545;

    // Gamma correction
	color = pow(color, gamma.xxx); 

	return float4(color, 1.0);
}