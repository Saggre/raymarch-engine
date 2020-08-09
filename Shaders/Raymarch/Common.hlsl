struct RaymarchGameObjectBufferData
{
	int raymarchShape;
	float3 position;
	float3 eulerAngles;
	float3 scale;
	float2 blank;
};

cbuffer ShaderBuffer : register(b0)
{
    float4x4 viewMatrix;
	float3 cameraPosition;
	float aspectRatio;
	float time;
	float3 blank0;
	float4 blank1;
	float4 blank2;
};

uniform RWStructuredBuffer<RaymarchGameObjectBufferData> raymarchObjects : register(u0);

struct VS_INPUT
{
	float4 Position : POSITION;
	float2 TexCoord : TEXCOORD;
	float2 vI : SV_VERTEXID;
};

struct PS_INPUT
{
	float4 Position : SV_POSITION;
	float2 TexCoord : TEXCOORD;
};