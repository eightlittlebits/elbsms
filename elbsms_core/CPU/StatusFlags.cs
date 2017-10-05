using System;
using System.Runtime.CompilerServices;

namespace elbsms_core.CPU
{
    [Flags]
    enum StatusFlags : byte
    {
        S  = 0b1000_0000,
        Z  = 0b0100_0000,
        B5 = 0b0010_0000,
        H  = 0b0001_0000,
        B3 = 0b0000_1000,
        PV = 0b0000_0100,
        N  = 0b0000_0010,
        C  = 0b0000_0001
    }

    static class StatusFlagExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool FlagSet(this StatusFlags f, StatusFlags flag)
        {
            return (f & flag) == flag;
        }
    }
}