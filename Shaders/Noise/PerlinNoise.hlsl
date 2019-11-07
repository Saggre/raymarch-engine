// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

#ifndef PERLIN_NOISE_INCLUDED
#define PERLIN_NOISE_INCLUDED

#define M_PI 3.14159265358979323846

inline float PerlinRand(float2 c)
{
    return frac(sin(dot(c.xy, float2(12.9898, 78.233))) * 43758.5453);
}

inline float PerlinRand(float2 co, float l)
{
    return PerlinRand(float2(PerlinRand(co), l));
}

inline float PerlinRand(float2 co, float l, float t)
{
    return PerlinRand(float2(PerlinRand(co, l), t));
}

float PerlinNoise(float2 p, float freq)
{
    float unit = screenWidth / freq;
    float2 ij = floor(p / unit);
    float2 xy = fmod(p, unit) / unit;
	//xy = 3.*xy*xy-2.*xy*xy*xy;
    xy = .5 * (1. - cos(PI * xy));
    float a = PerlinRand((ij + float2(0., 0.)));
    float b = PerlinRand((ij + float2(1., 0.)));
    float c = PerlinRand((ij + float2(0., 1.)));
    float d = PerlinRand((ij + float2(1., 1.)));
    float x1 = lerp(a, b, xy.x);
    float x2 = lerp(c, d, xy.x);
    return lerp(x1, x2, xy.y);
}

float PerlinNoise(float2 p, int res)
{
    float persistance = .5;
    float n = 0.;
    float normK = 0.;
    float f = 4.;
    float amp = 1.;
    int iCount = 0;
    for (int i = 0; i < 50; i++)
    {
        n += amp * PerlinNoise(p, f);
        f *= 2.;
        normK += amp;
        amp *= persistance;
        if (iCount == res)
            break;
        iCount++;
    }
    float nf = n / normK;
    return nf * nf * nf * nf;
}

float Perlin(float2 p, float dim, float time)
{
    float2 pos = floor(p * dim);
    float2 posx = pos + float2(1.0, 0.0);
    float2 posy = pos + float2(0.0, 1.0);
    float2 posxy = pos + float2(1.0);

    float c = PerlinRand(pos, dim, time);
    float cx = PerlinRand(posx, dim, time);
    float cy = PerlinRand(posy, dim, time);
    float cxy = PerlinRand(posxy, dim, time);

    float2 d = frac(p * dim);
    d = -0.5 * cos(d * M_PI) + 0.5;

    float ccx = lerp(c, cx, d.x);
    float cycxy = lerp(cy, cxy, d.x);
    float center = lerp(ccx, cycxy, d.y);

    return center * 2.0 - 1.0;
}

// p must be normalized!
float Perlin(float2 p, float dim)
{
    return perlin(p, dim, 0.0);
}


#endif //PERLIN_NOISE_INCLUDED