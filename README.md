# Raymarch Engine (WIP)

A game engine that renders primitive objects by utilizing raymarching. Different operations can be applied to the
primitives to create more complex objects. The engine uses `SharpDX` as its `DirectX`
wrapper and `System.Numerics.Vectors` for Vectors, Quaternions and Matrices.

![Rendering preview](.github/assets/raymarch.gif)

## Requirements:

- DirectX 11
- Shader Model 5 support

## Supported primitives:

- Sphere
- Box
- Plane
- Torus
- Capped torus
- Ellipsoid
- Capsule
- Hex prism
- Cone
- Cylinder
- Pyramid
- Octahedron
- Rhombus

## Supported operations:

+ Rounding
+ Infinite repetition
+ Union
+ Subtraction
+ Intersection
+ Onion slicing

## Current features and future work:

- :heavy_check_mark: Basic Gameobjects
- :heavy_minus_sign: All primitives
- :heavy_check_mark: Shading
- :heavy_check_mark: AO
- :heavy_check_mark: Subsurface scattering
- :heavy_check_mark: Blue noise
- :heavy_check_mark: Reflections
- :heavy_check_mark: Soft shadows
- :x: Dynamic sky
- :x: Custom resolutions
- :x: Color blending between shapes
- :x: Fractals
- :x: Physics
- :x: Custom shader code editor
- :x: Checkerboard rendering
- :x: Pre-compiled shaders
- :x: OpenGL support
- :x: Hyperbolic and spherical spaces