// Created by Sakri Koskimies (Github: Saggre) on 24/10/2019

using WindowsInput;
using WindowsInput.Native;

namespace RaymarchEngine.Core.Input
{
    /// <summary>
    /// A wrapper for whatever keyboard manager is used
    /// </summary>
    public class Keyboard
    {
        private static InputSimulator inputSimulator;

        /// <summary>
        /// Creates the shared input simulator, once per process
        /// </summary>
        public Keyboard()
        {
            if (inputSimulator != null)
            {
                return;
            }

            inputSimulator = new InputSimulator();
        }

        /// <summary>
        /// Reads the current state of a key. This is the system wide key state, so it reports
        /// true even when the render window does not have focus.
        /// </summary>
        /// <param name="keyCode">Key to check</param>
        /// <returns>True while the key is held down</returns>
        public bool IsKeyDown(VirtualKeyCode keyCode)
        {
            return inputSimulator.InputDeviceState.IsKeyDown(keyCode);
        }

    }
}