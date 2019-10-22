cbuffer ShaderBuffer : register(b0)
{
	matrix worldMatrix;
	matrix viewMatrix;
	matrix projectionMatrix;
	float4x4 worldInverseTranspose;
};

Texture2D<float4> tex : register(t1);

float4 AmbientColor = float4(1, 1, 1, 1);
float AmbientIntensity = 0.1;

float3 DiffuseLightDirection = float3(1, 1, 1);
float4 DiffuseColor = float4(1, 1, 1, 1);
float DiffuseIntensity = 1.0;

struct VertexInputType
{
	float4 Position : POSITION;
	float4 Normal : NORMAL0;
	float4 TexCoord : TEXCOORD0;
};

struct PixelInputType
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float4 TexCoord : TEXCOORD0;
};

PixelInputType VS(VertexInputType input)
{
	PixelInputType output;
	float4 position = input.Position;

	float4 worldPosition = mul(position, worldMatrix);
	float4 viewPosition = mul(worldPosition, viewMatrix);
	output.Position = input.Position;// mul(viewPosition, projectionMatrix);

	float4 normal = mul(input.Normal, worldInverseTranspose);
	float lightIntensity = dot(normal, DiffuseLightDirection);

	output.Color = saturate(DiffuseColor * DiffuseIntensity * lightIntensity);
	output.TexCoord = input.TexCoord;

	return output;
}