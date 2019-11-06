#include "Common.hlsl"

HS_CONSTANT_DATA SampleHSFunction(InputPatch<HS_INPUT, 3> ip,
                                          uint PatchID : SV_PrimitiveID)
{
    HS_CONSTANT_DATA Output;

    /*float2 tessAmountRange = float2(1, 20);
    float2 tessDistanceRange = float2(0.5, 5);

    float3 triCenter = (ip[0].Position.xyz + ip[1].Position.xyz + ip[2].Position.xyz) / 3;
    float distanceToCamera = distance(triCenter, cameraPosition.xyz);

    float a = 1;

    if (distanceToCamera < tessDistanceRange.x)
    {
        a = tessAmountRange.y;
    }
    else if (distanceToCamera > tessDistanceRange.y)
    {
        a = tessAmountRange.x;
    }
    else
    {
        a = lerp(tessAmountRange.y, tessAmountRange.x, (distanceToCamera - tessDistanceRange.x) / (tessDistanceRange.y - tessDistanceRange.x));
    }

    //a = 30;*/

    float minLOD = 3;
    float maxLOD = 20;

    float minDistance = 0.5;
    float maxDistance = 5;

    float distanceRange = maxDistance - minDistance;
    float vertex0 = lerp(minLOD, maxLOD, (1.0f - (saturate((distance(cameraPosition, ip[0].Position) - minDistance) / distanceRange))));
    float vertex1 = lerp(minLOD, maxLOD, (1.0f - (saturate((distance(cameraPosition, ip[1].Position) - minDistance) / distanceRange))));
    float vertex2 = lerp(minLOD, maxLOD, (1.0f - (saturate((distance(cameraPosition, ip[2].Position) - minDistance) / distanceRange))));
    //float vertex3 = lerp(minLOD, maxLOD, (1.0f - (saturate((distance(cameraPosition, ip[3].Position) - minDistance) / distanceRange))));

    // Use the minimum value for each edge factor (pair of vertices)

    Output.Edges[0] = min(vertex1, vertex2);
    Output.Edges[1] = min(vertex2, vertex0);
    Output.Edges[2] = min(vertex0, vertex1);
    //Output.Edges[3] = min(vertex2, vertex3);

    // Use the overall minimum value for the inside tessellation factor

    float minTess = min(Output.Edges[0], min(Output.Edges[1], Output.Edges[2]));
    Output.Inside[0] = minTess;
    //Output.Inside[1] = minTess;

    //--

    /*float tessFactor = a;
    float TessAmount = a;

    Output.Edges[0] = Output.Edges[1] = Output.Edges[2] = tessFactor;
    Output.Inside[0] = TessAmount;*/

    return Output;
}

[domain("tri")]
[partitioning("fractional_odd")]
[outputtopology("triangle_cw")]
[outputcontrolpoints(3)]
[patchconstantfunc("SampleHSFunction")]
DS_INPUT main(InputPatch<HS_INPUT, 3> p,
                    uint i : SV_OutputControlPointID,
                    uint PatchID : SV_PrimitiveID)
{


    DS_INPUT Output = (DS_INPUT) 0;
    Output.Position = p[i].Position;
    Output.TexCoord = p[i].TexCoord;
    return Output;
}