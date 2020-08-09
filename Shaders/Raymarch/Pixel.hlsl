#include "Common.hlsl"
#include "Primitives.hlsl"

#define MAX_STEPS 500
#define MAX_DIST 500
#define SURF_DIST 1e-2
#define MAX_OBJECTS 64

/*float raymarchObjectSd(RaymarchGameObjectBufferData shape, float3 p) {
    float d = MAX_DIST;
    float3 np = p - shape.position;

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
    }

    return d;
}*/

float GetDist(float3 p) {

	
	float d = min(sdSphere(p-float3(0, 1, 6), 1), sdPlane(p-float3(0,1,0)));

	return d;
}

// 
float3 GetNormal(in float3 p) {
	float2 e = float2(0.01, 0);
	float3 n = GetDist(p) - float3(
		GetDist(p - e.xyy),
		GetDist(p - e.yxy),
		GetDist(p - e.yyx)
		);

	return normalize(n);
}

// Return distance from ro to object in ray direction rd
float Raymarch(in float3 ro, in float3 rd) {
	float dO = 0.0;

	for (int i = 0; i < MAX_STEPS; i++) {
		float3 p = ro + dO * rd;
		float dS = GetDist(p);
		dO += dS;
		if (dS < SURF_DIST || dO > MAX_DIST) {
			break;
		}
	}

	return dO;
}

float GetLight(in float3 p) {

	float3 lightPos = float3(0, 5, 6);

	// Move light
	lightPos.xz += float2(sin(time), cos(time)) * 2.;

	float3 l = normalize(lightPos - p);
	float3 n = GetNormal(p);

	float dif = clamp(dot(n, l), 0., 1.);

	// Calcualte hard shadow
	float d = Raymarch(p + n * SURF_DIST * 2., l);
	if (d < length(lightPos - p)) {
		dif *= .1;
	}

	return dif;
}

float4 main(PS_INPUT input) : SV_Target
{

	float2 uv = input.TexCoord - (0.5).xx;
	uv.x *= aspectRatio;
	//uv *= 0.5;
	float3 ro = cameraPosition; // TODO position from model matrices
	float3 rd = normalize(mul(viewMatrix, float4(uv.xy, 1.0, 0.0))).xyz; // FOV comes from uv coords
	//rd.x *= aspectRatio;

	float d = Raymarch(ro, rd);

	float3 col = (0).xxx;

	float3 p = ro + rd * d;

	float dif = GetLight(p);
	col = dif.xxx;

	float gamma = 0.4545;

	col = pow(col, gamma.xxx); // Gamma correction

	return float4(col, 1.0);
}