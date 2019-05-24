using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security;

namespace elbsms_ui.NativeMethods
{
    [SuppressUnmanagedCodeSecurity]
    internal static class User32
    {
        [StructLayout(LayoutKind.Sequential)]
        internal struct Message
        {
            public IntPtr hWnd;
            public uint msg;
            public IntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public Point p;
        }

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool PeekMessage(out Message msg, IntPtr hWnd, uint messageFilterMin, uint messageFilterMax, uint flags);
    }
}
