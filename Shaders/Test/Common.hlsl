cbuffer MatrixBuffer : register(b0)
{
    float4x4 worldMatrix;
    float4x4 viewMatrix;
    float4x4 projectionMatrix;
};

struct VS_INPUT
{
    float4 Pos : POSITION;
    float2 TexCoord : TEXCOORD;
};

struct HS_INPUT
{
    float4 Pos : POSITION;
};

struct DS_INPUT
{
    float4 Pos : POSITION;
};

struct HS_CONSTANT_DATA
{
    float Edges[3] : SV_TessFactor;
    float Inside[1] : SV_InsideTessFactor;
};

struct GS_INPUT
{
    float4 Pos : SV_POSITION;
};

struct PS_INPUT
{
    float4 Pos : SV_POSITION;
    float3 Nrm : NORMAL;
};
