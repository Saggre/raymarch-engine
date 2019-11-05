#include "Common.hlsl"

HS_CONSTANT_DATA SampleHSFunction(InputPatch<HS_INPUT, 3> ip,
                                          uint PatchID : SV_PrimitiveID)
{
    HS_CONSTANT_DATA Output;
    float tessFactor = 10; //factor.x;

    float TessAmount = 10; //factor.y;

    Output.Edges[0] = Output.Edges[1] = Output.Edges[2] = tessFactor;
    Output.Inside[0] = TessAmount;

    return Output;
}

[domain("tri")]
[partitioning("pow2")]
[outputtopology("triangle_cw")]
[outputcontrolpoints(3)]
[patchconstantfunc("SampleHSFunction")]
DS_INPUT main(InputPatch<HS_INPUT, 3> p,
                    uint i : SV_OutputControlPointID,
                    uint PatchID : SV_PrimitiveID)
{


    DS_INPUT Output = (DS_INPUT) 0;
    Output.Pos = p[i].Pos;
    return Output;
}