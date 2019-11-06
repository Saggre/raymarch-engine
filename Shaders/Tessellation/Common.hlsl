cbuffer MatrixBuffer : register(b0)
{
    float4x4 worldMatrix;
    float4x4 viewMatrix;
    float4x4 projectionMatrix;
};

cbuffer ShaderBuffer : register(b1)
{
    float4 cameraPosition;
};

Texture2D<float4> tex : register(t0);

SamplerState sam : register(s0);

struct VS_INPUT
{
    float4 Position : POSITION;
    float2 TexCoord : TEXCOORD;
};

struct HS_INPUT
{
    float4 Position : POSITION;
    float2 TexCoord : TEXCOORD;
};

struct DS_INPUT
{
    float4 Position : POSITION;
    float2 TexCoord : TEXCOORD;
};

struct HS_CONSTANT_DATA
{
    float Edges[3] : SV_TessFactor;
    float Inside[1] : SV_InsideTessFactor;
};

struct GS_INPUT
{
    float4 Position : SV_POSITION;
    float2 TexCoord : TEXCOORD;
};

struct PS_INPUT
{
    float4 Position : SV_POSITION;
    float2 TexCoord : TEXCOORD;
    float3 Nrm : NORMAL;
};
