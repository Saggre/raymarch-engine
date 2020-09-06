// Created by Sakri Koskimies (Github: Saggre) on 02/10/2019

using System.Drawing;
using System.Numerics;
using System.Windows.Forms;
using SharpDX.Windows;
using WinMouse = System.Windows.Input.Mouse;

namespace RaymarchEngine.Core.Input
{
    public class Mouse : AutoUpdateable // TODO this class is a mess
    {
        /// <summary>
        /// FPS or menu mouse
        /// </summary>
        public enum MouseMode
        {
            Infinite = 0,
            Constrained = 1
        }

        private Vector2 position;
        private Vector2 deltaPosition;
        public MouseMode mouseMode;
        private readonly int screenX;
        private readonly int screenY;
        private readonly int screenHalfX;
        private readonly int screenHalfY;

        private RenderForm renderForm;

        private Point lastCursorPosition;

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
        }

        public void HideCursor()
        {
            Cursor.Hide();
        }

        public void ShowCursor()
        {
            Cursor.Show();
        }

        public Vector2 DeltaPosition => deltaPosition;

        /// <summary>
        /// Current cursor position
        /// </summary>
        public Vector2 Position => position;

        /// <summary>
        /// Set cursor to a certain pixel on screen
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void SetCursorPosition(int x, int y)
        {
            Cursor.Position = new Point(x, y);
        }

        /// <summary>
        /// Set cursor to a point on screen between [0,1]
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
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
            //WinMouse.GetPosition()
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