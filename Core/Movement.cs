using System.Numerics;
using RaymarchEngine.EMath;

namespace RaymarchEngine.Core
{
    /// <summary>
    /// A component holding a GameObject's position, rotation and scale. Every GameObject has one,
    /// and moving it carries its children along.
    /// </summary>
    public class Movement : IComponent
    {
        private GameObject gameObject;

        private Vector3 position;
        private Quaternion rotation;
        private Vector3 scale;

        private Vector3 deltaPosition;

        private Vector3 lastPosition;

        /// <summary>
        /// Create a new movement
        /// </summary>
        public Movement()
        {
            position = Vector3.Zero;
            rotation = Quaternion.Identity;
            scale = Vector3.One;
        }

        /// <summary>
        /// Create a new movement from a full transform
        /// </summary>
        /// <param name="position">World space position</param>
        /// <param name="rotation">World space rotation</param>
        /// <param name="scale">Scale along each axis</param>
        public Movement(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
        }

        /// <summary>
        /// How fast the GameObject moved across the ground during the previous frame, in world
        /// units per second.
        ///
        /// Horizontal only. A jump is not a change in how fast someone is moving, and reading the
        /// full vector would make the number leap every time one starts.
        /// </summary>
        public float Speed { get; private set; }

        /// <summary>
        /// Get or set the GameObject's position
        /// </summary>
        public Vector3 Position
        {
            get => position;
            set => position = value;
        }

        /// <summary>
        /// Get or set the GameObject's scale
        /// </summary>
        public Vector3 Scale
        {
            get => scale;
            set => scale = value;
        }

        /// <summary>
        /// Moves the GameObject by direction scaled by speed. The direction is not normalised,
        /// so its length is part of the distance travelled.
        /// </summary>
        /// <param name="direction">Offset to add to the position</param>
        /// <param name="speed">Multiplier applied to the offset</param>
        public void Move(Vector3 direction, float speed = 1f)
        {
            position += direction * speed;
        }

        /// <summary>
        /// The GameObject's rotation in world space
        /// </summary>
        public Quaternion Rotation
        {
            get => rotation;
            set => rotation = value;
        }

        /// <summary>
        /// Add eulerAngles to the current rotation
        /// </summary>
        /// <param name="eulerAngles">Rotation to apply, in degrees per axis</param>
        public void Rotate(Vector3 eulerAngles)
        {
            Quaternion eulerRot = eulerAngles.EulerToQuaternion();
            rotation *= eulerRot;
        }

        /// <summary>
        /// Add a rotation to the current rotation
        /// </summary>
        /// <param name="x">Rotation around the x axis, in degrees</param>
        /// <param name="y">Rotation around the y axis, in degrees</param>
        /// <param name="z">Rotation around the z axis, in degrees</param>
        public void Rotate(float x, float y, float z)
        {
            Rotate(new Vector3(x, y, z));
        }

        /// <summary>
        /// Unit x-vector relative the object
        /// </summary>
        public Vector3 Right => rotation.Multiply(Vector3.UnitX);

        /// <summary>
        /// Unit y-vector relative the object
        /// </summary>
        public Vector3 Up => rotation.Multiply(Vector3.UnitY);

        /// <summary>
        /// Unit z-vector relative the object
        /// </summary>
        public Vector3 Forward => rotation.Multiply(Vector3.UnitZ);

        /// <inheritdoc />
        public void OnAddedToGameObject(GameObject gameObject)
        {
            this.gameObject = gameObject;
        }

        /// <inheritdoc />
        public void Start(int startTime)
        {
        }

        /// <inheritdoc />
        public void Update(float deltaTime)
        {
            deltaPosition = position - lastPosition;

            // The first frame has no previous position and no elapsed time to divide by
            if (deltaTime > 0.0f)
            {
                Speed = new Vector2(deltaPosition.X, deltaPosition.Z).Length() / deltaTime;
            }

            foreach (GameObject gameObject in gameObject.Children)
            {
                gameObject.Movement.Move(deltaPosition);
                // TODO
            }

            lastPosition = position;
        }

        /// <inheritdoc />
        public void End(int endTime)
        {
        }
    }
}