// Created by Sakri Koskimies (Github: Saggre) on 05/11/2019

using SharpDX.Windows;

namespace RaymarchEngine.Core.Input
{
    /// <summary>
    /// The engine's single mouse and keyboard, created once at startup
    /// </summary>
    public static class InputDevice
    {
        private static Mouse mouse;
        private static Keyboard keyboard;

        /// <summary>
        /// Reads this frame's input. Called at the top of the frame, before anything that acts on
        /// it, so a look or a step is never a frame behind the hand.
        /// </summary>
        /// <param name="deltaTime">Seconds elapsed since the previous frame</param>
        public static void Update(float deltaTime)
        {
            mouse?.Update(deltaTime);
        }

        /// <summary>
        /// Releases the input devices
        /// </summary>
        public static void Dispose()
        {
            mouse?.Dispose();
            mouse = null;
        }

        /// <summary>
        /// Creates the mouse and keyboard and hides the cursor
        /// </summary>
        /// <param name="renderForm">The window the mouse is centered on</param>
        public static void Init(RenderForm renderForm)
        {
            // Init inputs
            mouse = new Mouse(renderForm);
            mouse.HideCursor();
            keyboard = new Keyboard();
        }

        /// <summary>
        /// The mouse, null until Init has run
        /// </summary>
        public static Mouse Mouse => mouse;

        /// <summary>
        /// The keyboard, null until Init has run
        /// </summary>
        public static Keyboard Keyboard => keyboard;
    }
}