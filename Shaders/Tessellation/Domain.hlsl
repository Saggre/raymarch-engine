#include "Common.hlsl"

[domain("tri")]
GS_INPUT main(HS_CONSTANT_DATA input,
                    float3 UV : SV_DomainLocation,
                    const OutputPatch<DS_INPUT, 3> TrianglePatch)
{
    GS_INPUT outP = (GS_INPUT) 0;
    
    outP.Pos = UV.x * TrianglePatch[0].Pos + UV.y * TrianglePatch[1].Pos + UV.z * TrianglePatch[2].Pos;
    outP.Pos /= outP.Pos.w;
	
    //outP.Pos.y = 2.0F * sin(outP.Pos.x / 4.0F) * cos(outP.Pos.z / 4.0F);
		
    outP.Pos = mul(outP.Pos, viewMatrix);
    outP.Pos = mul(outP.Pos, projectionMatrix);
    
    return outP;
}