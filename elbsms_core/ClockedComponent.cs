using System.Runtime.CompilerServices;

namespace elbsms_core
{
    internal abstract class ClockedComponent
    {
        private readonly SystemClock _clock;
        private readonly uint _divisor;
        private ulong _lastUpdate;

        public ClockedComponent(SystemClock clock, uint divisor = 1)
        {
            _clock = clock;
            _divisor = divisor;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SynchroniseWithSystemClock()
        {
            uint masterCyclesElapsed = (uint)(_clock.Timestamp - _lastUpdate);

            // round the master clock cycles to the last multiple of the clock divisor
            uint masterCyclesToUpdate = masterCyclesElapsed - (masterCyclesElapsed % _divisor);

            _lastUpdate += masterCyclesToUpdate;

            Update(masterCyclesToUpdate / _divisor);
        }

        // Run this component for the required number of cycles
        public abstract void Update(uint cycleCount);
    }
}