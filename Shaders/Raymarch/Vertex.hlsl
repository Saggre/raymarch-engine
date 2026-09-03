#include "Common.hlsl"

// Creates a full-screen quad from any 4 vertices
PS_INPUT main(VS_INPUT input)
{
    PS_INPUT output = (PS_INPUT)0;

    // Triangle list with indices 0,2,1,2,3,1. Under DrawIndexed SV_VertexID is the index value,
    // not a counter, so vI is 0..3 and the two triangles cover the whole [0,1] uv square:
    //  2--3
    //  |\ |
    //  | \|
    //  0--1
    // The vertex buffer's own Position and TexCoord are never read.
    output.TexCoord = float2(input.vI % 2, input.vI % 4 / 2);

    // The * 2000 scales x, y and w alike, so it cancels out in the perspective divide.
    output.Position = float4(output.TexCoord * 2 - 1, 0, 1) * 2000;

    return output;
}
