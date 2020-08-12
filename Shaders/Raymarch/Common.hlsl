struct RaymarchObject
{
	int raymarchShape;
	float4 primitiveOptions;
	float3 position;
	float3 eulerAngles;
	float3 scale;
	float4 color;
	float2 materialOptions;
};

cbuffer ShaderBuffer : register(b0)
{
    float4x4 viewMatrix;
	float3 cameraPosition;
	float aspectRatio;
	float time;
	float objectCount;
	float2 blank1;
	float4 blank2;
	float4 blank3;
};

uniform StructuredBuffer<RaymarchObject> objects : register(t0); 

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