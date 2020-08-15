#include "Common.hlsl"

#define MAX_STEPS 1000
#define MAX_DIST 100
#define SHADOW_MAX_DIST 50
#define AO_FALLOFF 40
#define SURF_DIST 1e-3

static cLight mainLight;

// Returns distance from position to the closest point in scene geometry
/*float getDist2(float3 pos, out float3 color) {
    float minDist = MAX_DIST;

    // TODO [unroll(sphereCount)] 
    for(int i = 0; i < sphereCount; i++){
        float3 objectColor;
        float curDist = spheres[i].ExecSDF(pos);
        
        if(curDist < minDist){
            color = objectColor;
            minDist = curDist; 
        }
    }
    
    for(int i = 0; i < planeCount; i++){
        float3 objectColor;
        float curDist = planes[i].ExecSDF(pos);
        
        if(curDist < minDist){
            color = objectColor;
            minDist = curDist; 
        }
    }
    
    for(int i = 0; i < boxCount; i++){
        float3 objectColor;
        float curDist = boxes[i].ExecSDF(pos);
        
        if(curDist < minDist){
            color = objectColor;
            minDist = curDist; 
        }
    }
    
    color = float3(0, 0, 0);
	return minDist;
}*/

float getDist(in float3 pos, out cMaterial material){

    cMaterial materialA;
    materialA.Create(float3(0.4, 0.4, 0.4));
    materialA.diffraction = 0.99;
    
    cMaterial materialB;
    materialB.Create(float3(0.3, 0.8, 0.2));
    
    cMaterial materialC;
    materialC.Create(float3(0.1, 0.1, 0.1));
    materialC.diffraction = 0.33;
    
    cMaterial materialD;
    materialD.Create(float3(0.2, 0.1, 0.8));
    materialD.diffraction = 0.33;

    cSphere sphere;
    sphere.Create(materialA, float3(sin(time) + 0.5, 1, 0));
    
    cPlane plane;
    plane.Create(materialC, float3(0, -1, 0));
    
    cBox box;
    box.Create(materialB, float3(-1, 1, 0));
    
    cOctahedron octahedron;
    octahedron.Create(materialD, float3(1, 2.1, 0));
    
    /*iPrimitive primitives[3] = {
        sphere,
        plane,
        box
    };
    
    float dist = MAX_DIST;
    int closestIndex = 0;
    for(int i = 0; i<3; i++){
        float sdf = primitives[i].ExecSDF(pos);
        if(sdf < dist){
            dist = sdf;
            closestIndex = 0;
        }
    }
    
    returnMaterial = primitives[closestIndex].GetMaterial();*/
    
    float dist = MAX_DIST;
    
    dist = sphere.ExecSDF(pos);
    material = sphere.material;
    
    float planeDist = plane.ExecSDF(pos);
    if(planeDist < dist){
        dist = planeDist; // TODO materialMin()
        material = plane.material;
    }
    
    float boxDist = opRound(box.ExecSDF(pos), 0.4);
    if(boxDist < dist){
        dist = boxDist;
        material = box.material;
    }
    
    float octDist = octahedron.ExecSDF(pos);
    if(octDist < dist){
        dist = octDist;
        material = octahedron.material;
    }
      
    return dist;
}

// Calculate surface normal at position
float3 getNormal(in float3 pos) {
    cMaterial material;
	float2 e = float2(0.01, 0);
	float3 n = getDist(pos, material) - float3(
		getDist(pos - e.xyy, material),
		getDist(pos - e.yxy, material),
		getDist(pos - e.yyx, material)
	);

	return normalize(n);
}

// Returns distance from rayOrigin to an object in the GetDist() scene, in ray direction rayDir
void raymarch(in cRay ray, out cRaymarchResult raymarchResult) {
    raymarchResult.ray = ray;
	float totalDist = 0.0;
    float3 marchPos;
    
    int i = 0; 
	while(i < MAX_STEPS) {
		marchPos = ray.origin + totalDist * ray.dir;
		float curDist = getDist(marchPos, raymarchResult.hitMaterial);
		totalDist += curDist;
		if (curDist < SURF_DIST || totalDist > MAX_DIST) {
			break;
		}
		i++;
	}
	
	float3 hitPos = ray.origin + totalDist * ray.dir;
	raymarchResult.hitPos = hitPos;
	raymarchResult.stepsTaken = i * 1.0;
    raymarchResult.hitDistance = totalDist; 
    raymarchResult.surfaceNormal = getNormal(hitPos);
}

// Get shadow at position
// lightDir is direction from object surface to light source
float getShadow(in cRaymarchResult raymarchResult, in float3 lightDir, float shadowHardness = 4, float shadowIntensity = 0.995)
{
    float res = 1.0;
    float3 rayOrigin = raymarchResult.hitPos + raymarchResult.surfaceNormal * 0.01;
    float mint = SURF_DIST * 2.0;
    float ph = 1e20;
    
    cMaterial material;
    
    // t = distance from object surface towards light source
    for(float t = mint; t < SHADOW_MAX_DIST;)
    {
        float h = getDist(rayOrigin + lightDir * t, material);
        if(h<0.001){
            res = 0;
            break;
        }
        float y = h * h / (2.0 * ph);
        float d = sqrt(h * h - y * y);
        res = min(res, shadowHardness * d / max(0.0, t - y));
        ph = h;
        t += h;
    }
    
    return lerp(1.0 - shadowIntensity, 1.0, res);
}

// Get light at position
float getLight(cRaymarchResult raymarchResult) {
	float3 lightDir = normalize(mainLight.position - raymarchResult.hitPos);
	float dif = clamp(dot(raymarchResult.surfaceNormal, lightDir), 0.0, 1.0);
	return dif;
}

float3 getCameraRayDir(float2 uv, float fov)
{
    float3 camForward = cameraDirection;
    float3 camRight = normalize(cross(float3(0.0, 1.0, 0.0), camForward));
    float3 camUp = normalize(cross(camForward, camRight));
     
    return normalize(uv.x * camRight + uv.y * camUp + camForward * fov);
}

float3 getPhongLight(cRaymarchResult raymarchResult) {
    float3 normal = raymarchResult.surfaceNormal;
    float3 lightReverseDir = normalize(mainLight.position - raymarchResult.hitPos); 
    float3 reverseRayDir = -raymarchResult.ray.dir; 
    float3 R = reflect(-lightReverseDir, normal); 
    
    float dotLN = dot(lightReverseDir, normal); // project light direction to surface normal
    float dotRV = dot(R, reverseRayDir); // project light reflection direction to direction to camera
    
    float3 color = float3(0, 0, 0);
    
    if (dotLN > 0.0) { 
        color += raymarchResult.hitMaterial.diffuseColor * dotLN;
    }
    
    if (dotRV > 0.0) {
        color += raymarchResult.hitMaterial.specularColor * pow(dotRV, raymarchResult.hitMaterial.shininess);
    }

    return color;
}

float3 getColor(cRaymarchResult raymarchResult) {
	float3 color = float3(0, 0, 0);

    color += getPhongLight(raymarchResult);
    color *= mainLight.color;

	return color;
}

float3 getReflection(cRaymarchResult raymarchResult) {
    cRay ray;
    cRaymarchResult refRaymarchResult;
	ray.Create(raymarchResult.hitPos + raymarchResult.surfaceNormal * 0.01, reflect(raymarchResult.ray.dir, -raymarchResult.surfaceNormal));

    raymarch(ray, refRaymarchResult);
    
    if(refRaymarchResult.hitDistance >= MAX_DIST){
        return float3(0, 0, 0);
    }
    
    return getPhongLight(refRaymarchResult);
}

float getSubsurfCheap(cRaymarchResult raymarchResult) {
    float sub = 0.0;
    cMaterial mat;
    for (int i = 0; i < 30; i++) {
        float dist = i * 0.4 / 30.0;
        sub += dist + getDist(raymarchResult.hitPos - raymarchResult.surfaceNormal * dist, mat);
    }
    sub /= 30.0;
    return sub;
}

float4 main(PS_INPUT input) : SV_Target
{
    float3 FOG_COLOR = float3(0.2, 0.2, 0.3);

    mainLight.Create(float3(70, 200, 100));

    // Move light
	mainLight.position.xz += float2(sin(time), cos(time)) * 2.0;

	float2 uv = input.TexCoord - (0.5).xx;
	uv.x *= aspectRatio;
	
	cRay ray;
	ray.Create(cameraPosition, getCameraRayDir(uv, 1.0));

    cRaymarchResult raymarchResult;
	raymarch(ray, raymarchResult);
	
	if (raymarchResult.hitDistance >= MAX_DIST) {
	    return float4(FOG_COLOR, 1);
	}
	
	// AO
    float AOIntensity = raymarchResult.hitDistance / AO_FALLOFF;
    float AO = pow(1.0 - (raymarchResult.stepsTaken / MAX_STEPS), 16);
	
	// Fog
	float3 fogIntensity = raymarchResult.hitDistance / MAX_DIST;
	
	float3 sceneColor = getColor(raymarchResult);
	
	float3 lightReverseDir = normalize(mainLight.position - raymarchResult.hitPos); 
	
	float3 shadow = getShadow(raymarchResult, lightReverseDir);
	
	// Reflection
	sceneColor += getReflection(raymarchResult) * saturate(raymarchResult.hitMaterial.diffraction);
	
	sceneColor += getSubsurfCheap(raymarchResult).xxx * pow(raymarchResult.hitMaterial.diffuseColor, 0.5);
	
	float3 ambientColor = float3(0.001, 0.001, 0.001);
    sceneColor += ambientColor;
	
	sceneColor *= shadow;
	
	// Apply AO
	sceneColor *= lerp(AO, 1, AOIntensity);
	
	// Gamma correction
	sceneColor = pow(sceneColor, 0.5); 
	
	// Apply fog
	sceneColor = lerp(sceneColor, FOG_COLOR, fogIntensity);
	
	return float4(saturate(sceneColor), 1);
}