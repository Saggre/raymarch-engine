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

// Moves a world space point into a primitive's local frame, so the SDFs stay axis aligned.
// Rotating by the conjugate is the inverse of rotating the primitive.
float3 toLocalSpace(float3 worldPos, in cPrimitiveData data)
{
    float3 p = worldPos - data.position;
    float3 axis = -data.rotation.xyz;
    return p + 2.0 * cross(axis, cross(axis, p) + data.rotation.w * p);
}

// One loop per primitive type. The bodies differ only by buffer, count and class, so a macro
// keeps them from drifting apart. Each expansion is braced at the call site to scope its locals.
#define ADD_PRIMITIVE_TYPE(PrimitiveClass, primitiveBuffer, primitiveCount)             \
    [loop]                                                                              \
    for (int primitiveIndex = 0; primitiveIndex < primitiveCount; primitiveIndex++)     \
    {                                                                                   \
        cPrimitiveData data = primitiveBuffer[primitiveIndex];                          \
                                                                                        \
        PrimitiveClass shape;                                                           \
        shape.Create(data.scale, data.options);                                         \
                                                                                        \
        cMaterial shapeMaterial;                                                        \
        toMaterial(data.material, shapeMaterial);                                       \
                                                                                        \
        addPrimitive(shape, shapeMaterial, toLocalSpace(pos, data), dist, material);    \
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

    { ADD_PRIMITIVE_TYPE(cSphere, spheres, sphereCount) }
    { ADD_PRIMITIVE_TYPE(cBox, boxes, boxCount) }
    { ADD_PRIMITIVE_TYPE(cPlane, planes, planeCount) }
    { ADD_PRIMITIVE_TYPE(cTorus, toruses, torusCount) }
    { ADD_PRIMITIVE_TYPE(cOctahedron, octahedrons, octahedronCount) }
    { ADD_PRIMITIVE_TYPE(cEllipsoid, ellipsoids, ellipsoidCount) }
    { ADD_PRIMITIVE_TYPE(cCylinder, cylinders, cylinderCount) }

    return dist;
}
