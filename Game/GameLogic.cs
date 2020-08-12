// Created by Sakri Koskimies (Github: Saggre) on 25/10/2019

using System.Diagnostics;
using System.Numerics;
using BepuPhysics;
using BepuPhysics.Collidables;
using RaymarchEngine.Core;
using RaymarchEngine.Core.Input;
using RaymarchEngine.Core.Primitives;
using RaymarchEngine.EMath;
using Plane = RaymarchEngine.Core.Primitives.Plane;
using Sphere = RaymarchEngine.Core.Primitives.Sphere;

namespace RaymarchEngine.Game
{
    /// <summary>
    /// Main class for the game logic separated from the engine itself
    /// </summary>
    public class GameLogic : AutoUpdateable
    {
        private Camera camera;
        private Vector2 lookVector;
        private PlayerMovement playerMovement;

        private Sphere visualSphere;
        private BodyHandle sphereHandle;
        
        public override void Start(int startTime)
        {
            // Init movement manager
            playerMovement = new PlayerMovement();

            camera = Engine.CurrentScene.ActiveCamera;
            camera.Position = new Vector3(0, 2, 0);

            lookVector = new Vector2(180, 180);

            /*Engine.CurrentScene.AddGameObject(new Sphere()
            {
                Position = new Vector3(-2f, 0, 0),
                Radius = 1f
            });*/

            Engine.CurrentScene.AddGameObject(new Plane()
            {
                Position = new Vector3(0, -1, 0)
            });

            Engine.CurrentScene.AddGameObject(new Torus()
            {
                Position = new Vector3(2, 3, 0),
                Dimensions = new Vector2(0.5f, 0.2f)
            });

            Physics.Physics.SetCallback(PhysicsStart);

            Debug.WriteLine("START");
        }

        private void PhysicsStart()
        {
            
            visualSphere = new Sphere()
            {
                Position = new Vector3(0, 5, 0),
                Radius = 1f
            };
            Engine.CurrentScene.AddGameObject(visualSphere);

            var sphere = new BepuPhysics.Collidables.Sphere(visualSphere.Radius);

            sphere.ComputeInertia(1, out var sphereInertia);

            var sphereDescription = BodyDescription.CreateDynamic(visualSphere.Position, sphereInertia,
                new CollidableDescription(Physics.Physics.simulation.Shapes.Add(sphere), 0.1f),
                new BodyActivityDescription(0.01f));

            sphereHandle = Physics.Physics.simulation.Bodies.Add(sphereDescription);

            //sphereDescription.Velocity = new BodyVelocity(new Vector3(0, -1, 0));
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

            if (visualSphere != null)
            {
                visualSphere.Position = Physics.Physics.simulation.Bodies.GetBodyReference(sphereHandle).Pose.Position;
                Debug.WriteLine(Physics.Physics.simulation.Bodies.GetBodyReference(sphereHandle).Pose.Position.ToString());

            }


            Physics.Physics.simulation.Timestep(deltaTime);
        }

        public override void End(int endTime)
        {
        }
    }
}