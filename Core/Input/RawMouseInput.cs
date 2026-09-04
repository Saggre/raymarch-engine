using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace RaymarchEngine.Core.Input
{
    /// <summary>
    /// Accumulates unfiltered mouse movement straight from the device, through WM_INPUT.
    ///
    /// Reading Cursor.Position instead loses movement three separate ways: the pointer
    /// acceleration curve is applied first, the result is rounded to whole screen pixels, and
    /// everything that happened during a frame arrives as one sample. Raw input reports every
    /// device report, unaccelerated, which is what makes a mouse look feel attached to the hand.
    ///
    /// This is a message only window rather than a hook on the render form, so it can be created
    /// and destroyed without touching the form's own message handling.
    /// </summary>
    internal sealed class RawMouseInput : NativeWindow, IDisposable
    {
        private const int WmInput = 0x00FF;
        private const int RidInput = 0x10000003;
        private const int RimTypeMouse = 0;

        // Usage page 1 is the generic desktop controls page, usage 2 on it is a mouse
        private const ushort UsagePageGeneric = 0x01;
        private const ushort UsageMouse = 0x02;

        // Set in ButtonFlags when ButtonData carries a wheel delta
        private const ushort RiMouseWheel = 0x0400;

        // Deliver input even when the window is not in the foreground, so a look does not stall
        // when focus is briefly elsewhere
        private const int RidevInputSink = 0x00000100;

        [StructLayout(LayoutKind.Sequential)]
        private struct RawInputDevice
        {
            public ushort UsagePage;
            public ushort Usage;
            public int Flags;
            public IntPtr Target;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RawInputHeader
        {
            public int Type;
            public int Size;
            public IntPtr Device;
            public IntPtr WParam;
        }

        /// <summary>
        /// RAWMOUSE. Reserved stands in for the padding before the button union, which the
        /// unmanaged struct gets from aligning a 4 byte field after a 2 byte one.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct RawMouse
        {
            public ushort Flags;
            public ushort Reserved;
            public ushort ButtonFlags;
            public ushort ButtonData;
            public uint RawButtons;
            public int LastX;
            public int LastY;
            public uint ExtraInformation;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RawInput
        {
            public RawInputHeader Header;
            public RawMouse Mouse;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RegisterRawInputDevices(RawInputDevice[] devices, int deviceCount, int structSize);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetRawInputData(IntPtr rawInput, int command, out RawInput data, ref int size,
            int headerSize);

        private int accumulatedX;
        private int accumulatedY;
        private int accumulatedWheel;

        /// <summary>
        /// Whether the device registered. When it did not, the caller has to fall back to polling
        /// the cursor.
        /// </summary>
        public bool IsAvailable { get; }

        /// <summary>
        /// Starts listening for mouse movement
        /// </summary>
        public RawMouseInput()
        {
            CreateHandle(new CreateParams());

            RawInputDevice[] devices =
            {
                new RawInputDevice
                {
                    UsagePage = UsagePageGeneric,
                    Usage = UsageMouse,
                    Flags = RidevInputSink,
                    Target = Handle
                }
            };

            IsAvailable = RegisterRawInputDevices(devices, devices.Length, Marshal.SizeOf<RawInputDevice>());
        }

        /// <summary>
        /// Movement since the last call, in device counts, and resets the accumulator.
        ///
        /// Reading and clearing together is what makes this frame rate independent: however many
        /// reports arrived while a frame was being drawn, all of them are in the next result.
        /// </summary>
        /// <param name="x">Horizontal movement, positive to the right</param>
        /// <param name="y">Vertical movement, positive downwards</param>
        /// <param name="wheel">Wheel movement, positive away from the hand, in notches of 120</param>
        public void ConsumeMovement(out int x, out int y, out int wheel)
        {
            x = accumulatedX;
            y = accumulatedY;
            wheel = accumulatedWheel;

            accumulatedX = 0;
            accumulatedY = 0;
            accumulatedWheel = 0;
        }

        /// <inheritdoc />
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WmInput)
            {
                int size = Marshal.SizeOf<RawInput>();

                if (GetRawInputData(m.LParam, RidInput, out RawInput input, ref size,
                        Marshal.SizeOf<RawInputHeader>()) > 0 && input.Header.Type == RimTypeMouse)
                {
                    // Absolute mode is what tablets and remote desktop report. The deltas mean
                    // something else there, so leave them to the cursor polling fallback.
                    if ((input.Mouse.Flags & 1) == 0)
                    {
                        accumulatedX += input.Mouse.LastX;
                        accumulatedY += input.Mouse.LastY;
                    }

                    if ((input.Mouse.ButtonFlags & RiMouseWheel) != 0)
                    {
                        // ButtonData is a signed delta in a field declared unsigned, so a scroll
                        // towards the hand arrives as a number just under 65536
                        accumulatedWheel += (short) input.Mouse.ButtonData;
                    }
                }
            }

            base.WndProc(ref m);
        }

        /// <summary>
        /// Stops listening and destroys the message window
        /// </summary>
        public void Dispose()
        {
            DestroyHandle();
        }
    }
}
