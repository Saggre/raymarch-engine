
void addPrimitive(iPrimitive primitive, in cMaterial primitiveMaterial, in float3 pos, inout float dist, inout cMaterial material) {
    float primitiveDist = primitive.ExecSDF(pos);
    if(primitiveDist < dist){
        dist = primitiveDist; 
        material = primitiveMaterial;
    }
}

void addPrimitiveSmooth(iPrimitive primitive, in cMaterial primitiveMaterial, in float3 pos, inout float dist, inout cMaterial material, float smoothing = 0.4) {
    float primitiveDist = opRound(primitive.ExecSDF(pos), smoothing);
    if(primitiveDist < dist){
        dist = primitiveDist; 
        material = primitiveMaterial;
    }
}

void subtractPrimitive(iPrimitive primitive, in float3 pos, inout float dist) {
    float primitiveDist = primitive.ExecSDF(pos);
    if(primitiveDist > dist){
        dist = primitiveDist; 
    }
}

void subtractPrimitiveSmooth(iPrimitive primitive, in float3 pos, inout float dist, float smoothing = 0.4) {
    float primitiveDist = opRound(primitive.ExecSDF(pos), smoothing);
    if(primitiveDist > dist){
        dist = primitiveDist; 
    }
}

float getDist(in float3 pos, out cMaterial material) {

    float ts = sin(time) + 1.0;

    // TODO get consts like materials from cpu

    cMaterial materialA;
    materialA.Create(float3(0.99, 0.99, 0.99));
    materialA.diffraction = 0.7;
    
    cMaterial materialB;
    materialB.Create(float3(0.95, 0.1, 0), 200);
    materialA.diffraction = 0.98;
    
    cMaterial materialC;
    materialC.Create(float3(0, 0.99, 0));
    materialC.diffraction = 0.98;
    
    cMaterial materialD;
    materialD.Create(float3(0, 0, 0.99), 100);
    materialD.diffraction = 0.98;

    cSphere sphere;
    sphere.Create(float3(0, pow(sin(time), 8), 0));
    sphere.scale.x = 0.8;
    
    cSphere sphere2;
    sphere2.Create(float3(0, 1, 0));
    sphere2.position = float3(sin(time)*2, 2.6 + sin(time)*0.1, cos(time)*4);

    cPlane plane;
    plane.Create(float3(0, -1, 0));
    
    cBox box;
    box.Create(float3(-1, 1, 0));
    box.scale = (1.5 - ts).xxx;
    
    cOctahedron octahedron;
    octahedron.Create(float3(1, 2.1, 0));
    octahedron.position = float3(sin(time), 3 + sin(time)*0.1, cos(time));
    octahedron.scale.x = 1 + sin(time)*0.3;
   
    cCylinder cyl;
    cyl.Create(float3(0, 0, 0));
   
    // ----
    
    float dist = MAX_DIST;
    
    addPrimitive(octahedron, materialC, pos, dist, material);
    addPrimitive(sphere, materialB, pos, dist, material);
    addPrimitive(sphere2, materialD, pos, dist, material);
    addPrimitive(plane, materialA, pos, dist, material);
    
    return dist;
    
    /*dist = sphere.ExecSDF(pos);
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
    }*/
      
    return dist;
}