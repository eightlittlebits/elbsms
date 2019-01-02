using System.Runtime.InteropServices;

namespace elb_utilities.NativeMethods
{
    //public static partial class NativeMethods
    //{
        public static class WinMM
        {
            [DllImport("winmm.dll", EntryPoint = "timeBeginPeriod", SetLastError = true)]
            public static extern uint TimeBeginPeriod(uint uMilliseconds);
        }
    //}
}
