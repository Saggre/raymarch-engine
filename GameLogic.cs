using System.Numerics;
using RaymarchEngine.Core;
using RaymarchEngine.Core.Input;
using RaymarchEngine.Core.Rendering;
using RaymarchEngine.EMath;
using RaymarchEngine.Physics;
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

        private GameObject sphere;

        public override void Start(int startTime)
        {
            // Init movement manager
            playerMovement = new PlayerMovement();

            camera = Scene.CurrentScene.ActiveCamera;
            camera.Movement.Position = new Vector3(0, 2, 0);

            lookVector = new Vector2(180, 180);

            GameObject plane = new GameObject(
                new Vector3(0, -1, 0),
                new RaymarchRenderer<Plane>(),
                new PrimitivePhysics(new BepuPhysics.Collidables.Box(1000f, 0.1f, 1000f), 1, true)
            );

            sphere = new GameObject(
                new Vector3(2, 5, 0),
                new RaymarchRenderer<Sphere>()
            );
            sphere.AddComponent(new PrimitivePhysics(new BepuPhysics.Collidables.Sphere(1f), 10));

            Scene.CurrentScene.AddGameObject(plane);
            Scene.CurrentScene.AddGameObject(sphere);

            //camera.AddComponent(new PrimitivePhysics(new BepuPhysics.Collidables.Sphere(1f), 10));
        }

        public override void Update(float deltaTime)
        {
            CameraLook(deltaTime);

            sphere.GetComponent<PrimitivePhysics>().AddForce(Vector3.UnitX * 0.05f);
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