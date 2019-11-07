#include "../Noise/ClassicPerlinNoise.hlsl"

RWTexture2D<float4> Result : register(u0);

cbuffer ShaderBuffer : register(b0)
{
    int resolution;
    int offsetX;
    int offsetY;
};

float turbulence(float2 pos, uint octaves, float amplitude)
{
    float value;
    float2 st = pos;
    for (int i = 0; i < octaves; i++)
    {
        value += amplitude * abs(ClassicPerlinNoise(st));
        st *= 2.;
        amplitude *= .5;
    }
    return value;
}

float fbm(float2 pos, uint octaves)
{
    float r = 0;
	
    for (int i = 0; i < octaves; i++)
    {
        r += ClassicPerlinNoise(pos * (i + 1));
    }

    return r / octaves;
}

[numthreads(1, 1, 1)]
void main(uint3 id : SV_DispatchThreadID)
{
    // TODO
    float2 offsetPixels = float2(offsetX, offsetY) * (resolution - 1);

    float baseNoise = turbulence((id.xy + offsetPixels) / resolution, 2, 0.9);
    float surfaceNoise = ClassicPerlinNoise(id.xy * 0.1);
    Result[id.xy] = float4(1 - abs(baseNoise), 0, 0, 1);
    //Result[id.xy] = float4(abs(id.x - (resolution - 1) / 2.0) * 1.0 / (resolution), 0, 0, 1);
}
