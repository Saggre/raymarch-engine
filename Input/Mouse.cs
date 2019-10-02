// Created by Sakri Koskimies (Github: Saggre) on 02/10/2019

using EconSim.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using IUpdateable = EconSim.Core.IUpdateable;

namespace EconSim.Input
{
    using GameMouse = Microsoft.Xna.Framework.Input.Mouse;

    public class Mouse : IUpdateable
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
        public MouseMode mouseMode;
        private readonly int screenX;
        private readonly int screenY;

        public Mouse()
        {
            StaticUpdater.Add(this);
            mouseMode = MouseMode.Infinite;
            screenX = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            screenY = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
        }

        /// <summary>
        /// Cursor delta position since last update
        /// </summary>
        public Vector2 DeltaPosition { get; private set; }

        /// <summary>
        /// Last cursor position
        /// </summary>
        public Vector2 LastPosition { get; private set; }

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
            GameMouse.SetPosition(x, y);
        }

        /// <summary>
        /// Set cursor to a point on screen between [0,1]
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void SetCursorPositionRelative(float x, float y)
        {
            GameMouse.SetPosition((int)(screenX * x), (int)(screenY * y));
        }

        public void Start()
        {

        }

        public void Update()
        {
            var mouseState = GameMouse.GetState();
            position.X = mouseState.X;
            position.Y = mouseState.Y;

            DeltaPosition = position - LastPosition;

            if (mouseMode == MouseMode.Infinite)
            {
                SetCursorPosition(screenX / 2, screenY / 2);
            }

            LastPosition = position;
        }

        public void End()
        {

        }
    }
}