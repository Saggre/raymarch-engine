#include "Common.hlsl"

float4 main(PS_INPUT input) : SV_Target
{
    float4 heightmap = tex.Sample(sam, input.TexCoord);

    float4 color = float4(heightmap.rrr, 1);

    float4 fogColor = float4(0, 0, 0, 1);

    // TODO fog
    return lerp(color, fogColor, saturate(distance(cameraPosition, float4(0, 0, 0, 0)) * 0.1));
}