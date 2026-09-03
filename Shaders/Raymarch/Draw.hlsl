void addPrimitive(iPrimitive primitive, in cMaterial primitiveMaterial, in float3 pos, inout float dist,
                  inout cMaterial material)
{
    float primitiveDist = primitive.ExecSDF(pos);
    if (primitiveDist < dist)
    {
        dist = primitiveDist;
        material = primitiveMaterial;
    }
}

void addPrimitiveSmooth(iPrimitive primitive, in cMaterial primitiveMaterial, in float3 pos, inout float dist,
                        inout cMaterial material, float smoothing = 0.4)
{
    float primitiveDist = opRound(primitive.ExecSDF(pos), smoothing);
    if (primitiveDist < dist)
    {
        dist = primitiveDist;
        material = primitiveMaterial;
    }
}

void subtractPrimitive(iPrimitive primitive, in float3 pos, inout float dist)
{
    float primitiveDist = primitive.ExecSDF(pos);
    if (primitiveDist > dist)
    {
        dist = primitiveDist;
    }
}

void subtractPrimitiveSmooth(iPrimitive primitive, in float3 pos, inout float dist, float smoothing = 0.4)
{
    float primitiveDist = opRound(primitive.ExecSDF(pos), smoothing);
    if (primitiveDist > dist)
    {
        dist = primitiveDist;
    }
}

// Turns a primitive's uploaded surface parameters into a shading material
void toMaterial(in cMaterialData data, out cMaterial material)
{
    material.diffuseColor = data.color;
    material.shininess = data.shininess;
    material.specularColor = data.specularStrength.xxx;
    material.diffraction = data.diffraction;
}

// The scene is whatever the engine uploaded this frame. The counts are baked in by
// HLSLFileIncludeHandler, so each loop has a constant bound and vanishes when the type is unused.
float getDist(in float3 pos, out cMaterial material)
{
    float dist = MAX_DIST;

    // material is an out parameter, so it has to be written even when the scene is empty
    material.diffuseColor = float3(1.0, 1.0, 1.0);
    material.shininess = 50.0;
    material.specularColor = float3(1.0, 1.0, 1.0);
    material.diffraction = 0.0;

    [loop]
    for (int s = 0; s < sphereCount; s++)
    {
        cPrimitiveData data = spheres[s];

        cSphere sphere;
        sphere.Create(data.position, data.eulerAngles, data.scale);
        sphere.primitiveOptions = data.options;

        cMaterial sphereMaterial;
        toMaterial(data.material, sphereMaterial);

        addPrimitive(sphere, sphereMaterial, pos, dist, material);
    }

    [loop]
    for (int b = 0; b < boxCount; b++)
    {
        cPrimitiveData data = boxes[b];

        cBox box;
        box.Create(data.position, data.eulerAngles, data.scale);
        box.primitiveOptions = data.options;

        cMaterial boxMaterial;
        toMaterial(data.material, boxMaterial);

        addPrimitive(box, boxMaterial, pos, dist, material);
    }

    [loop]
    for (int p = 0; p < planeCount; p++)
    {
        cPrimitiveData data = planes[p];

        cPlane plane;
        plane.Create(data.position, data.eulerAngles, data.scale);
        plane.primitiveOptions = data.options;

        cMaterial planeMaterial;
        toMaterial(data.material, planeMaterial);

        addPrimitive(plane, planeMaterial, pos, dist, material);
    }

    return dist;
}
