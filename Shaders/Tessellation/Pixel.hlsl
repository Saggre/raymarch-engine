#include "Common.hlsl"

float4 main(PS_INPUT input) : SV_Target
{
    return tex.Sample(sam, input.TexCoord);
    return float4(1, 0, 0, 1);
}