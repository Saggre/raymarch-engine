// Created by Sakri Koskimies (Github: Saggre) on 24/10/2019

using WindowsInput;
using WindowsInput.Native;

namespace EconSim.Core.Input
{
    /// <summary>
    /// A wrapper for whatever keyboard manager is used
    /// </summary>
    public class Keyboard
    {
        private static InputSimulator inputSimulator;

        public Keyboard()
        {
            if (inputSimulator != null)
            {
                return;
            }

            inputSimulator = new InputSimulator();
        }

        public bool IsKeyDown(VirtualKeyCode keyCode)
        {
            return inputSimulator.InputDeviceState.IsKeyDown(keyCode);
        }

    }
}