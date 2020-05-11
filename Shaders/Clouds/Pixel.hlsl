#include "Common.hlsl"

float cnoise(in float3 x)
{
    return noise(x);
    /*float3 p = floor(x);
    float3 f = frac(x);
    f = f * f * (3.0 - 2.0 * f);
    
    float2 uv = (p.xy + float2(37.0, 17.0) * p.z) + f.xy;
    float2 rg = textureLod(iChannel0, (uv + 0.5) / 256.0, 0.).yx;

    return -1.0 + 2.0 * lerp(rg.x, rg.y, f.z);*/
}

float map5(in float3 p)
{
    float3 q = p - float3(0.0, 0.1, 1.0) * iTime;
    float f;
    f = 0.50000 * cnoise(q);
    q = q * 2.02;
    f += 0.25000 * cnoise(q);
    q = q * 2.03;
    f += 0.12500 * cnoise(q);
    q = q * 2.01;
    f += 0.06250 * cnoise(q);
    q = q * 2.02;
    f += 0.03125 * cnoise(q);
    return clamp(1.5 - p.y - 2.0 + 1.75 * f, 0.0, 1.0);
}

float map4(in float3 p)
{
    float3 q = p - float3(0.0, 0.1, 1.0) * iTime;
    float f;
    f = 0.50000 * cnoise(q);
    q = q * 2.02;
    f += 0.25000 * cnoise(q);
    q = q * 2.03;
    f += 0.12500 * cnoise(q);
    q = q * 2.01;
    f += 0.06250 * cnoise(q);
    return clamp(1.5 - p.y - 2.0 + 1.75 * f, 0.0, 1.0);
}
float map3(in float3 p)
{
    float3 q = p - float3(0.0, 0.1, 1.0) * iTime;
    float f;
    f = 0.50000 * cnoise(q);
    q = q * 2.02;
    f += 0.25000 * cnoise(q);
    q = q * 2.03;
    f += 0.12500 * cnoise(q);
    return clamp(1.5 - p.y - 2.0 + 1.75 * f, 0.0, 1.0);
}
float map2(in float3 p)
{
    float3 q = p - float3(0.0, 0.1, 1.0) * iTime;
    float f;
    f = 0.50000 * cnoise(q);
    q = q * 2.02;
    f += 0.25000 * cnoise(q);;
    return clamp(1.5 - p.y - 2.0 + 1.75 * f, 0.0, 1.0);
}

float3 sundir = normalize(float3(-1.0, 0.0, -1.0));

float4 integrate(in float4 sum, in float dif, in float den, in float3 bgcol, in float t)
{
    // lighting
    float3 lin = float3(0.65, 0.7, 0.75) * 1.4 + float3(1.0, 0.6, 0.3) * dif;
    float4 col = float4(lerp(float3(1.0, 0.95, 0.8), float3(0.25, 0.3, 0.35), den), den);
    col.xyz *= lin;
    col.xyz = lerp(col.xyz, bgcol, 1.0 - exp(-0.003 * t * t));
    // front to back blending    
    col.a *= 0.4;
    col.rgb *= col.a;
    return sum + col * (1.0 - sum.a);
}

float mapChooser(uint MAPLOD, float3 input)
{
    float val = 0;
    if (MAPLOD == 2)
    {
        val = map2(input);
    }
    else if (MAPLOD == 3)
    {
        val = map3(input);
    }
    else if (MAPLOD == 4)
    {
        val = map4(input);
    }
    else if (MAPLOD == 5)
    {
        val = map5(input);
    }
}

void MARCH(uint STEPS, uint MAPLOD, inout float t, inout float4 sum, inout float3 bgcol)
{
    for (int i = 0; i < STEPS; i++)
    {
        float3 pos = ro + t * rd;
        if (pos.y < -3.0 || pos.y > 2.0 || sum.a > 0.99)
            break;
        float den = mapChooser(MAPLOD, pos);

        if (den > 0.01)
        {
            float dif = clamp((den - mapChooser(MAPLOD, pos + 0.3 * sundir)) / 0.6, 0.0, 1.0);
            sum = integrate(sum, dif, den, bgcol, t);
        }
        t += max(0.05, 0.02 * t);
    }
}

float4 raymarch(in float3 ro, in float3 rd, in float3 bgcol, in int2 px)
{
    float4 sum = float4(0.0);

    float t = 0.0; //0.05*texelFetch( iChannel0, px&255, 0 ).x;

    // TODO fix maybe
    MARCH(30, 5, t, sum, bgcol);
    MARCH(30, 4, t, sum, bgcol);
    MARCH(30, 3, t, sum, bgcol);
    MARCH(30, 2, t, sum, bgcol);

    return clamp(sum, 0.0, 1.0);
}

float3x3 setCamera(in float3 ro, in float3 ta, float cr)
{
    float3 cw = normalize(ta - ro);
    float3 cp = float3(sin(cr), cos(cr), 0.0);
    float3 cu = normalize(cross(cw, cp));
    float3 cv = normalize(cross(cu, cw));
    return float3x3(cu, cv, cw);
}

float4 render(in float3 ro, in float3 rd, in int2 px)
{
    // background sky     
    float sun = clamp(dot(sundir, rd), 0.0, 1.0);
    float3 col = float3(0.6, 0.71, 0.75) - rd.y * 0.2 * float3(1.0, 0.5, 1.0) + 0.15 * 0.5;
    col += 0.2 * float3(1.0, .6, 0.1) * pow(sun, 8.0);

    // clouds    
    float4 res = raymarch(ro, rd, col, px);
    col = col * (1.0 - res.w) + res.xyz;
    
    // sun glare    
    col += 0.2 * float3(1.0, 0.4, 0.2) * pow(sun, 3.0);

    return float4(col, 1.0);
}

float4 main(PixelInputType input) : SV_Target
{
    float4 fragColor = float4(1, 1, 1, 1);
    float2 fragCoord = input.TexCoord;

    float2 p = (-iResolution.xy + 2.0 * fragCoord.xy) / iResolution.y;

    float2 m = float2(1, 1); // Mouse pos?
    
    // camera
    float3 ro = 4.0 * normalize(float3(sin(3.0 * m.x), 0.4 * m.y, cos(3.0 * m.x)));
    //float3 ta = float3(0.0, -1.0, 0.0);
    //float3x3 ca = setCamera(ro, ta, 0.0);
    // ray
    float3 rd = normalize(float3(p.xy, 1.5));
    
    fragColor = render(ro, rd, int2(fragCoord - 0.5));
    return fragColor;
}

void mainVR(out float4 fragColor, in float2 fragCoord, in float3 fragRayOri, in float3 fragRayDir)
{
    fragColor = render(fragRayOri, fragRayDir, int2(fragCoord - 0.5));
}