struct VertexInputType
{
    float4 Position : POSITION;
    float2 TexCoord : TEXCOORD;
};

struct PixelInputType
{
    float4 Position : SV_POSITION;
    float2 TexCoord : TEXCOORD;
};

cbuffer ShaderBuffer : register(b0)
{
    float4x4 worldMatrix;
    float4x4 viewMatrix;
    float4x4 projectionMatrix;
};

Texture2D<float4> tex : register(t0);

SamplerState g_samLinear : register(s0);