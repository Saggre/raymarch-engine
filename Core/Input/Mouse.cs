// Created by Sakri Koskimies (Github: Saggre) on 02/10/2019

using System;
using System.Drawing;
using System.Numerics;
using System.Windows.Forms;
using SharpDX.Windows;

namespace RaymarchEngine.Core.Input
{
    /// <summary>
    /// Tracks the cursor and reports how far the mouse moved since the previous frame.
    ///
    /// Not an updateable: the engine drives this at the top of the frame instead. Registering it
    /// alongside the game logic meant whichever registered first ran first, and the game logic
    /// does, so a look was always acting on the previous frame's movement.
    /// </summary>
    public class Mouse : IDisposable
    {
        /// <summary>
        /// FPS or menu mouse
        /// </summary>
        public enum MouseMode
        {
            /// <summary>
            /// The cursor is recentered every frame, so it can keep moving in one direction forever
            /// </summary>
            Infinite = 0,

            /// <summary>
            /// The cursor is left alone and stops at the edge of the screen
            /// </summary>
            Constrained = 1
        }

        private Vector2 position;
        private Vector2 deltaPosition;

        /// <summary>
        /// Whether the cursor is recentered every frame
        /// </summary>
        public MouseMode mouseMode;

        private readonly int screenX;
        private readonly int screenY;
        private readonly int screenHalfX;
        private readonly int screenHalfY;

        private readonly RenderForm renderForm;
        private readonly RawMouseInput rawInput;

        private Point lastCursorPosition;

        /// <summary>
        /// Starts raw input and centers the cursor
        /// </summary>
        /// <param name="renderForm">The window the cursor is centered on</param>
        public Mouse(RenderForm renderForm)
        {
            this.renderForm = renderForm;

            screenX = renderForm.Width;
            screenY = renderForm.Height;
            screenHalfX = screenX / 2;
            screenHalfY = screenY / 2;
            mouseMode = MouseMode.Infinite;

            rawInput = new RawMouseInput();

            SetCursorCenter();
            lastCursorPosition = Cursor.Position;
        }

        /// <summary>
        /// Hides the cursor
        /// </summary>
        public void HideCursor()
        {
            Cursor.Hide();
        }

        /// <summary>
        /// Shows the cursor again after HideCursor
        /// </summary>
        public void ShowCursor()
        {
            Cursor.Show();
        }

        /// <summary>
        /// How far the mouse moved during the previous frame. In device counts when raw input is
        /// available, in screen pixels otherwise.
        /// </summary>
        public Vector2 DeltaPosition => deltaPosition;

        /// <summary>
        /// Current cursor position, in screen pixels
        /// </summary>
        public Vector2 Position => position;

        /// <summary>
        /// Set cursor to a certain pixel on screen
        /// </summary>
        /// <param name="x">Screen x coordinate in pixels</param>
        /// <param name="y">Screen y coordinate in pixels</param>
        public void SetCursorPosition(int x, int y)
        {
            Cursor.Position = new Point(x, y);
        }

        /// <summary>
        /// Set cursor to a point on screen between [0,1]
        /// </summary>
        /// <param name="x">Horizontal position, 0 is the left edge and 1 the right</param>
        /// <param name="y">Vertical position, 0 is the top edge and 1 the bottom</param>
        public void SetCursorPositionRelative(float x, float y)
        {
            Cursor.Position = new Point((int) (screenX * x), (int) (screenY * y));
        }

        /// <summary>
        /// Sets cursor to the center of the game viewport
        /// </summary>
        private void SetCursorCenter()
        {
            SetCursorPosition(renderForm.Left + screenHalfX, renderForm.Top + screenHalfY);
        }

        /// <summary>
        /// Reads the movement that arrived since the previous frame
        /// </summary>
        /// <param name="deltaTime">Seconds elapsed since the previous frame, unused</param>
        public void Update(float deltaTime)
        {
            Point cursorPosition = Cursor.Position;

            position.X = cursorPosition.X;
            position.Y = cursorPosition.Y;

            if (rawInput.IsAvailable)
            {
                rawInput.ConsumeMovement(out int rawX, out int rawY);

                deltaPosition.X = rawX;
                deltaPosition.Y = rawY;
            }
            else
            {
                deltaPosition.X = cursorPosition.X - lastCursorPosition.X;
                deltaPosition.Y = cursorPosition.Y - lastCursorPosition.Y;
            }

            // Recentering is what keeps the cursor inside the window. With raw input the movement
            // no longer comes from the cursor at all, so this cannot swallow any of it.
            if (mouseMode == MouseMode.Infinite)
            {
                SetCursorCenter();
            }

            lastCursorPosition = Cursor.Position;
        }

        /// <summary>
        /// Stops listening for raw input
        /// </summary>
        public void Dispose()
        {
            rawInput?.Dispose();
        }
    }
}
