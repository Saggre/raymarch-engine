// Created by Sakri Koskimies (Github: Saggre) on 02/10/2019

using System.Drawing;
using System.Numerics;
using System.Windows.Forms;
using SharpDX.Windows;

namespace RaymarchEngine.Core.Input
{
    /// <summary>
    /// Tracks the cursor and reports how far it moved since the previous frame
    /// </summary>
    public class Mouse : AutoUpdateable // TODO this class is a mess
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

        private RenderForm renderForm;

        private Point lastCursorPosition;

        /// <summary>
        /// Centers the cursor and takes a first reading, so the first frame reports no movement
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

            SetCursorCenter();
            Update(0); // Do first update manually to prevent mouse jump at start

            // That update measured its delta against an unset lastCursorPosition, so the stored
            // delta is the whole distance from the screen origin. Reading it first jerks the camera.
            deltaPosition = Vector2.Zero;
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
        /// Pixels the cursor moved during the previous frame
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

        /// <inheritdoc />
        public override void Start(int startTime)
        {
        }

        /// <inheritdoc />
        public override void Update(float deltaTime)
        {
            // TODO for smoother input render sharpdx to a custom windows form by changing swapchain handle
            Point cursorPosition = Cursor.Position;
            
            position.X = cursorPosition.X;
            position.Y = cursorPosition.Y;

            deltaPosition.X = cursorPosition.X - lastCursorPosition.X;
            deltaPosition.Y = cursorPosition.Y - lastCursorPosition.Y;

            // Center cursor
            if (mouseMode == MouseMode.Infinite)
            {
                SetCursorCenter();
            }

            lastCursorPosition = Cursor.Position;
        }

        /// <inheritdoc />
        public override void End(int endTime)
        {
        }
    }
}