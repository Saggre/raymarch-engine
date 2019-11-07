#include "../Noise/Simplex.hlsl"

RWTexture2D<float4> Result : register(u0);

cbuffer ShaderBuffer : register(b0)
{
    int resolution;
    int offsetX;
    int offsetY;
};

float fbm(float2 pos, uint octaves)
{
    float s = 0.0;
    float m = 0.0;
    float a = 0.5;
    float2 p = pos;
	
    for (int i = 0; i < octaves; i++)
    {
        s += a * snoise(pos);
        m += a;
        a *= 0.5;
        p *= 2.0;
    }

    return s / m;
}

[numthreads(1, 1, 1)]
void main(uint3 id : SV_DispatchThreadID)
{
    Result[id.xy] = float4(fbm((id.xy + float2(offsetX, offsetY) * (resolution - 1)) / resolution, 4), 0, 0, 1);
    //Result[id.xy] = float4(abs(id.x - (resolution - 1) / 2.0) * 1.0 / (resolution), 0, 0, 1);
}
