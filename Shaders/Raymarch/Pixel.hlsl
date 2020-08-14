#include "Common.hlsl"

#define MAX_STEPS 700
#define MAX_DIST 400
#define SHADOW_MAX_DIST 200
#define AO_FALLOFF 100
#define SURF_DIST 1e-2

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
    materialA.Create(float3(0.9, 0.1, 0.1));

    cMaterial materialB;
    materialB.Create(float3(0.3, 0.8, 0.2));
    
    cMaterial materialC;
    materialC.Create(float3(0.1, 0.1, 0.1));

    cSphere sphere;
    sphere.Create(materialA, float3(3, 1, 0));
    
    cPlane plane;
    plane.Create(materialC, float3(0, -1, 0));
    
    cBox box;
    box.Create(materialB, float3(-1, 1, 0));
    
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
    
    float boxDist = box.ExecSDF(pos);
    if(boxDist < dist){
        dist = boxDist;
        material = box.material;
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
float raymarch(in cRay ray, out float steps, out cMaterial hitMaterial) {
	float totalDist = 0.0;

    int i = 0; 
	while(i < MAX_STEPS) {
		float3 marchPos = ray.origin + totalDist * ray.dir;
		float curDist = getDist(marchPos, hitMaterial);
		totalDist += curDist;
		if (curDist < SURF_DIST || totalDist > MAX_DIST) {
			break;
		}
		i++;
	}
	
	steps = i * 1.0;

	return totalDist;
}

// Get shadow at position
// lightDir is direction from object surface to light source
float getShadow(in float3 pos, in float3 lightDir, float shadowHardness = 64, float shadowIntensity = 0.990)
{
    float res = 1.0;
    float3 rayOrigin = pos;
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
float getLight(in float3 pos, in cLight light) {
	float3 lightDir = normalize(light.position - pos);
	float3 normal = getNormal(pos);

	float dif = clamp(dot(normal, lightDir), 0.0, 1.0);
    dif *= getShadow(pos, lightDir);

	return dif;
}

float3 getCameraRayDir(float2 uv, float fov)
{
    float3 camForward = cameraDirection;
    float3 camRight = normalize(cross(float3(0.0, 1.0, 0.0), camForward));
    float3 camUp = normalize(cross(camForward, camRight));
     
    return normalize(uv.x * camRight + uv.y * camUp + camForward * fov);
}

float3 getPhongLight(cMaterial material, cRay ray, float dist, cLight light) {
    float3 p = ray.origin + ray.dir * dist;
    
    float3 N = getNormal(p); // surface normal
    float3 L = normalize(light.position - p); // surface to light direction
    float3 V = -ray.dir; // surface to camera direction
    float3 R = reflect(-L, N); // mirror of L by the axis N
    
    float dotLN = dot(L, N); // project light direction to surface normal
    float dotRV = dot(R, V); // project light reflection direction to direction to camera
    
    float3 color = float3(0, 0, 0);
    
    if (dotLN > 0.) { // we can see the diffuse
        color += material.diffuseColor * dotLN;
    }
    
    if (dotRV > 0.) { // we can see the specular
        color += material.specularColor * pow(dotRV, material.shininess);
    }
    
    color *= light.color; // influence of color of the light
    color *= getShadow(p, L);
    return color;
}

float4 main(PS_INPUT input) : SV_Target
{
    mainLight.Create(float3(70, 200, 100));

    // Move light
	mainLight.position.xz += float2(sin(time), cos(time)) * 2.0;

    float3 FOG_COLOR = float3(0.2, 0.2, 0.3);

	float2 uv = input.TexCoord - (0.5).xx;
	uv.x *= aspectRatio;
	
	cRay ray;
	ray.Create(cameraPosition, getCameraRayDir(uv, 1.0));

    cMaterial material;
    float steps;
	float dist = raymarch(ray, steps, material);
	
	if (dist > MAX_DIST) {
	    return float4(FOG_COLOR, 1);
	    //discard;
	}

	float3 hitPos = ray.origin + ray.dir * dist;

    float3 ambientColor = float3(0.05, 0.05, 0.05);
	float3 color = float3(0, 0, 0);

    color += ambientColor;
    color += getPhongLight(material, ray, dist, mainLight);

	float gamma = 0.4545;

    // AO
    color *= lerp(pow(1.0 - (steps / MAX_STEPS), 16), 1, dist / AO_FALLOFF);

    // Gamma correction
	color = pow(color, gamma.xxx); 

    // Fog
    color = lerp(color, FOG_COLOR, dist / MAX_DIST);

	return float4(color, 1);
}