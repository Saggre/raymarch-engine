using System.Numerics;
using RaymarchEngine.Core;
using RaymarchEngine.Core.Input;
using RaymarchEngine.Core.Rendering;
using RaymarchEngine.EMath;
using RaymarchEngine.Physics;
using Box = RaymarchEngine.Core.Primitives.Box;
using Plane = RaymarchEngine.Core.Primitives.Plane;
using Sphere = RaymarchEngine.Core.Primitives.Sphere;

namespace RaymarchEngine
{
    // TODO only RaymarchEngine references should be needed - create wrapper for Bepuphysics
    /// <summary>
    /// Main class for the game logic separated from the engine itself
    /// Used to test how the engine could be used to build a game
    /// </summary>
    public class GameLogic : AutoUpdateable // TODO separate autoupdateable and iupdateable
    {
        private Camera camera;
        private Vector2 lookVector;
        private PlayerMovement playerMovement;
        //private GameObject sphere;

        public override void Start(int startTime)
        {
            // Init movement manager
            playerMovement = new PlayerMovement();

            camera = Scene.CurrentScene.ActiveCamera;
            camera.Movement.Position = new Vector3(0, 2, -5);

            lookVector = new Vector2(180, 180);

            BuildScene();
        }

        /// <summary>
        /// Fills the scene with the objects the raymarch shader renders.
        /// Renderers can only be added here: the shader bakes the per-type counts in on first compile.
        /// </summary>
        private void BuildScene()
        {
            GameObject plane = new GameObject(new Vector3(0, -1, 0));
            plane.AddComponent(new RaymarchRenderer<Plane>
            {
                Color = new Vector3(0.99f, 0.99f, 0.99f),
                Diffraction = 0.7f
            });

            // A sphere's radius comes from Scale.x
            GameObject redSphere = new GameObject(new Vector3(0, 0.5f, 0));
            redSphere.Movement.Scale = new Vector3(0.8f, 0.8f, 0.8f);
            redSphere.AddComponent(new RaymarchRenderer<Sphere>
            {
                Color = new Vector3(0.95f, 0.1f, 0f),
                Shininess = 200f,
                Diffraction = 0.98f
            });

            GameObject blueSphere = new GameObject(new Vector3(2.5f, 1.5f, 2f));
            blueSphere.AddComponent(new RaymarchRenderer<Sphere>
            {
                Color = new Vector3(0f, 0f, 0.99f),
                Shininess = 100f,
                Diffraction = 0.98f
            });

            // A box's Scale is its half extents
            GameObject greenBox = new GameObject(new Vector3(-2f, 0f, 1f));
            greenBox.Movement.Scale = new Vector3(0.8f, 0.8f, 0.8f);
            greenBox.AddComponent(new RaymarchRenderer<Box>
            {
                Color = new Vector3(0f, 0.99f, 0f),
                Diffraction = 0.98f
            });

            Scene.CurrentScene.AddGameObject(plane);
            Scene.CurrentScene.AddGameObject(redSphere);
            Scene.CurrentScene.AddGameObject(blueSphere);
            Scene.CurrentScene.AddGameObject(greenBox);
        }

        public override void Update(float deltaTime)
        {
            CameraLook(deltaTime);

            //sphere.GetComponent<PrimitivePhysics>().AddForce(Vector3.UnitX * 0.05f);
        }

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