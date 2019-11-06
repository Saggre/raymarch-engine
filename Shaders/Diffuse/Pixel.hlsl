#include "Common.hlsl"

float4 main(PixelInputType input) : SV_Target
{
    float4 textureColor = tex.Sample(sam, input.TexCoord);

    textureColor.a = 1;
    return textureColor;
}