cbuffer ShaderData : register(b0)
{
	float4x4 World;
	float4x4 View;
	float4x4 Projection;
}

float tessellationAmount;
float3 tessellationPadding;

// The HullInputType structure is the same as the output structure from the vertex shader.

struct HullInputType
{
	float3 position : POSITION;
	float4 color : COLOR;
	float2 TextureCoordinate : TEXCOORD0;
};

// The ConstantOutputType structure is what will be the output from the patch constant function.

struct ConstantOutputType
{
	float edges[3] : SV_TessFactor;
	float inside : SV_InsideTessFactor;
};

// The HullOutputType structure is what will be the output from the hull shader.

struct HullOutputType
{
	float3 position : POSITION;
	float4 color : COLOR;
	float2 TextureCoordinate : TEXCOORD0;
};

////////////////////////////////////////////////////////////////////////////////
// Patch Constant Function
////////////////////////////////////////////////////////////////////////////////
ConstantOutputType ColorPatchConstantFunction(InputPatch<HullInputType, 3> inputPatch, uint patchId : SV_PrimitiveID)
{
	ConstantOutputType output;


	// Set the tessellation factors for the three edges of the triangle.
	output.edges[0] = tessellationAmount;
	output.edges[1] = tessellationAmount;
	output.edges[2] = tessellationAmount;

	// Set the tessellation factor for tessallating inside the triangle.
	output.inside = tessellationAmount;

	return output;
}

////////////////////////////////////////////////////////////////////////////////
// Hull Shader
////////////////////////////////////////////////////////////////////////////////
[domain("tri")]
[partitioning("integer")]
[outputtopology("triangle_cw")]
[outputcontrolpoints(3)]
[patchconstantfunc("ColorPatchConstantFunction")]

HullOutputType ColorHullShader(InputPatch<HullInputType, 3> patch, uint pointId : SV_OutputControlPointID, uint patchId : SV_PrimitiveID)
{
	HullOutputType output;


	// Set the position for this control point as the output position.
	output.position = patch[pointId].position;

	// Set the input color as the output color.
	output.color = patch[pointId].color;

	output.tex = patch[pointId].tex;

	return output;
}

////////////////////////////////////////////////////////////////////////////////
// Domain Shader
////////////////////////////////////////////////////////////////////////////////
[domain("tri")]

// The idea is that the GPU runs the domain shader for each generated vertex of the tessellated patch, 
// and you have complete control over where these vertices go. 
// That's how you can implement different algorithms such as bicubic patches, PN triangles, subdivision surfaces, displacement mapping, etc.
VertexShaderInput ColorDomainShader(ConstantOutputType input, float3 uvwCoord : SV_DomainLocation, const OutputPatch<HullOutputType, 3> patch)
{
	float3 vertexPosition;
	VertexShaderInput output;

	// Determine the position of the new vertex.
	vertexPosition = uvwCoord.x * patch[0].position + uvwCoord.y * patch[1].position + uvwCoord.z * patch[2].position;

	// Get texture pixel
	float4 textureColor = tex2Dlod(textureSampler, float4(patch[0].tex, 0, 10));

	// Set height from heightmap (red channel)
	vertexPosition.y = textureColor.r * 20;

	// Calculate the position of the new vertex against the world, view, and projection matrices.
	output.Position = mul(float4(vertexPosition, 1.0f), World);
	output.Position = mul(output.Position, View);
	output.Position = mul(output.Position, Projection);


	// Send the input color into the pixel shader.
	//output.color = patch[0].color;

	return output;
}