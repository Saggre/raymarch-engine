#include "Common.hlsl"

float4 main(PixelInputType input) : SV_Target
{
    float4 textureColor = tex.Sample(g_samLinear, input.TexCoord.xy);

    textureColor.a = 1;
    return textureColor;
}