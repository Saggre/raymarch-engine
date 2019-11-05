#include "Common.hlsl"

PixelInputType main(VertexInputType input)
{
    PixelInputType output = (PixelInputType) 0;

    output.Position = mul(input.Position, worldMatrix);
    output.Position = mul(output.Position, viewMatrix);
    output.Position = mul(output.Position, projectionMatrix);

    output.TexCoord = input.TexCoord;

    return output;
}