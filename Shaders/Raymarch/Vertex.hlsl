#include "Common.hlsl"

PS_INPUT main(VS_INPUT input)
{
    PS_INPUT output = (PS_INPUT) 0;

    output.Position = mul(input.Position, worldMatrix);
    output.Position = mul(output.Position, viewMatrix);
    output.Position = mul(output.Position, projectionMatrix);

    output.TexCoord = input.TexCoord;

    return output;
}