namespace elbsms_core
{
    internal static class Extensions
    {
        internal static bool Bit(this byte v, int bit)
        {
            int mask = 1 << bit;

            return (v & mask) != 0;
        }

        internal static bool Bit(this int v, int bit)
        {
            int mask = 1 << bit;

            return (v & mask) != 0;
        }

        internal static bool EvenParity(this int v)
        {
            v ^= v >> 4;
            v ^= v >> 2;
            v ^= v >> 1;

            return (v & 1) != 1;
        }
    }
}
