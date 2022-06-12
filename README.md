# Raymarch Engine (WIP)

A game engine that renders primitive objects by utilizing raymarching. Different operations can be applied to the
primitives to create more complex objects. The engine uses `SharpDX` as its `DirectX`
wrapper and `System.Numerics.Vectors` for Vectors, Quaternions and Matrices.

## Live rendering preview

### Things to look for:

- The preview has four different primitives (octahedron, box, sphere, plane). And one light source.
- All objects cast and receive shadows. These are fully dynamic.
- Octahedron, sphere and plane have reflections.
- All objects have Blinn–Phong shading as a base.
- The octahedron has subsurface scattering. light penetrates its thin edges, making them appear lighter.
- The box has rounded corners as a result of a rounding operation being applied to it.
- There is ambient occlusion and blue noise applied to the view space.
- Some purple-ish distance fog is visible in the background.
- Raymarched objects have infinite resolution (signed distance function = no mesh).

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

- :heavy_check_mark: Basic gameobjects
- :heavy_check_mark: Basic shading (Blinn–Phong)
- :heavy_check_mark: Ambient occlusion
- :heavy_check_mark: Subsurface scattering
- :heavy_check_mark: Blue noise
- :heavy_check_mark: Reflections
- :heavy_check_mark: Soft shadows
- :heavy_minus_sign: All primitives
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