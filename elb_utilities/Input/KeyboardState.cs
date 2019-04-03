using System.Windows.Forms;

namespace elb_utilities.Input
{
    public readonly struct KeyboardState
    {
        private readonly byte[] _keyboardState;

        internal KeyboardState(byte[] keyboardState)
        {
            _keyboardState = keyboardState;
        }

        public bool IsKeyDown(Keys key) => (_keyboardState[(int)key] & 0x80) == 0x80;
        public bool IsKeyToggled(Keys key) => (_keyboardState[(int)key] & 0x01) == 0x01;
    }
}
