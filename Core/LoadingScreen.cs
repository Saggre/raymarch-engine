using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace RaymarchEngine.Core
{
    /// <summary>
    /// A splash window shown while the engine starts up.
    ///
    /// It runs on its own thread with its own message loop. Startup is dominated by compiling the
    /// raymarch pixel shader, which takes several seconds and blocks the thread it runs on, so a
    /// splash owned by that thread would freeze and Windows would grey it out as not responding.
    /// Off the main thread it keeps animating no matter how long a step takes.
    ///
    /// The bar is a marquee rather than a percentage. The long step is a single call into the
    /// shader compiler, which reports nothing until it returns, so any percentage during it would
    /// be invented.
    /// </summary>
    public sealed class LoadingScreen : IDisposable
    {
        private readonly Thread thread;
        private readonly ManualResetEventSlim ready = new ManualResetEventSlim(false);

        private Form form;
        private Label statusLabel;

        /// <summary>
        /// Opens the splash and waits until it is on screen, so the first status is never missed
        /// </summary>
        public LoadingScreen()
        {
            thread = new Thread(Run)
            {
                IsBackground = true,
                Name = "LoadingScreen"
            };

            // Windows Forms needs a single threaded apartment to pump messages
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

            ready.Wait();
        }

        /// <summary>
        /// Says what the engine is doing now
        /// </summary>
        /// <param name="status">Short description of the current step</param>
        public void Report(string status)
        {
            if (form == null || form.IsDisposed)
            {
                return;
            }

            try
            {
                form.BeginInvoke(new Action(() => statusLabel.Text = status));
            }
            catch (InvalidOperationException)
            {
                // The window went away between the check and the call, which only means the engine
                // finished loading first
            }
        }

        /// <summary>
        /// Closes the splash and waits for its thread to finish
        /// </summary>
        public void Dispose()
        {
            if (form != null && !form.IsDisposed)
            {
                try
                {
                    form.BeginInvoke(new Action(() => form.Close()));
                }
                catch (InvalidOperationException)
                {
                }
            }

            thread.Join(TimeSpan.FromSeconds(2));
            ready.Dispose();
        }

        private void Run()
        {
            statusLabel = new Label
            {
                Text = "Starting",
                ForeColor = Color.FromArgb(210, 218, 230),
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 10f),
                TextAlign = ContentAlignment.MiddleCenter,
                Bounds = new Rectangle(0, 96, 420, 24)
            };

            Label titleLabel = new Label
            {
                Text = "Raymarch Engine",
                ForeColor = Color.FromArgb(245, 248, 252),
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI Light", 22f),
                TextAlign = ContentAlignment.MiddleCenter,
                Bounds = new Rectangle(0, 36, 420, 44)
            };

            ProgressBar progressBar = new ProgressBar
            {
                Style = ProgressBarStyle.Marquee,
                MarqueeAnimationSpeed = 24,
                Bounds = new Rectangle(60, 136, 300, 6)
            };

            form = new Form
            {
                // Borderless, so this is never drawn, but it is what alt tab and screen capture
                // identify the window by
                Text = "Raymarch Engine Loading",
                FormBorderStyle = FormBorderStyle.None,
                StartPosition = FormStartPosition.CenterScreen,
                ClientSize = new Size(420, 180),
                BackColor = Color.FromArgb(24, 28, 36),
                TopMost = true,
                ShowInTaskbar = false,
                ControlBox = false
            };

            form.Controls.Add(titleLabel);
            form.Controls.Add(statusLabel);
            form.Controls.Add(progressBar);

            // Released once the window actually exists, so Report cannot be called against nothing
            form.Shown += (sender, args) => ready.Set();

            Application.Run(form);
        }
    }
}
