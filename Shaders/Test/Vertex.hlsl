#include "Common.hlsl"

float4 main(float4 position : POSITION) : SV_POSITION
{
	float4 output = position;

	output = mul(output, worldMatrix);
	output = mul(output, viewMatrix);
	output = mul(output, projectionMatrix);

   return output;
}