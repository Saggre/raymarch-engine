//#include "RaymarchEngine"
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

class cPlane : cBasePrimitive, iBasePrimitive
{
    float ExecSDF(float3 from){
        return sdPlane(from - position);
    }
};

/*struct RaymarchObject
{
	int raymarchShape;
	float4 primitiveOptions;
	float3 position;
	float3 eulerAngles;
	float3 scale;
	float4 color;
	float2 materialOptions;
};*/

cbuffer ShaderBuffer : register(b0)
{
	float3 cameraPosition;
	float aspectRatio;
	float3 cameraDirection;
	float time;
	float objectCount;
	float3 blank1;
};

uniform StructuredBuffer<cSphere> spheres : register(t0); 

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