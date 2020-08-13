#include "Common.hlsl"

#define MAX_STEPS 700
#define MAX_DIST 400
#define SHADOW_MAX_DIST 200
#define SURF_DIST 1e-2
#define MAX_OBJECTS 64

// Get pseudo-random number in the range [0, 1).
float random(float2 co)
{
  return frac(sin(dot(co.xy, float2(12.9898, 78.233))) * 43758.5453);
}

float checkers(float2 p)
{
    float2 w = fwidth(p) + 0.001;
    float2 i = 2.0*(abs(frac((p-0.5*w)*0.5)-0.5)-abs(frac((p+0.5*w)*0.5)-0.5))/w;
    return 0.5 - 0.5*i.x*i.y;
}

// Get distance to an object depending on its shape parameter
float raymarchObjectSd(cSphere shape, float3 pos, out float3 color) {

    float dist = MAX_DIST;
    //float3 newPos = pos - shape.position;

    color = float3(1.0, 1.0, 1.0);

    // TODO replace this with something, it's very slow
    /*switch (shape.raymarchShape) {
        case 0:
            dist = sdSphere(newPos, shape.primitiveOptions.r); 
            color = float3(1.0, 0.0, 0.0);
            break;
        case 1:
            dist = sdBox(newPos, shape.primitiveOptions.rrr);
            break;
        case 2:
            dist = sdPlane(newPos);
            break;
        case 3:
            dist = sdEllipsoid(newPos, shape.primitiveOptions.rgb);
            break;
        case 4:
            dist = sdTorus(newPos, shape.primitiveOptions.rg);
            break;
        case 5:
            dist = sdCappedTorus(newPos, shape.primitiveOptions.rg, shape.primitiveOptions.b, shape.primitiveOptions.a);
            break;
    }*/
    
    dist = shape.ExecSDF(pos); 

    return dist;
}

// Returns distance from position to the closest point in scene geometry
float getDist(float3 pos, out float3 color) {
    float minDist = MAX_DIST;

    for(int i = 0; i<objectCount && i<MAX_OBJECTS; i++){
        float3 objectColor;
        float curDist = raymarchObjectSd(spheres[i], pos, objectColor);
        
        if(curDist < minDist){
            color = objectColor;    
        }
        
        minDist = min(minDist, curDist);
    }
    
	return minDist;
}

// Colorless version
float getDist(float3 pos) {
    float3 col;
    return getDist(pos, col);
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
float raymarch(in float3 rayOrigin, in float3 rayDir, out float3 color) {
	float totalDist = 0.0;

	[loop] for (int i = 0; i < MAX_STEPS; i++) {
		float3 marchPos = rayOrigin + totalDist * rayDir;
		float curDist = getDist(marchPos, color);
		totalDist += curDist;
		if (curDist < SURF_DIST || totalDist > MAX_DIST) {
			break;
		}
	}

    color = float3(checkers((rayOrigin + rayDir * totalDist).xz), 0.0, 0.0); 

	return totalDist;
}

// Colorless version
float raymarch(in float3 rayOrigin, in float3 rayDir) {
    float3 col;
    return raymarch(rayOrigin, rayDir, col);
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
    [loop] for(float t = mint; t < SHADOW_MAX_DIST;)
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

float3 getCameraRayDir(float2 uv, float fov)
{
    float3 camForward = cameraDirection;
    float3 camRight = normalize(cross(float3(0.0, 1.0, 0.0), camForward));
    float3 camUp = normalize(cross(camForward, camRight));
     
    return normalize(uv.x * camRight + uv.y * camUp + camForward * fov);
}

float4 main(PS_INPUT input) : SV_Target
{
    float3 FOG_COLOR = float3(0.2, 0.2, 0.3);

	float2 uv = input.TexCoord - (0.5).xx;
	uv.x *= aspectRatio;
	
	float3 rayOrigin = cameraPosition; // TODO position from model matrices
	float3 rayDir = getCameraRayDir(uv, 1.0);

    float3 objectColor;
	float dist = raymarch(rayOrigin, rayDir, objectColor);
	
	if (dist > MAX_DIST) {
	    return float4(FOG_COLOR, 1);
	    //discard;
	}

	float3 color;

	float3 hitPos = rayOrigin + rayDir * dist;

	float diffuse = getLight(hitPos);
	color = diffuse.xxx * objectColor;

	float gamma = 0.4545;

    // Gamma correction
	color = pow(color, gamma.xxx); 

    // Fog
    color = lerp(color, FOG_COLOR, dist / MAX_DIST);

	return float4(color, 1);
}