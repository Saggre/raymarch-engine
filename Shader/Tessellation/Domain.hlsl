// Called once per tessellated vertex
[domain("tri")] // indicates that triangle patches were used
// The original patch is passed in, along with the vertex position in barycentric
coordinates, and the patch constant phase hull shader output(tessellation factors)
DS_VS_OUTPUT_PS_INPUT DS(HS_CONSTANT_DATA_OUTPUT input,
	float3 BarycentricCoordinates : SV_DomainLocation,
	const OutputPatch<HS_CONTROL_POINT_OUTPUT, 3>
	TrianglePatch)
{
	DS_VS_OUTPUT_PS_INPUT Out;
	// Interpolate world space position with barycentric coordinates
	float3 vWorldPos = BarycentricCoordinates.x * TrianglePatch[0].vWorldPos +
		BarycentricCoordinates.y * TrianglePatch[1].vWorldPos +
		BarycentricCoordinates.z * TrianglePatch[2].vWorldPos;

	// Interpolate texture coordinates with barycentric coordinates
	Out.vTexCoord = BarycentricCoordinates.x * TrianglePatch[0].vTexCoord +
		BarycentricCoordinates.y * TrianglePatch[1].vTexCoord +
		BarycentricCoordinates.z * TrianglePatch[2].vTexCoord;

	// Interpolate normal with barycentric coordinates
	Out.vNormal = BarycentricCoordinates.x * TrianglePatch[0].vNormal +
		BarycentricCoordinates.y * TrianglePatch[1].vNormal +
		BarycentricCoordinates.z * TrianglePatch[2].vNormal;
	fDisplacement *= g_Scale;
	fDisplacement += g_Bias;
	float3 vDirection = -vNormal; // direction is opposite normal
	// translate the position
	vWorldPos += vDirection * fDisplacement;
	// transform to clip space
	Out.vPosCS = mul(float4(vWorldPos.xyz, 1.0),
		g_mWorldViewProjection);
	return Out;
} // end of domain shader