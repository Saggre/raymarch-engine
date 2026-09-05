using System.Numerics;
using BepuPhysics;
using BepuPhysics.Collidables;
using RaymarchEngine.Core;

namespace RaymarchEngine.Physics
{
    /// <summary>
    /// Gives a GameObject a body in the physics simulation, so rays can find it and the player can
    /// walk into it.
    ///
    /// The body is kinematic and follows the GameObject rather than driving it. The scene animates
    /// its objects directly, the torus tumbles and the box turns, so a body that moved under its
    /// own physics would drift away from the shape being drawn.
    ///
    /// The factory methods exist so the rest of the engine can describe a collider without
    /// referencing BepuPhysics types.
    /// </summary>
    public abstract class PrimitiveCollider : IComponent
    {
        private GameObject parent;
        private BodyHandle handle;
        private bool added;

        /// <summary>
        /// A sphere collider, which also serves an octahedron: the circumscribing sphere touches
        /// all six of its vertices.
        /// </summary>
        public static PrimitiveCollider Sphere(float radius)
        {
            return new PrimitiveCollider<Sphere>(new Sphere(radius));
        }

        /// <summary>
        /// A box collider, given the half extents a primitive's scale is expressed in
        /// </summary>
        public static PrimitiveCollider Box(Vector3 halfExtents)
        {
            return new PrimitiveCollider<Box>(new Box(halfExtents.X * 2f, halfExtents.Y * 2f, halfExtents.Z * 2f));
        }

        /// <summary>
        /// A cylinder collider, which also serves a torus laid flat: the cylinder that contains it
        /// has the major plus the minor radius, and twice the minor radius of height.
        /// </summary>
        public static PrimitiveCollider Cylinder(float radius, float halfHeight)
        {
            return new PrimitiveCollider<Cylinder>(new Cylinder(radius, halfHeight * 2f));
        }

        /// <summary>
        /// Registers the shape with the simulation and hands back its index
        /// </summary>
        protected abstract TypedIndex AddShape();

        /// <inheritdoc />
        public void OnAddedToGameObject(GameObject gameObject)
        {
            parent = gameObject;
        }

        /// <inheritdoc />
        public void Start(int startTime)
        {
            // The simulation is published from a callback during engine construction, so this
            // cannot happen any earlier than Start
            if (PhysicsHandler.Simulation == null)
            {
                return;
            }

            handle = PhysicsHandler.Simulation.Bodies.Add(BodyDescription.CreateKinematic(
                new RigidPose(parent.Movement.Position, parent.Movement.Rotation),
                new CollidableDescription(AddShape(), 0.1f),
                new BodyActivityDescription(0.01f)));

            added = true;
        }

        /// <inheritdoc />
        public void Update(float deltaTime)
        {
            if (!added)
            {
                return;
            }

            // A sleeping body leaves the active set, and the broad phase then stops following the
            // pose being written to it. Keeping the body awake is what keeps the collider on the
            // shape it belongs to.
            BodyReference body = PhysicsHandler.Simulation.Bodies.GetBodyReference(handle);
            body.Awake = true;
            body.Pose.Position = parent.Movement.Position;
            body.Pose.Orientation = parent.Movement.Rotation;
        }

        /// <inheritdoc />
        public void End(int endTime)
        {
        }
    }

    /// <summary>
    /// Generic because BepuPhysics stores shapes unboxed and needs the concrete type to do it
    /// </summary>
    /// <typeparam name="TShape">A BepuPhysics convex shape</typeparam>
    internal class PrimitiveCollider<TShape> : PrimitiveCollider where TShape : unmanaged, IConvexShape
    {
        private readonly TShape shape;

        public PrimitiveCollider(TShape shape)
        {
            this.shape = shape;
        }

        /// <inheritdoc />
        protected override TypedIndex AddShape()
        {
            return PhysicsHandler.Simulation.Shapes.Add(shape);
        }
    }
}
