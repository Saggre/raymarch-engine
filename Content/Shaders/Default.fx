//float4x4 World;
//float4x4 View;
//float4x4 Projection;

cbuffer ShaderBuffer : register(b0)
{
	matrix worldMatrix;
	matrix viewMatrix;
	matrix projectionMatrix;
	float tessellationAmount;
	float3 tessellationPadding;
	float4x4 worldInverseTranspose;
};

float4 AmbientColor = float4(1, 1, 1, 1);
float AmbientIntensity = 0.1;


float3 DiffuseLightDirection = float3(1, 1, 1);
float4 DiffuseColor = float4(1, 1, 1, 1);
float DiffuseIntensity = 1.0;

texture modelTexture;
sampler2D textureSampler = sampler_state {
	Texture = (modelTexture);
	MinFilter = Linear;
	MagFilter = Linear;
	AddressU = Clamp;
	AddressV = Clamp;
};

struct VertexShaderInput
{
	float4 Position : POSITION0;
	float4 Normal : NORMAL0;
	float2 TextureCoordinate : TEXCOORD0;
};

struct HullInputType
{
	float4 position : POSITION;
};

struct PixelInputType
{
	float4 position : SV_POSITION;
	//float4 color : COLOR;
};

/*struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 TextureCoordinate : TEXCOORD0;
	float4 ScreenPos : TEXCOORD1;
};*/

HullInputType VertexShaderFunction(VertexShaderInput input)
{
	HullInputType output;
	float4 position = input.Position;

	float4 worldPosition = mul(position, worldMatrix);
	float4 viewPosition = mul(worldPosition, viewMatrix);
	output.position = mul(viewPosition, projectionMatrix);

	float4 normal = mul(input.Normal, worldInverseTranspose);
	float lightIntensity = dot(normal, DiffuseLightDirection);

	//output.Color = saturate(DiffuseColor * DiffuseIntensity * lightIntensity);
	//output.TextureCoordinate = input.TextureCoordinate;

	return output;
}

float4 PixelShaderFunction(PixelInputType input) : COLOR0
{
	// Between -1 and 1
	//float2 screenPos = input.ScreenPos.xy / input.ScreenPos.w;

	//float4 textureColor = tex2D(textureSampler, input.TextureCoordinate);
	//textureColor.a = 1;

	// Position is fragment position on screen
	//if (screenPos.x > 0.5) {
		//return float4(1,0,0,1);
	//}'
	return float4(1,1,0,1);

	//return saturate(textureColor + AmbientColor * AmbientIntensity);
}

technique Diffuse
{
	pass Pass1
	{
		VertexShader = compile vs_5_0 VertexShaderFunction();
		PixelShader = compile ps_5_0 PixelShaderFunction();
	}
}