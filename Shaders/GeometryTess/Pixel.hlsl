struct PSInput
{
    float4 Position : SV_POSITION;
    float2 TexCoord : TEXCOORD0;
};

float4 main(PSInput pin) : SV_TARGET
{
    return float4(1, 0, 0, 1);
}
