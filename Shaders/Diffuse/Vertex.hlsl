#include "Common.hlsl"

Texture2D<float4> tex : register(t1);

float4 AmbientColor = float4(1, 1, 1, 1);
float AmbientIntensity = 0.1;

float3 DiffuseLightDirection = float3(1, 1, 1);
float4 DiffuseColor = float4(1, 1, 1, 1);
float DiffuseIntensity = 1.0;

struct VertexInputType
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
};

struct PixelInputType
{
    float4 Position : SV_POSITION;
    float2 TexCoord : TEXCOORD0;
};

PixelInputType VS(VertexInputType input)
{
    PixelInputType output;

    float4 worldPosition = mul(input.Position, worldMatrix);
    float4 viewPosition = mul(worldPosition, viewMatrix);
    float4 projectionPosition = mul(viewPosition, projectionMatrix);

    output.Position = projectionPosition;
	//float4 normal = mul(input.Normal, worldInverseTranspose);
	//float lightIntensity = dot(normal, DiffuseLightDirection);
	// TODO projection matrix
	//output.Position = mul(mul(input.Position, worldMatrix), viewMatrix);
    output.TexCoord = input.TexCoord;

    return output;
}