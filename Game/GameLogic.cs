// Created by Sakri Koskimies (Github: Saggre) on 25/10/2019

using System.Numerics;
using BepuPhysics.Collidables;
using RaymarchEngine.Core;
using RaymarchEngine.Core.Input;
using RaymarchEngine.Core.Primitives;
using RaymarchEngine.Core.Rendering;
using RaymarchEngine.EMath;
using RaymarchEngine.Physics;
using Plane = RaymarchEngine.Core.Primitives.Plane;
using Sphere = RaymarchEngine.Core.Primitives.Sphere;

namespace RaymarchEngine.Game
{
    /// <summary>
    /// Main class for the game logic separated from the engine itself
    /// </summary>
    public class GameLogic : AutoUpdateable // TODO separate autoupdateable and iupdateable
    {
        private Camera camera;
        private Vector2 lookVector;
        private PlayerMovement playerMovement;

        public override void Start(int startTime)
        {
            // Init movement manager
            playerMovement = new PlayerMovement();

            camera = Engine.CurrentScene.ActiveCamera;
            camera.Position = new Vector3(0, 2, 0);

            lookVector = new Vector2(180, 180);

            GameObject plane = new GameObject(new RaymarchRenderer<Plane>(),
                new PrimitivePhysics(new BepuPhysics.Collidables.Box(1000f, 0.1f, 1000f), 1, true))
            {
                Position = new Vector3(0, -1, 0)
            };

            GameObject sphere = new GameObject(new RaymarchRenderer<Sphere>())
            {
                Position = new Vector3(0, 5, 0)
            };
            sphere.AddComponent(new PrimitivePhysics(new BepuPhysics.Collidables.Sphere(1f), 10));

            Engine.CurrentScene.AddGameObject(plane);
            Engine.CurrentScene.AddGameObject(sphere);

            //camera.AddComponent(new PrimitivePhysics(new BepuPhysics.Collidables.Sphere(1f), 10));
        }


        private void CameraLook(float deltaTime)
        {
            float sensitivity = 0.03f;

            // Move camera
            Vector3 xzInput = new Vector3(playerMovement.MovementInput.X, 0, playerMovement.MovementInput.Z);
            camera.Move(xzInput.Multiply(Engine.CurrentScene.ActiveCamera.Rotation), deltaTime * 8f);
            camera.Move(Vector3.UnitY * playerMovement.MovementInput.Y, deltaTime * 8f);

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

            camera.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, lookVector.X * EMath.Util.Deg2Rad) *
                              Quaternion.CreateFromAxisAngle(Vector3.UnitX, lookVector.Y * EMath.Util.Deg2Rad);
        }

        public override void Update(float deltaTime)
        {
            CameraLook(deltaTime);
        }

        public override void End(int endTime)
        {
        }
    }
}