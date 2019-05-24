using System.ComponentModel;
using System.Runtime.InteropServices;
using elb_utilities.NativeMethods;

namespace elb_utilities.Input
{
    public static class Keyboard
    {
        public static KeyboardState GetState()
        {
            byte[] keys = new byte[256];

            //Get pressed keys
            if (!User32.GetKeyboardState(keys))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            return new KeyboardState(keys);
        }
    }
}
