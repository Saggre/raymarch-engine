
// Called once per control point
[domain("tri")] // indicates a triangle patch (3 verts)
[partitioning("fractional_odd")] // fractional avoids popping
// vertex ordering for the output triangles
[outputtopology("triangle_cw")][outputcontrolpoints(3)]
// name of the patch constant hull shader
[patchconstantfunc("ConstantsHS")]
[maxtessfactor(7.0)] //hint to the driver – the lower the better
// Pass in the input patch and an index for the control point
HS_CONTROL_POINT_OUTPUT HS(InputPatch<VS_OUTPUT_HS_INPUT, 3>
	inputPatch, uint uCPID : SV_OutputControlPointID)
{
	HS_CONTROL_POINT_OUTPUT Out;
	// Copy inputs to outputs – “pass through” shaders are optimal
	Out.vWorldPos = inputPatch[uCPID].vPosWS.xyz;
	Out.vTexCoord = inputPatch[uCPID].vTexCoord;
	Out.vNormal = inputPatch[uCPID].vNormal;
	Out.vLightTS = inputPatch[uCPID].vLightTS;
	return Out;
}

//Called once per patch. The patch and an index to the patch (patch
// ID) are passed in
HS_CONSTANT_DATA_OUTPUT ConstantsHS(InputPatch<VS_OUTPUT_HS_INPUT, 3>
	p, uint PatchID : SV_PrimitiveID)
{
	HS_CONSTANT_DATA_OUTPUT Out;
	// Assign tessellation factors – in this case use a global
	// tessellation factor for all edges and the inside. These are
	// constant for the whole mesh.
	Out.Edges[0] = g_TessellationFactor;
	Out.Edges[1] = g_TessellationFactor;
	Out.Edges[2] = g_TessellationFactor;
	Out.Inside = g_TessellationFactor;
	return Out;
}