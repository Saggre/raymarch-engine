// Created by Sakri Koskimies (Github: Saggre) on 05/11/2019

using SharpDX.Windows;

namespace RaymarchEngine.Core.Input
{
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

        public static void Init(RenderForm renderForm)
        {
            // Init inputs
            mouse = new Mouse(renderForm);
            mouse.HideCursor();
            keyboard = new Keyboard();
        }

        public static Mouse Mouse => mouse;
        public static Keyboard Keyboard => keyboard;
    }
}