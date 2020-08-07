#include "Common.hlsl"
#include "Primitives.hlsl"

#define MAX_STEPS 150
#define MAX_DIST 500
#define SURF_DIST 1e-2
#define MAX_OBJECTS 64

float GetDist(float3 p) {
	float4 s = float4(0, 1, 6, 1);

	float sphereDist = length(p - s.xyz) - s.w;
	float planeDist = p.y;

	float d = min(sphereDist, planeDist);
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

	//float3 lightPos = _WorldSpaceLightPos0.xyz; //float3(0, 5, 6);
	//lightPos.xz += vec2(sin(iTime), cos(iTime)) * 2.;
	float3 l = normalize(float3(0, 5, 6));// normalize(lightPos - p);
	float3 n = GetNormal(p);

	float dif = clamp(dot(n, l), 0., 1.);
	//float d = Raymarch(p + n * SURF_DIST * 2., l);
	/*if (d < length(lightPos - p)) {
		dif *= .1;
	}*/

	return dif;
}

float4 main(PS_INPUT input) : SV_Target
{

	float2 uv = input.TexCoord - float2(0.5, 0.5);
	float3 ro = float3(0, 1, 0);
	float3 rd = normalize(float3(uv.x, uv.y, 1.0));

	float d = Raymarch(ro, rd);

	float3 col = float3(0, 0, 0);

	float3 p = ro + rd * d;

	float dif = GetLight(p);

	col.rgb = float3(p);


	return float4(col, 1.0);
}