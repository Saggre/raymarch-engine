// Created by Sakri Koskimies (Github: Saggre) on 05/11/2019

using SharpDX.Windows;

namespace RaymarchEngine.Core.Input
{
    public static class InputDevice
    {
        private static Mouse mouse;
        private static Keyboard keyboard;
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