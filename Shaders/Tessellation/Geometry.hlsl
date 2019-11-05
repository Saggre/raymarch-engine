#include "Common.hlsl"

[maxvertexcount(72)]
void main(triangle GS_INPUT input[3], inout TriangleStream<PS_INPUT> TriStream)
{
    float3 P1 = input[0].Pos.xyz;
    float3 P2 = input[1].Pos.xyz;
    float3 P3 = input[2].Pos.xyz;
	
    float3 Nrm = normalize(cross(P2 - P1, P3 - P1));
	
    PS_INPUT A;
    A.Pos = input[0].Pos;
    A.Nrm = Nrm;
	
    PS_INPUT B;
    B.Pos = input[1].Pos;
    B.Nrm = Nrm;
	
    PS_INPUT C;
    C.Pos = input[2].Pos;
    C.Nrm = Nrm;
	
    TriStream.Append(A);
    TriStream.Append(B);
    TriStream.Append(C);
    TriStream.RestartStrip();
}