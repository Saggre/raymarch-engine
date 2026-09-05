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