# AGENTS.md

Guidance for AI coding agents working in this repository.

## What this is

`RaymarchEngine` is a real-time 3D engine that renders primitives with signed distance
functions instead of meshes. Rendering is a single fullscreen quad; all shape data is
uploaded to the GPU in buffers and the raymarch pixel shader does the work.

- Language: C# 8.0 (`LangVersion` per configuration; `7.3` on the fallback `AnyCPU` ones)
- Target: .NET Framework 4.8, Windows only, `Exe` (WinForms host window)
- Graphics: DirectX 11 via `SharpDX` 4.2.0, Shader Model 5 (`vs_5_0` / `ps_5_0`)
- Math: `System.Numerics` (`Vector3`, `Quaternion`, `Matrix4x4`), not `SharpDX.Mathematics`
- Physics: `BepuPhysics` 2.2.0
- Project style: legacy non-SDK `.csproj` with `packages.config` NuGet restore

## Layout

| Path | Contents |
| --- | --- |
| `Ignition.cs` | `Main`, constructs `Engine` with a `GameLogic` instance and runs it |
| `GameLogic.cs` | Sample game code driving the camera, kept separate from the engine |
| `Core/` | Engine core: `Engine`, `Scene`, `GameObject`, `Camera`, `Shader`, `Movement` |
| `Core/Rendering/` | `RenderDevice` (D3D11 device, swap chain, draw loop), `RaymarchRenderer` |
| `Core/Buffers/` | `ConstantBuffer<T>`, `StructuredBuffer<T>`, `TextureBuffer<T>` wrappers |
| `Core/Primitives/` | Marker types (`Sphere`, `Box`, `Plane`, `Torus`, `Octahedron`, `Ellipsoid`, `Cylinder`) implementing `IPrimitive` |
| `Core/Input/` | `InputDevice` with static `Keyboard` / `Mouse`, `PlayerMovement` |
| `EMath/` | Math helpers and extension methods, `Vector2Int`, `Byte4` |
| `Geometry/` | `RenderVertex` (input layout), `Primitive`, `SquareRect` |
| `Physics/` | `PhysicsHandler` (Bepu simulation), `PrimitivePhysics` component |
| `Shaders/Raymarch/` | HLSL: `Vertex.hlsl`, `Pixel.hlsl` and the includes they pull in |
| `RaymarchEngineTests/` | NUnit 3 tests, SDK-style project referencing the engine |

## Architecture

**Component model.** A `GameObject` owns a `Movement` (position, rotation, scale) and a
list of `IComponent`. Components implement `Start(int startTime)`, `Update(float deltaTime)`
and `End(int endTime)`, plus `OnAddedToGameObject(GameObject parent)`. `Movement` and
`RaymarchRenderer<T>` are components. `Camera` is **not** a component: it derives from
`GameObject`, so it is added to the scene with `AddGameObject`, never `AddComponent`.
`Scene.CurrentScene` is the single active scene and holds the game objects and the active
camera.

**Frame order** (`Engine.GameLoop`): escape check, stopwatch restart, `StaticUpdater`
update actions, `renderDevice.Draw()`, per-component `Update`, physics timestep, deltaTime
measurement. `deltaTime` is the *previous* frame's duration.

**Startup order matters.** The scene is created, then physics, then input, then every
component's `Start`, and only then is `RenderDevice` constructed. The shader is compiled
lazily on the first frame, because the primitive counts per type must be final before the
HLSL is generated. `RaymarchRenderer<T>` throws from `OnAddedToGameObject` if
`Engine.RenderDevice` already exists, so renderers can only be added during `Start`.

**Shader constant injection.** `Shader.CompileFromFiles(@"Shaders\Raymarch")` compiles every
stage file that exists in the folder (`Vertex.hlsl`, `Hull.hlsl`, `Domain.hlsl`,
`Geometry.hlsl`, `Pixel.hlsl`), each with entry point `main`. `HLSLFileIncludeHandler`
resolves `#include`. The virtual include `RaymarchEngine` is not a file: the handler
synthesizes a `static const int <type>Count` per primitive type by counting the
`RaymarchRenderer<T>` components in `Scene.CurrentScene`. That has to stay the same source
`RenderDevice.Draw` uploads from, or the baked count and the structured buffer disagree and
the shader loop reads past the end. If you add a primitive type, emit its count in
`HLSLFileIncludeHandler.GetShaderConstantsStream()`, give it a `StructuredBuffer` register
in `Common.hlsl`, upload it in `RenderDevice.Draw`, and loop over it in `getDist`.
Shaders are copied to the output directory as `Content` with
`CopyToOutputDirectory=Always`, so they are read from disk at runtime and can be edited
without rebuilding the C#.

**GPU data.** `PrimitiveBufferData` and `MaterialBufferData` are
`[StructLayout(LayoutKind.Sequential)]` structs mirrored by HLSL `cbuffer` and
`StructuredBuffer` declarations. Any field change must be mirrored on both sides, with HLSL
16-byte packing rules respected. Rotation travels as a quaternion, and `getDist` rotates the
sample point by its conjugate to put the point in each primitive's local frame, so the signed
distance functions themselves stay axis aligned and receive an already-local point.

## Conventions

- Formatting: 4-space indentation in C#, Allman braces, one type per file matching the
  file name. There is no `.editorconfig`; match the surrounding file.
- Namespaces follow the directory structure under the `RaymarchEngine` root namespace.
- Fields are `camelCase` and private; public access goes through expression-bodied
  properties (`public static RenderDevice RenderDevice => renderDevice;`).
- Public types and members carry XML doc comments; `<inheritdoc />` on interface
  implementations. Keep this up when adding public API.
- Files often open with a `// Created by Sakri Koskimies (Github: Saggre) on <date>` header.
  Preserve existing headers, do not invent them for new files.
- `System.Numerics.Plane` and `System.Numerics.Vector*` collide with `SharpDX` and the
  engine's own types. Existing files disambiguate with `using` aliases at the top
  (`using Plane = RaymarchEngine.Core.Primitives.Plane;`). Follow that pattern rather than
  fully qualifying at each use site.
- `TODO` comments mark known gaps. Leave them unless you actually resolve them.
- HLSL: `MAX_STEPS`, `SURF_DIST` and other tunables live in `Shaders/Raymarch/Options.hlsl`.
  Shader-side structs are prefixed with `c` (`cRay`, `cMaterial`, `cRaymarchResult`).

## Build and run

Requires Windows with a DirectX 11 / Shader Model 5 capable GPU. This is a
`packages.config` project, so restore writes into a local `packages/` folder and the
`.csproj` references assemblies from there by `HintPath`. Building needs MSBuild with the
Roslyn compiler and the .NET Framework 4.8 targeting pack. The legacy
`C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe` cannot build this project: it
predates Roslyn and only understands C# 5.

    nuget restore RaymarchEngine.sln
    msbuild RaymarchEngine.sln /p:Configuration=Debug /p:Platform=x64
    bin\x64\Debug\RaymarchEngine.exe

Every configuration has its own output directory: `Debug|x86` (the project default) writes to
`bin\Debug\`, `Debug|AnyCPU` to `bin\AnyCPU\Debug\`, the x64 pair to `bin\x64\Debug\` and
`bin\x64\Release\`, and `Release|x86` to `bin\x86\Release\`.

`PlatformTarget` is x64 everywhere except `Release|x86`, including in the configurations whose
solution platform is named x86 or AnyCPU. That is deliberate: BepuPhysics runs its solver on
`System.Numerics.Vector<T>`, which .NET Framework only hardware-accelerates on x64. Do not
"correct" a config to 32-bit to match its name.

Running opens a maximized borderless window on the primary screen. Escape quits, WASD and
the mouse move the camera. There is no headless mode: the `Engine` constructor creates a
WinForms window and a D3D11 device, so it cannot run without a GPU and an interactive
desktop session.

## CI and versioning

Two long lived branches. `master` is where development lands, `release` is what ships.

    feature/* -> master -> release -> GitHub release

`.github/workflows/build.yml` builds `Release|x64` on pull requests and on pushes to either
branch. Only a push to `release` publishes; master builds leave the zip as a run artifact and
create no release.

The version comes from GitVersion (`GitVersion.yml`). The branch roles are inverted relative to
GitVersion's defaults, which treat master as the release branch, so both branches have explicit
entries. `release` uses mode `ContinuousDeployment` for a clean `0.0.2`, and `master` uses
`ContinuousDelivery` with an `alpha` label for `0.0.2-alpha.43`. The mode has to be set per
branch: `ContinuousDeployment` drops the pre-release label, which is right for `release` and
wrong for `master`.

Every commit between two tags resolves to the same version. Publishing creates the tag, and that
is what moves the next build on. No back-merge from `release` to `master` is needed; GitVersion
picks up release tags on `master` regardless.

This is a non SDK style project, so there is no `<Version>` property to set. The workflow runs
`build/Stamp-AssemblyInfo.ps1` to write the version into `Properties/AssemblyInfo.cs` before
building. GitVersion's own `/updateassemblyinfo` is not used, because the CLI restores the file
when it exits and the build step that follows would see the original values.

The packaged zip carries the DLLs and the `Shaders` folder alongside the exe. Dropping either
breaks the app at runtime rather than at build time, so the package step asserts both are present.

## Tests

`RaymarchEngineTests` is NUnit 3 and only covers pure logic (`GameObject` hierarchy). It has
a project reference to the engine, so it pulls in SharpDX and Bepu but never creates a
device. Do not add tests that construct `Engine`, `RenderDevice` or a device-dependent
component; those need a GPU and a window.

Do not run the test suite unless asked.

    msbuild RaymarchEngineTests\RaymarchEngineTests.csproj /t:Restore;Build
    vstest.console RaymarchEngineTests\bin\Debug\net48\RaymarchEngineTests.dll

## Gotchas

- Adding a `PackageReference` does not work in the engine project. New dependencies go into
  `packages.config` **and** as a `<Reference>` with a `HintPath` in `RaymarchEngine.csproj`,
  and may need a `bindingRedirect` in `app.config`.
- The `.csproj` lists files explicitly. A new `.cs` file must be added as `<Compile Include>`
  and a new `.hlsl` file as `<Content Include>` with `CopyToOutputDirectory=Always`, or it is
  silently left out of the build.
- Shader compile errors surface at runtime as a `SharpDX.CompilationException` on the first
  frame, not at build time.
- The shaders can only be built with `fxc` (which is what `SharpDX.D3DCompiler` wraps). The
  primitive system uses HLSL `interface` and `class`, and DXC dropped interface support when it
  moved to Shader Model 6, so switching compilers means rewriting `Common.hlsl` first.
- `Engine`, `Scene.CurrentScene`, `InputDevice`, `PhysicsHandler.Simulation` and
  `RaymarchRenderer`'s counts are all static, single-instance global state. Initialization
  order is load-bearing.
- Physics runs on the render thread inside `GameLoop`, not on its own loop.
