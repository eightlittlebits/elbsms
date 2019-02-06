using System.Runtime.InteropServices;

namespace elbemu_shared.Audio
{
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
}
