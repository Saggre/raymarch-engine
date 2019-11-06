#include "Common.hlsl"

float heightmapAtPoint(float2 coord)
{
    float4 terrainMap = tex.SampleLevel(sam, coord, 0);
    float height = terrainMap.r * 0.5;

    return height;
}

[maxvertexcount(72)]
void main(triangle GS_INPUT input[3], inout TriangleStream<PS_INPUT> TriStream)
{
    float3 P1 = input[0].Position.xyz;
    float3 P2 = input[1].Position.xyz;
    float3 P3 = input[2].Position.xyz;
	
    float3 Nrm = normalize(cross(P2 - P1, P3 - P1));

    PS_INPUT A;
    A.Position = input[0].Position;
    A.TexCoord = input[0].TexCoord;
    A.Position.y += heightmapAtPoint(A.TexCoord);
    A.Nrm = Nrm;
	
    PS_INPUT B;
    B.Position = input[1].Position;
    B.TexCoord = input[1].TexCoord;
    B.Position.y += heightmapAtPoint(B.TexCoord);
    B.Nrm = Nrm;
	
    PS_INPUT C;
    C.Position = input[2].Position;
    C.TexCoord = input[2].TexCoord;
    C.Position.y += heightmapAtPoint(C.TexCoord);
    C.Nrm = Nrm;
	
    TriStream.Append(A);
    TriStream.Append(B);
    TriStream.Append(C);
    TriStream.RestartStrip();
}