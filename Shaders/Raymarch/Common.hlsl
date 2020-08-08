struct RaymarchGameObjectBufferData
{
	int raymarchShape;
	float3 position;
	float3 eulerAngles;
	float3 scale;
	float2 blank;
};

cbuffer MatrixBuffer : register(b0)
{
	float4x4 worldMatrix;
	float4x4 viewMatrix;
	float4x4 projectionMatrix;
};

cbuffer ShaderBuffer : register(b1)
{
	float3 cameraPosition;
	float aspectRatio;
	float3 cameraDirection;
	float time;
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