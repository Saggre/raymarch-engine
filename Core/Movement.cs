using System.Numerics;
using RaymarchEngine.EMath;

namespace RaymarchEngine.Core
{
    /// <summary>
    /// A component that let's GameObjects move
    /// </summary>
    public class Movement : IComponent
    {
        private GameObject gameObject;

        private Vector3 position;
        private Quaternion rotation;
        private Vector3 scale;

        private Vector3 deltaPosition;
        private Quaternion deltaRotation;
        private Vector3 deltaScale;

        private Vector3 lastPosition;
        private Quaternion lastRotation;
        private Vector3 lastScale;

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
        /// Create a new movement from params
        /// </summary>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="scale"></param>
        public Movement(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
        }

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
        /// Moves the GameObject
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="speed"></param>
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
        /// <param name="eulerAngles"></param>
        public void Rotate(Vector3 eulerAngles)
        {
            Quaternion eulerRot = eulerAngles.EulerToQuaternion();
            rotation *= eulerRot;
        }

        /// <summary>
        /// Add eulerAngles to the current rotation
        /// </summary>
        public void Rotate(float x, float y, float z)
        {
            Rotate(new Vector3(x, y, x));
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

        public void Start(int startTime)
        {
        }

        public void Update(float deltaTime)
        {
            deltaPosition = position - lastPosition;
            deltaRotation = rotation - lastRotation;
            deltaScale = scale - lastScale;

            foreach (GameObject gameObject in gameObject.Children)
            {
                gameObject.Movement.Move(deltaPosition);
                // TODO
            }

            lastPosition = position;
            lastRotation = rotation;
            lastScale = scale;
        }

        public void End(int endTime)
        {
        }
    }
}