using System.Runtime.InteropServices;

namespace elbsms_core.CPU
{
    [StructLayout(LayoutKind.Explicit)]
    class AFRegisters
    {
        [FieldOffset(0)] public ushort AF;
        [FieldOffset(1)] public byte A;
        [FieldOffset(0)] public StatusFlags F;
    }

    [StructLayout(LayoutKind.Explicit)]
    class GPRegisters
    {
        [FieldOffset(0)] public ushort BC;
        [FieldOffset(1)] public byte B;
        [FieldOffset(0)] public byte C;

        [FieldOffset(2)] public ushort DE;
        [FieldOffset(3)] public byte D;
        [FieldOffset(2)] public byte E;

        [FieldOffset(4)] public ushort HL;
        [FieldOffset(5)] public byte H;
        [FieldOffset(4)] public byte L;
    }
}
