// Created by Sakri Koskimies (Github: Saggre) on 24/10/2019

using System.Numerics;
using WindowsInput.Native;

namespace RaymarchEngine.Core.Input
{
    /// <summary>
    /// Reads WASD, space and control every frame into a direction the camera can be moved along
    /// </summary>
    public class PlayerMovement : AutoUpdateable
    {
        private Vector3 movementInput;

        /// <summary>
        /// Movement direction from the keys held this frame, in object space. Left shift doubles
        /// its length rather than normalising it, so the length carries the speed.
        /// </summary>
        public Vector3 MovementInput => movementInput;
        
        /// <inheritdoc />
        public override void Start(int startTime)
        {
        }

        /// <inheritdoc />
        public override void Update(float deltaTime)
        {
            movementInput = Vector3.Zero;

            float playerSpeed = 1f;
            if (InputDevice.Keyboard.IsKeyDown(VirtualKeyCode.LSHIFT))
            {
                playerSpeed = 2f;
            }

            if (InputDevice.Keyboard.IsKeyDown(VirtualKeyCode.VK_D))
                movementInput -= Vector3.UnitX * playerSpeed;
            if (InputDevice.Keyboard.IsKeyDown(VirtualKeyCode.VK_A))
                movementInput += Vector3.UnitX * playerSpeed;
            if (InputDevice.Keyboard.IsKeyDown(VirtualKeyCode.VK_W))
                movementInput += Vector3.UnitZ * playerSpeed;
            if (InputDevice.Keyboard.IsKeyDown(VirtualKeyCode.VK_S))
                movementInput -= Vector3.UnitZ * playerSpeed;
            if (InputDevice.Keyboard.IsKeyDown(VirtualKeyCode.SPACE))
                movementInput += Vector3.UnitY * playerSpeed;
            if (InputDevice.Keyboard.IsKeyDown(VirtualKeyCode.LCONTROL))
                movementInput -= Vector3.UnitY * playerSpeed;
        }

        /// <inheritdoc />
        public override void End(int endTime)
        {
        }
    }
}