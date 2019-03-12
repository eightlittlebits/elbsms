using System.Diagnostics;
using System.Runtime.CompilerServices;


namespace elbsms_core
{
    class SystemClock
    {
        private readonly uint _clockMultiplier;

        public ulong Timestamp;

        public SystemClock(uint clockMultiplier)
        {
            _clockMultiplier = clockMultiplier;
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddCycles(uint cycleCount)
        {
            Timestamp += cycleCount * _clockMultiplier;
        }
    }
}
