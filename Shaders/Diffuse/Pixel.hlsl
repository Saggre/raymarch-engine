#include "Common.hlsl"

Texture2D<float4> tex : register(t0);
SamplerState g_samLinear : register(s0);

struct PixelInputType
{
    float4 Position : SV_POSITION;
    float2 TexCoord : TEXCOORD0;
};

float4 PS(PixelInputType input) : SV_Target
{
	// Between -1 and 1
	//float2 screenPos = input.ScreenPos.xy / input.ScreenPos.w;

    float4 textureColor = tex.Sample(g_samLinear, input.TexCoord.xy);
	//textureColor.a = 1;

	// Position is fragment position on screen
	//if (screenPos.x > 0.5) {
		//return float4(1,0,0,1);
	//}'
	//return float4(1,0,0,1);
    textureColor.a = 1;
    return textureColor;

	//return saturate(textureColor + AmbientColor * AmbientIntensity);
}