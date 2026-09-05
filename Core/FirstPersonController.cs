using System;
using System.Numerics;
using RaymarchEngine.Core.Input;
using RaymarchEngine.EMath;
using WindowsInput.Native;

namespace RaymarchEngine.Core
{
    /// <summary>
    /// Walks the GameObject it is attached to around on foot: mouse look, WASD along the ground,
    /// and a jump under gravity.
    ///
    /// Replaces the free flying camera. The two differ in more than the vertical axis: flying
    /// moved along the full view direction, so looking down and walking forward drove into the
    /// floor, while this projects movement onto the ground plane the way a character does.
    /// </summary>
    public class FirstPersonController : IComponent
    {
        /// <summary>
        /// How far the eye sits above the ground it is standing on, in world units
        /// </summary>
        public float EyeHeight { get; set; } = 1.7f;

        /// <summary>
        /// Height of the ground plane. The scene has one infinite floor and no collision system,
        /// so the floor is a number rather than something to trace against.
        /// </summary>
        public float GroundHeight { get; set; } = -1f;

        /// <summary>
        /// Walking speed in world units per second
        /// </summary>
        public float WalkSpeed { get; set; } = 6f;

        /// <summary>
        /// What holding shift multiplies the walking speed by
        /// </summary>
        public float SprintMultiplier { get; set; } = 1.9f;

        /// <summary>
        /// Upward speed a jump starts with, in world units per second
        /// </summary>
        public float JumpSpeed { get; set; } = 7f;

        /// <summary>
        /// Downward acceleration in world units per second squared. Well above the real 9.81,
        /// which is the usual trade: a realistic jump hangs long enough to feel floaty.
        /// </summary>
        public float Gravity { get; set; } = 20f;

        /// <summary>
        /// Degrees of rotation per unit of mouse movement
        /// </summary>
        public float LookSensitivity { get; set; } = 0.05f;

        /// <summary>
        /// How far the view can tilt from level, in degrees. Short of 90 so the forward direction
        /// never collapses onto the up axis.
        /// </summary>
        public float MaxPitch { get; set; } = 88f;

        private GameObject parent;

        // Yaw 0 faces +Z, which is where the scene is from the starting position. The old camera
        // reached the same direction with a yaw of 180 and a pitch of 172, because a half turn on
        // each axis cancels out, which made both angles read as arbitrary.
        private float yawDegrees;
        private float pitchDegrees = -8f;
        private float verticalVelocity;
        private bool isGrounded;

        /// <summary>
        /// Height the eye rests at when standing on the ground
        /// </summary>
        private float StandingHeight => GroundHeight + EyeHeight;

        /// <inheritdoc />
        public void OnAddedToGameObject(GameObject gameObject)
        {
            parent = gameObject;
        }

        /// <inheritdoc />
        public void Start(int startTime)
        {
            Vector3 position = parent.Movement.Position;
            parent.Movement.Position = new Vector3(position.X, StandingHeight, position.Z);

            ApplyRotation();
        }

        /// <inheritdoc />
        public void Update(float deltaTime)
        {
            UpdateLook();
            UpdateMovement(deltaTime);
        }

        /// <inheritdoc />
        public void End(int endTime)
        {
        }

        private void UpdateLook()
        {
            Vector2 delta = InputDevice.Mouse.DeltaPosition;

            yawDegrees += delta.X * LookSensitivity;
            pitchDegrees = (pitchDegrees - delta.Y * LookSensitivity).Clamp(-MaxPitch, MaxPitch);

            ApplyRotation();
        }

        /// <summary>
        /// Yaw first, then pitch, so the view tilts around the camera's own right axis rather than
        /// rolling as it turns
        /// </summary>
        private void ApplyRotation()
        {
            // Pitch is negated so that positive means looking up: rotating +Z about +X sends it
            // downwards, not up.
            parent.Movement.Rotation =
                Quaternion.CreateFromAxisAngle(Vector3.UnitY, yawDegrees * EMath.Util.Deg2Rad) *
                Quaternion.CreateFromAxisAngle(Vector3.UnitX, -pitchDegrees * EMath.Util.Deg2Rad);
        }

        private void UpdateMovement(float deltaTime)
        {
            Vector3 position = parent.Movement.Position + GroundVelocity() * deltaTime;

            if (isGrounded && InputDevice.Keyboard.IsKeyDown(VirtualKeyCode.SPACE))
            {
                verticalVelocity = JumpSpeed;
                isGrounded = false;
            }

            verticalVelocity -= Gravity * deltaTime;
            position.Y += verticalVelocity * deltaTime;

            // The only collision in the scene. Landing has to clear the velocity, or gravity keeps
            // accumulating into it and the next jump is swallowed.
            if (position.Y <= StandingHeight)
            {
                position.Y = StandingHeight;
                verticalVelocity = 0f;
                isGrounded = true;
            }

            parent.Movement.Position = position;
        }

        /// <summary>
        /// Movement along the ground from the keys held this frame.
        ///
        /// Built from the yaw alone. Rotating it by the full view direction is what let the old
        /// flying camera walk into the floor whenever it was looking down.
        /// </summary>
        private Vector3 GroundVelocity()
        {
            Vector3 input = Vector3.Zero;

            if (InputDevice.Keyboard.IsKeyDown(VirtualKeyCode.VK_W))
            {
                input += Vector3.UnitZ;
            }

            if (InputDevice.Keyboard.IsKeyDown(VirtualKeyCode.VK_S))
            {
                input -= Vector3.UnitZ;
            }

            if (InputDevice.Keyboard.IsKeyDown(VirtualKeyCode.VK_A))
            {
                input -= Vector3.UnitX;
            }

            if (InputDevice.Keyboard.IsKeyDown(VirtualKeyCode.VK_D))
            {
                input += Vector3.UnitX;
            }

            if (input == Vector3.Zero)
            {
                return Vector3.Zero;
            }

            // Normalised, so holding two keys does not travel faster than holding one
            Quaternion heading = Quaternion.CreateFromAxisAngle(Vector3.UnitY, yawDegrees * EMath.Util.Deg2Rad);
            Vector3 direction = Vector3.Normalize(input).Multiply(heading);

            float speed = WalkSpeed;
            if (InputDevice.Keyboard.IsKeyDown(VirtualKeyCode.LSHIFT))
            {
                speed *= SprintMultiplier;
            }

            return direction * speed;
        }
    }
}
