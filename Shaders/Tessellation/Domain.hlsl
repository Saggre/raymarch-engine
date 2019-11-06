#include "Common.hlsl"

[domain("tri")]
GS_INPUT main(HS_CONSTANT_DATA input,
                    float3 UV : SV_DomainLocation,
                    const OutputPatch<DS_INPUT, 3> TrianglePatch)
{
    GS_INPUT outP = (GS_INPUT) 0;
    
    outP.TexCoord = UV.x * TrianglePatch[0].TexCoord + UV.y * TrianglePatch[1].TexCoord + UV.z * TrianglePatch[2].TexCoord;

    outP.Position = UV.x * TrianglePatch[0].Position + UV.y * TrianglePatch[1].Position + UV.z * TrianglePatch[2].Position;
    outP.Position /= outP.Position.w;
		
    outP.Position = mul(outP.Position, viewMatrix);
    outP.Position = mul(outP.Position, projectionMatrix);
    
    return outP;
}