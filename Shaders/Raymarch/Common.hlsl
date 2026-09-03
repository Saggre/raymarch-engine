// Compiler adds array lengths as constants on RaymarchEngine include
#include "RaymarchEngine"
#include "Primitives.hlsl"
#include "Utils.hlsl"
#include "Options.hlsl"

class cLight
{
    float3 direction; // Normalised, pointing from the scene towards the light
    float3 color;

    void Create(float3 _direction, float3 _color = float3(1, 1, 1))
    {
        direction = _direction;
        color = _color;
    }
};

// Material base class
class cMaterial
{
    float3 diffuseColor;
    float shininess;
    float3 specularColor;
    float diffraction; // 0 = nothing, 1 = full reflective, -1 = full refractive

    void Create(float3 _diffuseColor = float3(1.0, 1.0, 1.0), float _shininess = 50.0,
                float3 _specularColor = float3(1.0, 1.0, 1.0), float _diffraction = 0.0)
    {
        diffuseColor = _diffuseColor;
        shininess = _shininess;
        specularColor = _specularColor;
        diffraction = _diffraction;
    }

    float3 GetCheckered(float3 worldPosition)
    {
        return checkers(worldPosition.xz);
    }

    void fuse(cMaterial material)
    {
        diffuseColor = diffuseColor * 0.5 + material.diffuseColor * 0.5;
        shininess = shininess * 0.5 + material.shininess * 0.5;
        specularColor = specularColor * 0.5 + material.specularColor * 0.5;
        diffraction = diffraction * 0.5 + material.diffraction * 0.5;
    }
};

class cRay
{
    float3 origin;
    float3 dir;

    void Create(float3 _origin, float3 _dir)
    {
        origin = _origin;
        dir = _dir;
    }
};

class cRaymarchResult
{
    cRay ray;
    cMaterial hitMaterial;
    float3 hitPos; // Hit position 3d world coords
    float3 surfaceNormal; // Normal of hit surface
    float stepsTaken; // Steps needed to calculate this result
    float hitDistance; // Distance from ray start to end

    void Create(cRay _ray, cMaterial _hitMaterial, float3 _hitPos, float3 _surfaceNormal, float _stepsTaken,
                float _hitDistance)
    {
        ray = _ray;
        hitMaterial = _hitMaterial;
        hitPos = _hitPos;
        surfaceNormal = _surfaceNormal;
        stepsTaken = _stepsTaken;
        hitDistance = _hitDistance;
    }
};

// Primitive shape interface
interface iPrimitive
{
    float ExecSDF(float3 from);
};

// Primitive shape base class. ExecSDF receives a point already in this primitive's local
// frame, so position and rotation are not stored here.
class cBasePrimitive
{
    float4 primitiveOptions;
    float3 scale;

    void Create(float3 _scale = float3(1, 1, 1), float4 _primitiveOptions = float4(0, 0, 0, 0))
    {
        scale = _scale;
        primitiveOptions = _primitiveOptions;
    }
};

class cSphere : cBasePrimitive, iPrimitive
{
    float ExecSDF(float3 from)
    {
        return sdSphere(from, scale.x);
    }
};

class cCylinder : cBasePrimitive, iPrimitive
{
    float ExecSDF(float3 from)
    {
        // scale.x radius, scale.y half height. Four scalars resolve to the arbitrary orientation
        // overload, whose axis collapses to zero.
        return sdCylinder(from, float2(scale.x, scale.y));
    }
};

class cBox : cBasePrimitive, iPrimitive
{
    float ExecSDF(float3 from)
    {
        return sdBox(from, scale);
    }
};

class cPlane : cBasePrimitive, iPrimitive
{
    float ExecSDF(float3 from)
    {
        return sdPlane(from);
    }
};

class cEllipsoid : cBasePrimitive, iPrimitive
{
    float ExecSDF(float3 from)
    {
        return sdEllipsoid(from, scale);
    }
};

class cTorus : cBasePrimitive, iPrimitive
{
    float ExecSDF(float3 from)
    {
        // scale.x is the major radius, scale.y the minor radius
        return sdTorus(from, float2(scale.x, scale.y));
    }
};

class cCappedTorus : cBasePrimitive, iPrimitive
{
    float ExecSDF(float3 from)
    {
        return sdCappedTorus(from, primitiveOptions.xy, primitiveOptions.z, primitiveOptions.w);
    }
};

class cOctahedron : cBasePrimitive, iPrimitive
{
    float ExecSDF(float3 from)
    {
        return sdOctahedron(from, scale.x);
    }
};

// Mirrors MaterialBufferData in RaymarchRenderer.cs
struct cMaterialData
{
    float3 color;
    float shininess;
    float specularStrength;
    float diffraction;
    float2 padding;
};

// Mirrors PrimitiveBufferData in RaymarchRenderer.cs. Every vector starts on a 16 byte boundary
// on both sides, so the two layouts have to be changed together.
struct cPrimitiveData
{
    cMaterialData material;
    float4 options;
    float3 position;
    float positionPadding;
    float4 rotation;
    float3 scale;
    float scalePadding;
};

cbuffer ShaderBuffer : register(b0)
{
float3 cameraPosition;
float aspectRatio;
float3 cameraDirection;
float time;
float4 additionalData;
};

// Buffers
// One buffer per primitive type, filled by RenderDevice.Draw. The register indices have to match
// the primitivesBuffer slots there.
StructuredBuffer<cPrimitiveData> spheres : register(t0);
StructuredBuffer<cPrimitiveData> boxes : register(t1);
StructuredBuffer<cPrimitiveData> planes : register(t2);
StructuredBuffer<cPrimitiveData> toruses : register(t3);
StructuredBuffer<cPrimitiveData> octahedrons : register(t4);
StructuredBuffer<cPrimitiveData> ellipsoids : register(t5);
StructuredBuffer<cPrimitiveData> cylinders : register(t6);

// Dither source for the AO term. This is low-frequency fractal value noise, not blue noise:
// see CreateNoise in RenderDevice. TODO generate real blue noise (void-and-cluster).
// t0..t7 belong to the per-primitive structured buffers, so the noise texture starts at t8.
Texture2D<float4> noiseTexture : register(t8);

// Value noise lattice for the clouds. Red holds one z slice and green the next, see cloudNoise in
// Sky.hlsl and CreateCloudNoise in RenderDevice.
Texture2D<float4> cloudNoiseTexture : register(t9);

SamplerState textureSampler : register(s0);

// The cloud lattice has to tile, which the default clamping sampler cannot do
SamplerState wrapSampler : register(s1);

struct VS_INPUT
{
    uint vI : SV_VERTEXID;
    float4 Position : POSITION;
    float2 TexCoord : TEXCOORD;
};

struct PS_INPUT
{
    float4 Position : SV_POSITION;
    float2 TexCoord : TEXCOORD;
};

// Needs the constant buffer and the textures above, so it comes last
#include "Sky.hlsl"
