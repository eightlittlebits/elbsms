using System.Runtime.InteropServices;
using System.Security;

namespace elbsms_ui.NativeMethods
{
    [SuppressUnmanagedCodeSecurity]
    internal static class WinMM
    {
        [DllImport("winmm.dll", EntryPoint = "timeBeginPeriod", SetLastError = true)]
        internal static extern uint TimeBeginPeriod(uint uMilliseconds);
    }
}
