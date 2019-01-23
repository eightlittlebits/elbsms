using System.Diagnostics;
using System.Runtime.CompilerServices;


namespace elbsms_core
{
    class SystemClock
    {
        public ulong Timestamp;

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddCycles(uint cycleCount)
        {
            Timestamp += cycleCount;
        }
    }
}
