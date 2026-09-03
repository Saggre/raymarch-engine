using System;
using System.Numerics;
using RaymarchEngine.Core;
using RaymarchEngine.Core.Input;
using RaymarchEngine.Core.Rendering;
using RaymarchEngine.EMath;
using Box = RaymarchEngine.Core.Primitives.Box;
using Cylinder = RaymarchEngine.Core.Primitives.Cylinder;
using Ellipsoid = RaymarchEngine.Core.Primitives.Ellipsoid;
using Octahedron = RaymarchEngine.Core.Primitives.Octahedron;
using Plane = RaymarchEngine.Core.Primitives.Plane;
using Sphere = RaymarchEngine.Core.Primitives.Sphere;
using Torus = RaymarchEngine.Core.Primitives.Torus;

namespace RaymarchEngine
{
    // TODO only RaymarchEngine references should be needed - create wrapper for Bepuphysics
    /// <summary>
    /// Main class for the game logic separated from the engine itself
    /// Used to test how the engine could be used to build a game
    /// </summary>
    public class GameLogic : AutoUpdateable // TODO separate autoupdateable and iupdateable
    {
        private const float OrbitRadius = 2.1f;

        private Camera camera;
        private Vector2 lookVector;
        private PlayerMovement playerMovement;

        private float elapsedTime;

        // The objects Update animates. Everything else in the scene is static.
        private GameObject torus;
        private GameObject octahedron;
        private GameObject box;
        private GameObject orbitingSphere;

        private Vector3 octahedronOrigin;
        private Vector3 orbitCentre;

        /// <inheritdoc />
        public override void Start(int startTime)
        {
            // Init movement manager
            playerMovement = new PlayerMovement();

            camera = Scene.CurrentScene.ActiveCamera;

            // Far enough back that the whole row is in frame without the player moving first
            camera.Movement.Position = new Vector3(0, 2.6f, -10.2f);

            lookVector = new Vector2(180, 172);

            BuildScene();
            AnimateScene();
        }

        /// <summary>
        /// Fills the scene with the objects the raymarch shader renders.
        /// Renderers can only be added here: the shader bakes the per-type counts in on first compile.
        /// </summary>
        private void BuildScene()
        {
            // Reflective floor, so every shape above it casts a shadow and shows up twice.
            // Reflections are not occluded by the shadow ray, so at half mirror the sky it picked
            // up washed the shadows out entirely.
            GameObject floor = new GameObject(new Vector3(0, -1, 0));
            floor.AddComponent(new RaymarchRenderer<Plane>
            {
                Color = new Vector3(0.34f, 0.35f, 0.38f),
                Shininess = 90f,
                Diffraction = 0.22f
            });

            // Torus: scale.x is the major radius, scale.y the minor one. Tumbles on two axes.
            torus = new GameObject(new Vector3(-5.4f, 1.3f, 3f));
            torus.Movement.Scale = new Vector3(1.2f, 0.42f, 1f);
            torus.AddComponent(new RaymarchRenderer<Torus>
            {
                Color = new Vector3(0.95f, 0.25f, 0.05f),
                Shininess = 120f,
                Diffraction = 0.45f
            });

            // Octahedron: thin edges make the specular highlight travel as it turns
            octahedronOrigin = new Vector3(-3.1f, 1.5f, 2f);
            octahedron = new GameObject(octahedronOrigin);
            octahedron.Movement.Scale = new Vector3(1.05f, 1.05f, 1.05f);
            octahedron.AddComponent(new RaymarchRenderer<Octahedron>
            {
                Color = new Vector3(0.15f, 0.85f, 0.35f),
                Shininess = 80f,
                Diffraction = 0.85f
            });

            // Box: hard corners make the rotation obvious. Scale is its half extents.
            orbitCentre = new Vector3(-0.2f, 1.1f, 2.4f);
            box = new GameObject(orbitCentre);
            box.Movement.Scale = new Vector3(0.85f, 0.85f, 0.85f);
            box.AddComponent(new RaymarchRenderer<Box>
            {
                Color = new Vector3(0.15f, 0.3f, 0.95f),
                Shininess = 60f,
                Diffraction = 0.35f
            });

            // Small satellite, to show one shape orbiting another
            orbitingSphere = new GameObject(orbitCentre + Vector3.UnitX * OrbitRadius);
            orbitingSphere.Movement.Scale = new Vector3(0.35f, 0.35f, 0.35f);
            orbitingSphere.AddComponent(new RaymarchRenderer<Sphere>
            {
                Color = new Vector3(0.1f, 0.85f, 0.9f),
                Shininess = 250f,
                Diffraction = 0.6f
            });

            // Ellipsoid: non-uniform scale on all three axes
            GameObject ellipsoid = new GameObject(new Vector3(2.7f, 1.1f, 2f));
            ellipsoid.Movement.Scale = new Vector3(1.5f, 0.75f, 1f);
            ellipsoid.AddComponent(new RaymarchRenderer<Ellipsoid>
            {
                Color = new Vector3(0.8f, 0.15f, 0.75f),
                Shininess = 220f,
                Diffraction = 0.2f
            });

            // Cylinder: scale.x is the radius, scale.y the half height
            GameObject cylinder = new GameObject(new Vector3(5.1f, 0.7f, 3f));
            cylinder.Movement.Scale = new Vector3(0.75f, 1.7f, 1f);
            cylinder.AddComponent(new RaymarchRenderer<Cylinder>
            {
                Color = new Vector3(0.95f, 0.75f, 0.2f),
                Shininess = 300f,
                Diffraction = 0.3f
            });

            // Mirror ball behind the row, which reflects everything in front of it
            GameObject chromeSphere = new GameObject(new Vector3(0f, 3.4f, 7.5f));
            chromeSphere.Movement.Scale = new Vector3(1.7f, 1.7f, 1.7f);
            chromeSphere.AddComponent(new RaymarchRenderer<Sphere>
            {
                Color = new Vector3(0.85f, 0.87f, 0.9f),
                Shininess = 400f,
                Diffraction = 0.95f
            });

            Scene.CurrentScene.AddGameObject(floor);
            Scene.CurrentScene.AddGameObject(torus);
            Scene.CurrentScene.AddGameObject(octahedron);
            Scene.CurrentScene.AddGameObject(box);
            Scene.CurrentScene.AddGameObject(orbitingSphere);
            Scene.CurrentScene.AddGameObject(ellipsoid);
            Scene.CurrentScene.AddGameObject(cylinder);
            Scene.CurrentScene.AddGameObject(chromeSphere);
        }

        /// <summary>
        /// Poses the moving objects. Amplitudes stay small so nothing animates out of frame.
        /// </summary>
        private void AnimateScene()
        {
            torus.Movement.Rotation =
                Quaternion.CreateFromAxisAngle(Vector3.UnitY, elapsedTime * 0.7f) *
                Quaternion.CreateFromAxisAngle(Vector3.UnitX, elapsedTime * 1.1f);

            octahedron.Movement.Position =
                octahedronOrigin + Vector3.UnitY * (float) Math.Sin(elapsedTime * 1.4f) * 0.45f;
            octahedron.Movement.Rotation =
                Quaternion.CreateFromAxisAngle(Vector3.UnitY, elapsedTime * 0.9f);

            box.Movement.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, elapsedTime * 0.8f);

            orbitingSphere.Movement.Position = orbitCentre + new Vector3(
                (float) Math.Cos(elapsedTime * 1.6f) * OrbitRadius,
                (float) Math.Sin(elapsedTime * 3.2f) * 0.3f,
                (float) Math.Sin(elapsedTime * 1.6f) * OrbitRadius);
        }

        /// <inheritdoc />
        public override void Update(float deltaTime)
        {
            elapsedTime += deltaTime;

            CameraLook(deltaTime);
            AnimateScene();
        }

        /// <inheritdoc />
        public override void End(int endTime)
        {
        }

        private void CameraLook(float deltaTime)
        {
            float sensitivity = 0.03f;

            // Move camera
            Vector3 xzInput = new Vector3(playerMovement.MovementInput.X, 0, playerMovement.MovementInput.Z);
            camera.Movement.Move(xzInput.Multiply(Scene.CurrentScene.ActiveCamera.Movement.Rotation), deltaTime * 8f);
            camera.Movement.Move(Vector3.UnitY * playerMovement.MovementInput.Y, deltaTime * 8f);

            // Rotate camera
            lookVector.X += InputDevice.Mouse.DeltaPosition.X * sensitivity;
            lookVector.Y -= InputDevice.Mouse.DeltaPosition.Y * sensitivity;

            // Clamp camera rotation
            if (lookVector.Y < 100)
            {
                lookVector.Y = 100;
            }
            else if (lookVector.Y > 260 - float.Epsilon)
            {
                lookVector.Y = 260;
            }

            camera.Movement.Rotation =
                Quaternion.CreateFromAxisAngle(Vector3.UnitY, lookVector.X * EMath.Util.Deg2Rad) *
                Quaternion.CreateFromAxisAngle(Vector3.UnitX, lookVector.Y * EMath.Util.Deg2Rad);
        }
    }
}
