using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace elb_utilities.NativeMethods
{
    //public static partial class NativeMethods
    //{
        public static class User32
        {
            [StructLayout(LayoutKind.Sequential)]
            public struct Message
            {
                public IntPtr hWnd;
                public uint msg;
                public IntPtr wParam;
                public IntPtr lParam;
                public uint time;
                public Point p;
            }

            [System.Security.SuppressUnmanagedCodeSecurity]
            [DllImport("User32.dll", CharSet = CharSet.Auto)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool PeekMessage(out Message msg, IntPtr hWnd, uint messageFilterMin, uint messageFilterMax, uint flags);
        }
    //}
}
