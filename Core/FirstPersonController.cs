using System;
using System.Numerics;
using RaymarchEngine.Core.Input;
using RaymarchEngine.EMath;
using RaymarchEngine.Physics;
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
        /// How far the eye sits above the ground it is standing on. This is what fixes the
        /// conversion in MovementUnits.
        /// </summary>
        public float EyeHeight { get; set; } = 64f;

        /// <summary>
        /// Height of the floor. It is an infinite plane, which no convex collider can express, so
        /// it stays a number and everything else in the scene is traced against.
        /// </summary>
        public float GroundHeight { get; set; } = -1f;

        /// <summary>
        /// How far in front of the eye a wall stops the player, in movement units. Stands in for the
        /// width of a character, since this traces a ray rather than sweeping a body.
        /// </summary>
        public float Radius { get; set; } = 16f;

        /// <summary>
        /// How far below the feet to look for something to stand on, in movement units
        /// </summary>
        public float GroundProbe { get; set; } = 4f;

        /// <summary>
        /// Steepest surface that still counts as ground, as the vertical part of its normal.
        /// Anything steeper is a wall, and standing on it would let the player hang off the side
        /// of a shape.
        /// </summary>
        public float MinGroundNormal { get; set; } = 0.5f;

        /// <summary>
        /// Fastest the ground acceleration will drive the player
        /// </summary>
        public float MaxSpeed { get; set; } = 320f;

        /// <summary>
        /// What holding shift multiplies the requested speed by
        /// </summary>
        public float SprintMultiplier { get; set; } = 1.6f;

        /// <summary>
        /// How hard the ground pushes back, per second
        /// </summary>
        public float Friction { get; set; } = 4f;

        /// <summary>
        /// Below this speed friction is applied as though the player were moving at it, which is
        /// what brings someone to a halt in finite time rather than asymptotically.
        /// </summary>
        public float StopSpeed { get; set; } = 100f;

        /// <summary>
        /// Ground acceleration, in multiples of the requested speed per second
        /// </summary>
        public float GroundAcceleration { get; set; } = 10f;

        /// <summary>
        /// Acceleration while airborne
        /// </summary>
        public float AirAcceleration { get; set; } = 10f;

        /// <summary>
        /// The cap that makes air control work.
        ///
        /// Airborne acceleration is measured against this rather than against the full requested
        /// speed, so once moving faster than it there is still headroom to accelerate sideways.
        /// Turning while holding a strafe key then adds speed rather than only redirecting it,
        /// which is where air strafing and bunny hopping come from.
        /// </summary>
        public float AirSpeedCap { get; set; } = 30f;

        /// <summary>
        /// Upward speed a jump starts with. Enough to clear a standard step height.
        /// </summary>
        public float JumpSpeed { get; set; } = 268f;

        /// <summary>
        /// Downward acceleration, about twice the real thing
        /// </summary>
        public float Gravity { get; set; } = 800f;

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
        private Vector3 velocity;
        private bool isGrounded;

        /// <summary>
        /// Height the eye rests at when standing on the ground
        /// </summary>
        private float StandingHeight => GroundHeight + MovementUnits.ToWorld(EyeHeight);

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

        /// <summary>
        /// Classic shooter movement: the keys ask for a direction and a speed, and acceleration
        /// closes the gap between what is asked for and what the player already has.
        ///
        /// The player is never moved at the requested speed directly. Everything is a velocity
        /// that friction takes from and acceleration adds to, which is what produces the ramp up,
        /// the slide when the keys are released, and the air control.
        /// </summary>
        private void UpdateMovement(float deltaTime)
        {
            Vector3 wishDirection = WishDirection();
            float wishSpeed = MovementUnits.ToWorld(MaxSpeed);

            if (InputDevice.Keyboard.IsKeyDown(VirtualKeyCode.LSHIFT))
            {
                wishSpeed *= SprintMultiplier;
            }

            // Before friction, so a jump leaves at full speed instead of the ground taking a
            // frame's worth off it first. It is also the reason a jump
            // taken on the instant of landing keeps its speed.
            if (isGrounded && JumpRequested())
            {
                velocity.Y = MovementUnits.ToWorld(JumpSpeed);
                isGrounded = false;
            }

            if (isGrounded)
            {
                ApplyFriction(deltaTime);
                Accelerate(wishDirection, wishSpeed, GroundAcceleration, deltaTime);
            }
            else
            {
                AirAccelerate(wishDirection, wishSpeed, deltaTime);
            }

            velocity.Y -= MovementUnits.ToWorld(Gravity) * deltaTime;
            velocity = ClipVelocity(parent.Movement.Position, velocity, deltaTime);

            Vector3 position = parent.Movement.Position + velocity * deltaTime;

            // Landing has to clear the vertical velocity, or gravity keeps accumulating into it
            // and the next jump is swallowed
            float floorHeight = FloorHeight(position);
            isGrounded = position.Y <= floorHeight;

            if (isGrounded)
            {
                position.Y = floorHeight;
                velocity.Y = 0f;
            }

            parent.Movement.Position = position;
            parent.Movement.Speed = new Vector2(velocity.X, velocity.Z).Length();
        }

        /// <summary>
        /// Where the eye rests when standing on whatever is directly below it.
        ///
        /// Traced rather than assumed flat, which is what lets the player stand on the shapes
        /// instead of only on the floor. The floor itself is not a collider, so a ray that finds
        /// nothing means open ground.
        /// </summary>
        private float FloorHeight(Vector3 position)
        {
            float eye = MovementUnits.ToWorld(EyeHeight);
            float reach = eye + MovementUnits.ToWorld(GroundProbe);

            if (PhysicsQuery.Raycast(position, -Vector3.UnitY, reach, out float distance, out Vector3 normal) &&
                normal.Y >= MinGroundNormal)
            {
                return position.Y - distance + eye;
            }

            return StandingHeight;
        }

        /// <summary>
        /// Takes the part of the velocity that heads into a wall away from it, and leaves the rest.
        ///
        /// This is clipping, and keeping the remainder is what makes running at a wall
        /// at an angle slide along it rather than stop dead. Clipping the velocity rather than the
        /// step also stops speed piling up against a wall and firing the player off when they turn
        /// away from it.
        /// </summary>
        private Vector3 ClipVelocity(Vector3 position, Vector3 velocity, float deltaTime)
        {
            Vector3 horizontal = new Vector3(velocity.X, 0f, velocity.Z);

            float speed = horizontal.Length();
            if (speed < 0.01f)
            {
                return velocity;
            }

            float reach = speed * deltaTime + MovementUnits.ToWorld(Radius);
            if (!PhysicsQuery.Raycast(position, horizontal / speed, reach, out float _, out Vector3 normal))
            {
                return velocity;
            }

            // A floor or a ceiling is not a wall, and its normal has nothing horizontal to clip
            Vector3 wall = new Vector3(normal.X, 0f, normal.Z);
            if (wall.LengthSquared() < 1e-6f)
            {
                return velocity;
            }

            float into = Vector3.Dot(velocity, Vector3.Normalize(wall));

            return into < 0f ? velocity - Vector3.Normalize(wall) * into : velocity;
        }

        /// <summary>
        /// Whether anything is asking for a jump this frame.
        ///
        /// Either direction of the wheel counts. Binding jump to the wheel is what players do in
        /// classic shooters to bunny hop: a notch produces a single frame of input on landing,
        /// which is far easier to land on the right frame than a key press is.
        /// </summary>
        private bool JumpRequested()
        {
            return InputDevice.Keyboard.IsKeyDown(VirtualKeyCode.SPACE) ||
                   InputDevice.Mouse.WheelDelta != 0;
        }

        /// <summary>
        /// Removes speed at a rate proportional to the current speed, so slowing down is
        /// exponential. Below StopSpeed the rate is held at what StopSpeed would give, which is
        /// what brings the player to an actual halt instead of creeping towards one forever.
        /// </summary>
        private void ApplyFriction(float deltaTime)
        {
            float speed = velocity.Length();
            if (speed < 0.01f)
            {
                velocity = Vector3.Zero;
                return;
            }

            float control = Math.Max(speed, MovementUnits.ToWorld(StopSpeed));
            float drop = control * Friction * deltaTime;

            velocity *= Math.Max(speed - drop, 0f) / speed;
        }

        /// <summary>
        /// Adds speed along the requested direction, but only as much as is missing from the
        /// requested speed in that direction. Already moving that fast means no acceleration,
        /// which caps ground speed without ever clamping the velocity.
        /// </summary>
        private void Accelerate(Vector3 wishDirection, float wishSpeed, float acceleration, float deltaTime)
        {
            float currentSpeed = Vector3.Dot(velocity, wishDirection);
            float addSpeed = wishSpeed - currentSpeed;

            if (addSpeed <= 0f)
            {
                return;
            }

            velocity += wishDirection * Math.Min(acceleration * wishSpeed * deltaTime, addSpeed);
        }

        /// <summary>
        /// The same, except the speed it measures against is capped at AirSpeedCap.
        ///
        /// That one substitution is the whole of air control. Past the cap the player can still
        /// accelerate perpendicular to their travel, so sweeping the mouse while holding a strafe
        /// key curves the path and gains speed rather than trading it.
        /// </summary>
        private void AirAccelerate(Vector3 wishDirection, float wishSpeed, float deltaTime)
        {
            float cappedSpeed = Math.Min(wishSpeed, MovementUnits.ToWorld(AirSpeedCap));

            float currentSpeed = Vector3.Dot(velocity, wishDirection);
            float addSpeed = cappedSpeed - currentSpeed;

            if (addSpeed <= 0f)
            {
                return;
            }

            velocity += wishDirection * Math.Min(AirAcceleration * wishSpeed * deltaTime, addSpeed);
        }

        /// <summary>
        /// Direction the keys are asking to move in, along the ground.
        ///
        /// Built from the yaw alone. Rotating it by the full view direction is what let the old
        /// flying camera walk into the floor whenever it was looking down.
        /// </summary>
        private Vector3 WishDirection()
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

            // Normalised, so holding two keys does not ask for more speed than holding one
            Quaternion heading = Quaternion.CreateFromAxisAngle(Vector3.UnitY, yawDegrees * EMath.Util.Deg2Rad);

            return Vector3.Normalize(input).Multiply(heading);
        }
    }
}
