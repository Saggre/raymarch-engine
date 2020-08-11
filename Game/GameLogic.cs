// Created by Sakri Koskimies (Github: Saggre) on 25/10/2019

using System.Numerics;
using EconSim.Core;
using EconSim.Core.Input;
using EconSim.Core.Primitives;
using EconSim.EMath;
using Plane = EconSim.Core.Primitives.Plane;

namespace EconSim.Game
{
    /// <summary>
    /// Main class for the game logic separated from the engine itself
    /// </summary>
    public class GameLogic : AutoUpdateable
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

            Engine.CurrentScene.AddGameObject(new Sphere()
            {
                Position = new Vector3(2f, 0, 0),
                Radius = 1f
            });

            Engine.CurrentScene.AddGameObject(new Sphere()
            {
                Position = new Vector3(-2f, 0, 0),
                Radius = 1f
            });

            Engine.CurrentScene.AddGameObject(new Plane()
            {
                Position = new Vector3(0, -1, 0)
            });
        }

        private void CameraLook(float deltaTime)
        {
            float sensitivity = 0.03f;

            // Move camera
            camera.Move(playerMovement.MovementInput.Multiply(Engine.CurrentScene.ActiveCamera.Rotation),
                deltaTime * 8f);

            // Rotate camera
            lookVector.X += InputDevice.Mouse.DeltaPosition.X * sensitivity;
            lookVector.Y += InputDevice.Mouse.DeltaPosition.Y * sensitivity;

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

        // TODO move raycast rendering plane out of game logic, it should be engine stuff
        public override void Update(float deltaTime)
        {
            CameraLook(deltaTime);
        }

        public override void End(int endTime)
        {
        }
    }
}