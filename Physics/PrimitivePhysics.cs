using System;
using System.Numerics;
using BepuPhysics;
using BepuPhysics.Collidables;
using RaymarchEngine.Core;
using RaymarchEngine.Core.Primitives;
using Box = BepuPhysics.Collidables.Box;
using Sphere = BepuPhysics.Collidables.Sphere;

namespace RaymarchEngine.Physics
{
    /// <summary>
    /// Enables physics for a raymarch primitive
    /// </summary>
    public class PrimitivePhysics : IComponent
    {
        private readonly float mass;
        private BodyHandle bodyHandle;
        private StaticHandle staticHandle;
        private GameObject parent;
        private readonly bool isStatic;
        private IConvexShape colliderShape;

        /// <summary>
        /// Create from Bepuphysics shape and mass
        /// </summary>
        /// <param name="mass"></param>
        /// <param name="isStatic"></param>
        public PrimitivePhysics(IConvexShape colliderShape, float mass, bool isStatic = false)
        {
            this.colliderShape = colliderShape;
            this.mass = mass;
            this.isStatic = isStatic;
        }

        /// <inheritdoc />
        public void OnAddedToGameObject(GameObject parent)
        {
            this.parent = parent;

            if (colliderShape is Sphere)
            {
                AddBody<Sphere>();
            }
            else if (colliderShape is Box)
            {
                AddBody<Box>();
            }
        }

        /// <summary>
        /// Moves the physics object
        /// </summary>
        public void AddForce(Vector3 force)
        {
            if (isStatic)
            {
                return;
            }

            PhysicsHandler.Simulation.Bodies.GetBodyReference(bodyHandle).ApplyLinearImpulse(force);
        }

        /// <summary>
        /// Rotates the physics object
        /// </summary>
        public void AddAngularForce(Vector3 angularForce)
        {
            if (isStatic)
            {
                return;
            }

            PhysicsHandler.Simulation.Bodies.GetBodyReference(bodyHandle).ApplyAngularImpulse(angularForce);
        }

        private void AddBody<T>() where T : unmanaged, IConvexShape
        {
            T withType = (T) colliderShape;
            colliderShape.ComputeInertia(mass, out BodyInertia inertia);

            if (isStatic)
            {
                staticHandle = PhysicsHandler.Simulation.Statics.Add(
                    new StaticDescription(
                        parent.Movement.Position,
                        new CollidableDescription(
                            PhysicsHandler.Simulation.Shapes.Add(withType),
                            0.1f
                        )
                    )
                );
            }
            else
            {
                bodyHandle = PhysicsHandler.Simulation.Bodies.Add(
                    BodyDescription.CreateDynamic(
                        parent.Movement.Position,
                        inertia,
                        new CollidableDescription(
                            PhysicsHandler.Simulation.Shapes.Add(withType),
                            0.1f
                        ),
                        new BodyActivityDescription(0.01f)
                    )
                );
            }
        }

        /// <inheritdoc />
        public void Start(int startTime)
        {
        }

        /// <inheritdoc />
        public void Update(float deltaTime)
        {
            if (isStatic)
            {
                parent.Movement.Position =
                    PhysicsHandler.Simulation.Statics.GetStaticReference(staticHandle).Pose.Position;
            }
            else
            {
                parent.Movement.Position = PhysicsHandler.Simulation.Bodies.GetBodyReference(bodyHandle).Pose.Position;
            }
        }

        /// <inheritdoc />
        public void End(int endTime)
        {
        }
    }
}