using System;
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
    public class PrimitivePhysics : IUpdateable
    {
        private float mass;
        private BodyHandle bodyHandle;
        private GameObject parent;

        /// <summary>
        /// Create from Bepuphysics shape and mass
        /// </summary>
        /// <param name="mass"></param>
        public PrimitivePhysics(float mass)
        {
            this.mass = mass;
        }

        /// <inheritdoc />
        public void OnAddedToGameObject(GameObject parent)
        {
            this.parent = parent;
            Primitive primitive = (Primitive) parent;

            switch (primitive.GetShapeType())
            {
                case PrimitiveShape.Sphere:
                    AddBody<Sphere>(primitive);
                    break;
                case PrimitiveShape.Plane:
                    AddBody<Box>(primitive);
                    break;
                case PrimitiveShape.Box:
                    AddBody<Box>(primitive);
                    break;
            }
        }

        private void AddBody<T>(Primitive primitive) where T : unmanaged, IConvexShape
        {
            T colliderShape = (T) primitive.GetColliderShape();
            colliderShape.ComputeInertia(1, out BodyInertia inertia);
            TypedIndex t = PhysicsHandler.Simulation.Shapes.Add(colliderShape);

            bodyHandle = PhysicsHandler.Simulation.Bodies.Add(
                BodyDescription.CreateDynamic(
                    primitive.Position,
                    inertia,
                    new CollidableDescription(
                        t,
                        0.1f
                    ),
                    new BodyActivityDescription(0.01f)
                )
            );
        }

        /// <inheritdoc />
        public void Start(int startTime)
        {
        }

        /// <inheritdoc />
        public void Update(float deltaTime)
        {
            parent.Position = PhysicsHandler.Simulation.Bodies.GetBodyReference(bodyHandle).Pose.Position;
        }

        /// <inheritdoc />
        public void End(int endTime)
        {
        }
    }
}