// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

#ifndef CLASSIC_PERLIN_NOISE_INCLUDED
#define CLASSIC_PERLIN_NOISE_INCLUDED


inline float2 _ClassicPerlinFade(float2 t)
{
    return t * t * t * (t * (t * 6.0 - 15.0) + 10.0);
}

inline float3 _ClassicPerlinFade(float3 t)
{
    return t * t * t * (t * (t * 6.0 - 15.0) + 10.0);
}

inline float4 _ClassicPerlinPermute(float4 x)
{
    return fmod(((x * 34.0) + 1.0) * x, 289.0);
}

inline float4 _ClassicPerlinTaylorInvSqrt(float4 r)
{
    return 1.79284291400159 - 0.85373472095314 * r;
}

//	Classic Perlin 2D Noise 
//	by Stefan Gustavson
//
float ClassicPerlinNoise(float2 P)
{
    float4 Pi = floor(P.xyxy) + float4(0.0, 0.0, 1.0, 1.0);
    float4 Pf = frac(P.xyxy) - float4(0.0, 0.0, 1.0, 1.0);
    Pi = fmod(Pi, 289.0); // To avoid truncation effects in permutation
    float4 ix = Pi.xzxz;
    float4 iy = Pi.yyww;
    float4 fx = Pf.xzxz;
    float4 fy = Pf.yyww;
    float4 i = _ClassicPerlinPermute(_ClassicPerlinPermute(ix) + iy);
    float4 gx = 2.0 * frac(i * 0.0243902439) - 1.0; // 1/41 = 0.024...
    float4 gy = abs(gx) - 0.5;
    float4 tx = floor(gx + 0.5);
    gx = gx - tx;
    float2 g00 = float2(gx.x, gy.x);
    float2 g10 = float2(gx.y, gy.y);
    float2 g01 = float2(gx.z, gy.z);
    float2 g11 = float2(gx.w, gy.w);
    float4 norm = 1.79284291400159 - 0.85373472095314 *
		float4(dot(g00, g00), dot(g01, g01), dot(g10, g10), dot(g11, g11));
    g00 *= norm.x;
    g01 *= norm.y;
    g10 *= norm.z;
    g11 *= norm.w;
    float n00 = dot(g00, float2(fx.x, fy.x));
    float n10 = dot(g10, float2(fx.y, fy.y));
    float n01 = dot(g01, float2(fx.z, fy.z));
    float n11 = dot(g11, float2(fx.w, fy.w));
    float2 fade_xy = _ClassicPerlinFade(Pf.xy);
    float2 n_x = lerp(float2(n00, n01), float2(n10, n11), fade_xy.x);
    float n_xy = lerp(n_x.x, n_x.y, fade_xy.y);
    return 2.3 * n_xy;
}

//	Classic Perlin 3D Noise 
//	by Stefan Gustavson
//
float ClassicPerlinNoise(float3 P)
{
    float3 Pi0 = floor(P); // Integer part for indexing
    float3 Pi1 = Pi0 + float3(1.0, 1.0, 1.0); // Integer part + 1
    Pi0 = fmod(Pi0, 289.0);
    Pi1 = fmod(Pi1, 289.0);
    float3 Pf0 = frac(P); // Fractional part for interpolation
    float3 Pf1 = Pf0 - float3(1.0, 1.0, 1.0); // Fractional part - 1.0
    float4 ix = float4(Pi0.x, Pi1.x, Pi0.x, Pi1.x);
    float4 iy = float4(Pi0.yy, Pi1.yy);
    float4 iz0 = Pi0.zzzz;
    float4 iz1 = Pi1.zzzz;

    float4 ixy = _ClassicPerlinPermute(_ClassicPerlinPermute(ix) + iy);
    float4 ixy0 = _ClassicPerlinPermute(ixy + iz0);
    float4 ixy1 = _ClassicPerlinPermute(ixy + iz1);

    float4 gx0 = ixy0 / 7.0;
    float4 gy0 = frac(floor(gx0) / 7.0) - 0.5;
    gx0 = frac(gx0);
    float4 gz0 = float4(0.5, 0.5, 0.5, 0.5) - abs(gx0) - abs(gy0);
    float4 sz0 = step(gz0, float4(0.0, 0.0, 0.0, 0.0));
    gx0 -= sz0 * (step(0.0, gx0) - 0.5);
    gy0 -= sz0 * (step(0.0, gy0) - 0.5);

    float4 gx1 = ixy1 / 7.0;
    float4 gy1 = frac(floor(gx1) / 7.0) - 0.5;
    gx1 = frac(gx1);
    float4 gz1 = float4(0.5, 0.5, 0.5, 0.5) - abs(gx1) - abs(gy1);
    float4 sz1 = step(gz1, float4(0.0, 0.0, 0.0, 0.0));
    gx1 -= sz1 * (step(0.0, gx1) - 0.5);
    gy1 -= sz1 * (step(0.0, gy1) - 0.5);

    float3 g000 = float3(gx0.x, gy0.x, gz0.x);
    float3 g100 = float3(gx0.y, gy0.y, gz0.y);
    float3 g010 = float3(gx0.z, gy0.z, gz0.z);
    float3 g110 = float3(gx0.w, gy0.w, gz0.w);
    float3 g001 = float3(gx1.x, gy1.x, gz1.x);
    float3 g101 = float3(gx1.y, gy1.y, gz1.y);
    float3 g011 = float3(gx1.z, gy1.z, gz1.z);
    float3 g111 = float3(gx1.w, gy1.w, gz1.w);

    float4 norm0 = _ClassicPerlinTaylorInvSqrt(float4(dot(g000, g000), dot(g010, g010), dot(g100, g100), dot(g110, g110)));
    g000 *= norm0.x;
    g010 *= norm0.y;
    g100 *= norm0.z;
    g110 *= norm0.w;
    float4 norm1 = _ClassicPerlinTaylorInvSqrt(float4(dot(g001, g001), dot(g011, g011), dot(g101, g101), dot(g111, g111)));
    g001 *= norm1.x;
    g011 *= norm1.y;
    g101 *= norm1.z;
    g111 *= norm1.w;

    float n000 = dot(g000, Pf0);
    float n100 = dot(g100, float3(Pf1.x, Pf0.yz));
    float n010 = dot(g010, float3(Pf0.x, Pf1.y, Pf0.z));
    float n110 = dot(g110, float3(Pf1.xy, Pf0.z));
    float n001 = dot(g001, float3(Pf0.xy, Pf1.z));
    float n101 = dot(g101, float3(Pf1.x, Pf0.y, Pf1.z));
    float n011 = dot(g011, float3(Pf0.x, Pf1.yz));
    float n111 = dot(g111, Pf1);

    float3 fade_xyz = _ClassicPerlinFade(Pf0);
    float4 n_z = lerp(float4(n000, n100, n010, n110), float4(n001, n101, n011, n111), fade_xyz.z);
    float2 n_yz = lerp(n_z.xy, n_z.zw, fade_xyz.y);
    float n_xyz = lerp(n_yz.x, n_yz.y, fade_xyz.x);
    return 2.2 * n_xyz;
}

#endif //CLASSIC_PERLIN_NOISE_INCLUDED