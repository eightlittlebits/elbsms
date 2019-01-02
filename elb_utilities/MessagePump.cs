using System;
using System.Windows.Forms;

namespace elb_utilities
{
    public class MessagePump
    {
        private Action _idleLoop;
        private bool _initialised;
        private bool _running;

        public static bool ApplicationStillIdle => !NativeMethods.User32.PeekMessage(out _, IntPtr.Zero, 0, 0, 0);

        public void RunWhileIdle(Action idleLoop)
        {
            _idleLoop = idleLoop;
            _initialised = true;
            _running = true;

            Application.Idle += OnIdle;
        }

        public void Pause() => _running = false;

        public void Resume() => _running = _initialised;

        public void Stop()
        {
            if (_initialised)
            {
                _running = false;
                _initialised = false;

                Application.Idle -= OnIdle;
            }
        }

        private void OnIdle(object sender, EventArgs e)
        {
            while (_running && ApplicationStillIdle)
            {
                _idleLoop();
            }
        }
    }
}
