// Compiler adds array lengts as consts on RaymarchEngine include
#include "RaymarchEngine"
#include "Primitives.hlsl"
#include "Utils.hlsl"

class cLight
{
    float3 position;
    float3 color;
    float intensity;
    
    void Create(float3 _position, float3 _color = float3(1, 1, 1), float _intensity = 1) {
        position = _position;
        color = _color;
        intensity = _intensity;
    }    
};

// Material base class
class cMaterial
{
    float3 diffuseColor;
    float shininess;
    float3 specularColor;
    float blank;
    
    void Create(float3 _diffuseColor, float _shininess = 50, float _specularColor = float3(1, 1, 1)) {
        diffuseColor = _diffuseColor;
        shininess = _shininess;
        specularColor = _specularColor;
        blank = 0;
    }
  
     float3 GetCheckered(float3 worldPosition) {
        return checkers(worldPosition.xz);
     }
};

class cRay
{
    float3 origin;
    float3 dir;
    
    void Create(float3 _origin, float3 _dir) {
        origin = _origin;
        dir = _dir;
    }
};

class cRaymarchResult
{
    cRay ray;
    cMaterial hitMaterial;
    float3 hitPos;
    float3 surfaceNormal;
    float stepsTaken;
    float hitDistance;
    
    void Create(cRay _ray, cMaterial _hitMaterial, float3 _hitPos, float3 _surfaceNormal, float _stepsTaken, float _hitDistance) {
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

// Primitive shape base class
class cBasePrimitive
{
    cMaterial material;
    float4 primitiveOptions;
	float3 position;
	float3 eulerAngles;
	float3 scale;
	
	void Create (cMaterial _material, float3 _position, float3 _eulerAngles = float3(0, 0, 0), float3 _scale = float3(1, 1, 1)) {
        material = _material;
        position = _position;
        eulerAngles = _eulerAngles;
        scale = _scale;
        primitiveOptions = float4(0, 0, 0, 0);
    }
   
};    

class cSphere : cBasePrimitive, iPrimitive
{
    float ExecSDF(float3 from) {
        return sdSphere(from - position, scale.x);
    }
};

class cBox : cBasePrimitive, iPrimitive
{
    float ExecSDF(float3 from) {
        return sdBox(from - position, scale);
    }
};

class cPlane : cBasePrimitive, iPrimitive
{
    float ExecSDF(float3 from) {
        return sdPlane(from - position);
    }
};

class cEllipsoid : cBasePrimitive, iPrimitive
{
    float ExecSDF(float3 from) {
        return sdEllipsoid(from - position, scale);
    }
};

class cTorus : cBasePrimitive, iPrimitive
{
    float ExecSDF(float3 from) {
        return sdTorus(from - position, primitiveOptions.xy);
    }
};

class cCappedTorus : cBasePrimitive, iPrimitive
{
    float ExecSDF(float3 from) {
        return sdCappedTorus(from - position, primitiveOptions.xy, primitiveOptions.z, primitiveOptions.w);
    }
};

cbuffer ShaderBuffer : register(b0)
{
	float3 cameraPosition;
	float aspectRatio;
	float3 cameraDirection;
	float time;
};

uniform StructuredBuffer<cSphere> spheres : register(t0); 
uniform StructuredBuffer<cBox> boxes : register(t1); 
uniform StructuredBuffer<cPlane> planes : register(t2); 
uniform StructuredBuffer<cEllipsoid> ellipsoids : register(t3); 
uniform StructuredBuffer<cTorus> tori : register(t4); 
uniform StructuredBuffer<cCappedTorus> cappedTori : register(t5); 

struct VS_INPUT
{
	float4 Position : POSITION;
	float2 TexCoord : TEXCOORD;
};

struct PS_INPUT
{
	float4 Position : SV_POSITION;
	float2 TexCoord : TEXCOORD;
};