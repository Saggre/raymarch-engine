#include "Common.hlsl"

HS_INPUT main(VS_INPUT input)
{
    HS_INPUT output = (HS_INPUT) 0;
    output.Pos = mul(input.Pos, worldMatrix);
    return output;
}