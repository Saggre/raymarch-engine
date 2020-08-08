#include "Common.hlsl"

// Creates a full-screen quad from any 4 vertices
PS_INPUT main(uint vI : SV_VERTEXID)
{
	PS_INPUT output = (PS_INPUT)0;

	// This vertex shader generates a trianglestrip like this:
	//  1--2
	//    /
	//   /
	//  3--4
	output.TexCoord = float2(vI % 2, vI % 4 / 2);

	output.Position = float4(output.TexCoord * 2 - 1, 0, 1);

	return output;
}