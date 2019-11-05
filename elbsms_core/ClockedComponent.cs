namespace elbsms_core
{
    internal abstract class ClockedComponent
    {
        private readonly SystemClock _clock;
        private ulong _lastUpdate;

        public ClockedComponent(SystemClock clock)
        {
            _clock = clock;
        }

        public void SynchroniseWithSystemClock()
        {
            ulong timestamp = _clock.Timestamp;
            uint cyclesToUpdate = (uint)(timestamp - _lastUpdate);
            _lastUpdate = timestamp;

            Update(cyclesToUpdate);
        }

        // Run this component for the required number of cycles
        public abstract void Update(uint cycleCount);
    }
}