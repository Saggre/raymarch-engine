static const float normalizer = 1.0 / 1024;

static const int elevationIndex = 0;
static const int moistureIndex = 1;
static const int depthIndex = 2;

// 16 bytes
struct Vertex {
	float elevation;
	float moisture;
	float2 position;
};

// 12 bytes
struct Site {
	int terrainType;
	float2 position;
};

// Create a RenderTexture with enableRandomWrite flag and set it
// with cs.SetTexture
RWTexture2D<float4> Result : register(u0);
RWStructuredBuffer<Vertex> Vertices : register(u1);
RWStructuredBuffer<Site> Sites : register(u2);

Site getClosestSiteOfType(in float2 startPoint, out float dist, in int terrainType, in uint sitesLength)
{
	Site closestSite;
	dist = 0.0;

	for (uint i = 0; i < sitesLength; i++)
	{
		if (Sites[i].terrainType != terrainType) {
			continue;
		}

		float d = distance(Sites[i].position, startPoint);
		if (i == 0 || d < dist)
		{
			dist = d;
			closestSite = Sites[i];
		}
	}

	return closestSite;
}

static const float coastFreshWaterPercentage = 0.5f;
static const float lakeFreshWaterPercentage = 1.0f;
void getVertexElevationAndMoisture(in Vertex vertex, in uint sitesLength, out float elevation, out float moisture) {

	// Set vertex elevations as distance from coast
	// Set moisture as distance from fresh water

	float distanceFromCoastline;
	float distanceFromLake;
	getClosestSiteOfType(vertex.position, distanceFromCoastline, 203,sitesLength);
	getClosestSiteOfType(vertex.position, distanceFromLake, 203,sitesLength);

	// TODO clamp max distance to map size or something
	float distanceFromFreshwater = min(distanceFromLake * lakeFreshWaterPercentage, distanceFromCoastline * coastFreshWaterPercentage);

	moisture = pow(0.985, distanceFromFreshwater);
	elevation = pow(0.985, distanceFromCoastline);
}

[numthreads(32, 32, 1)]
void ComputeTerrain(uint3 id : SV_DispatchThreadID)
{
	uint verticesLength[2];
	uint sitesLength[2];

	// Get data-array dimensions
	Vertices.GetDimensions(verticesLength[0], verticesLength[1]);
	Sites.GetDimensions(sitesLength[0], sitesLength[1]);
	float4 result = float4(0, 0, 0, 0);

	Vertex closestVertex;
	float closestVertexDistance = 0.0;
	for (uint i = 0; i < verticesLength[0]; i++) {
		float dist = distance(Vertices[i].position, id.xy * normalizer);
		if (i == 0 || dist < closestVertexDistance) {
			closestVertexDistance = dist;
			closestVertex = Vertices[i];
		}
	}

	if (closestVertex.elevation > 0.5) {
		// Transform [0,1] back into [-1,1]
		result[elevationIndex] = closestVertex.elevation * 2 - 1;
	}
	else {
		// Transform [0,1] back into [-1,1] and invert
		result[depthIndex] = -(closestVertex.elevation * 2 - 1);
	}
	result[moistureIndex] = closestVertex.moisture;

	Result[id.xy] = result;
}
