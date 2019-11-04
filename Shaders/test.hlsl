struct Vertex {
	float elevation;
	float moisture;
	float2 position;
};

struct Site {
	int terrainType;
	float2 position;
};

RWTexture2D<float4> Output : register(u0);
RWStructuredBuffer<Vertex> Vertices : register(u1);
RWStructuredBuffer<Site> Sites : register(u2);

[numthreads(32, 32, 1)]
void CS(uint3 id : SV_DispatchThreadID) {
	uint verticesLength[2];
	uint sitesLength[2];

	Vertices.GetDimensions(verticesLength[0], verticesLength[1]);
	Sites.GetDimensions(sitesLength[0], sitesLength[1]);

	Output[id.xy] = float4(1.0, 0.0, 0.0, 1.0);
}