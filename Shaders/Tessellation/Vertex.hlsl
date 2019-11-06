#include "Common.hlsl"

HS_INPUT main(VS_INPUT input)
{
    HS_INPUT output = (HS_INPUT) 0;
    output.Position = mul(input.Position, worldMatrix);
    output.TexCoord = input.TexCoord;
    return output;
}