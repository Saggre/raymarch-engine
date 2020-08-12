# Raymarch Engine

WIP

A game engine that renders primitive objects by utilizing raymarching.
Objects can be created, moved and otherwise manipulated traditionally on the CPU, and their data is sent to the GPU to be raymarched.
The engine uses SharpDX as its DirectX wrapper and System.Numerics.Vectors for Vectors, Quaternions and Matrices.

Example syntax for adding a Sphere:
```csharp
currentScene.AddGameObject(new Sphere()
{
    Position = new Vector3(2f, 0, 0),
    Radius = 1f
});
```

### Requirements:
DirectX 11 & Shader Model 5 support. Requirements will be lowered in the future.

### The engine supports following objects:
+ Sphere
+ Box
+ Plane
+ Torus
+ Capped torus
+ Ellipsoid
+ Capsule
+ Hex prism
+ Cone
+ Cylinder
+ Pyramid
+ Octahedron
+ Rhombus

### Following operations can be performed to shapes to make more complex sets:
+ Rounding
+ Infinite repetition
+ Union
+ Subtraction
+ Intersection
+ Onion slicing

### Features and future implementations:
+ :heavy_check_mark: Basic Gameobjects
+ :heavy_minus_sign: All primitives
+ :x: Shading
+ :x: Reflections
+ :x: Soft shadows
+ :x: Custom resolutions
+ :x: Color blending between shapes
+ :x: Fractal support
+ :x: Physics
+ :x: Custom shader code editor
+ :x: Checkerboard rendering
+ :x: Pre-compiled shaders
+ :x: OpenGL support
+ :x: Hyperbolic and spherical spaces

![Preview](https://i.imgur.com/FjwuLGe.png)