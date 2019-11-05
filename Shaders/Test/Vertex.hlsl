#include "Common.hlsl"

PS_INPUT main(VS_INPUT input)
{
    PS_INPUT output = (PS_INPUT) 0;
    output.Pos = mul(input.Pos, worldMatrix);
    output.Pos = mul(output.Pos, viewMatrix);
    output.Pos = mul(output.Pos, projectionMatrix);
    return output;
}