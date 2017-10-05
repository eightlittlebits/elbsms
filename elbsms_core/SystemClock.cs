using System.Runtime.CompilerServices;


namespace elbsms_core
{
    class SystemClock
    {
        public ulong Timestamp;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddCycles(uint cycleCount)
        {
            Timestamp += cycleCount;
        }
    }
}
