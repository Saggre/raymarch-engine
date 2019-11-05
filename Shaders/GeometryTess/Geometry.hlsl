struct VSInput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
};

struct PSInput
{
    float4 Position : SV_POSITION;
    float2 TexCoord : TEXCOORD0;
};

// required for array
#define MAX_ITERATIONS 5

void DrawTriangle(float4 p0, float4 p1, float4 p2, inout TriangleStream<PSInput> stream)
{
    PSInput v0 = (PSInput) 0;
    v0.Position = p0;
    stream.Append(v0);

    PSInput v1 = (PSInput) 0;
    v1.Position = p1;
    stream.Append(v1);

    PSInput v2 = (PSInput) 0;
    v2.Position = p2;
    stream.Append(v2);

    stream.RestartStrip();
}

[maxvertexcount(128)] // directx rule: maxvertexcount * sizeof(VS_OUT) <= 1024
void main(triangle PSInput input[3], inout TriangleStream<PSInput> stream)
{
    int itc = 3; //min(tess, MAX_ITERATIONS);
    float fitc = itc;
    float4 past_pos[MAX_ITERATIONS];
    float4 array_pass[MAX_ITERATIONS];
    for (int pi = 0; pi < MAX_ITERATIONS; pi++)
    {
        past_pos[pi] = float4(0, 0, 0, 0);
        array_pass[pi] = float4(0, 0, 0, 0);
    }
    // -------------------------------------
    // Tessellation kernel for the control points
    for (int x = 0; x <= itc; x++)
    {
        float4 last;
        for (int y = 0; y <= x; y++)
        {
            float2 seg = float2(x / fitc, y / fitc);
            float3 uv;
            uv.x = 1 - seg.x;
            uv.z = seg.y;
            uv.y = 1 - (uv.x + uv.z);

            // ---------------------------------------
            // Domain Stage
            // uv           Domain Location
            // x,y          IterationIndex

            float4 fpos = input[0].Position * uv.x;
            fpos += input[1].Position * uv.y;
            fpos += input[2].Position * uv.z;

            if (x > 0 && y > 0)
            {
                DrawTriangle(past_pos[y - 1], last, fpos, stream);
                if (y < x)
                {
                    // add adjacent triangle
                    DrawTriangle(past_pos[y - 1], fpos, past_pos[y], stream);
                }
            }
            array_pass[y] = fpos;
            last = fpos;
        }
        for (int i = 0; i < MAX_ITERATIONS; i++)
        {
            past_pos[i] = array_pass[i];
        }
    }
}