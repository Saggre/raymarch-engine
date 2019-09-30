RWTexture2D<float4> Output : register(u0);
RWStructuredBuffer<float> Test : register(u1);

[numthreads(32, 32, 1)]
void CS(uint3 id : SV_DispatchThreadID) {
	Output[id.xy] = float4(Test[0], 0.0, 0.0, 1.0);
}