using System.Runtime.InteropServices;

namespace elbemu_shared.Audio
{
    // TODO(david): C# 8 will consider this unmanaged so can be used instead of AudioFrame
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly struct AudioFrame<T> where T : unmanaged
    {
        public readonly T Left;
        public readonly T Right;

        public AudioFrame(T left, T right)
        {
            Left = left;
            Right = right;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly struct AudioFrame
    {
        public readonly short Left;
        public readonly short Right;

        public AudioFrame(short left, short right)
        {
            Left = left;
            Right = right;
        }
    }
}
