// Compiler adds array lengts as consts on RaymarchEngine include
#include "RaymarchEngine"
#include "Primitives.hlsl"

interface iBasePrimitive
{
    float ExecSDF(float3 from);
};

class cBasePrimitive 
{
    float4 primitiveOptions;
	float3 position;
	float3 eulerAngles;
	float3 scale;
	float4 color;
	float2 materialOptions;
};    

class cSphere : cBasePrimitive, iBasePrimitive
{
    float ExecSDF(float3 from){
        return sdSphere(from - position, primitiveOptions.x);
    }
};

class cBox : cBasePrimitive, iBasePrimitive
{
    float ExecSDF(float3 from){
        return sdBox(from - position, scale);
    }
};

class cPlane : cBasePrimitive, iBasePrimitive
{
    float ExecSDF(float3 from){
        return sdPlane(from - position);
    }
};

class cEllipsoid : cBasePrimitive, iBasePrimitive
{
    float ExecSDF(float3 from){
        return sdEllipsoid(from - position, scale);
    }
};

class cTorus : cBasePrimitive, iBasePrimitive
{
    float ExecSDF(float3 from){
        return sdTorus(from - position, primitiveOptions.xy);
    }
};

class cCappedTorus : cBasePrimitive, iBasePrimitive
{
    float ExecSDF(float3 from){
        return sdCappedTorus(from - position, primitiveOptions.xy, primitiveOptions.z, primitiveOptions.w);
    }
};

cbuffer ShaderBuffer : register(b0)
{
	float3 cameraPosition;
	float aspectRatio;
	float3 cameraDirection;
	float time;
};

uniform StructuredBuffer<cSphere> spheres : register(t0); 
uniform StructuredBuffer<cBox> boxes : register(t1); 
uniform StructuredBuffer<cPlane> planes : register(t2); 
uniform StructuredBuffer<cEllipsoid> ellipsoids : register(t3); 
uniform StructuredBuffer<cTorus> tori : register(t4); 
uniform StructuredBuffer<cCappedTorus> cappedTori : register(t5); 

struct VS_INPUT
{
	float4 Position : POSITION;
	float2 TexCoord : TEXCOORD;
};

struct PS_INPUT
{
	float4 Position : SV_POSITION;
	float2 TexCoord : TEXCOORD;
};