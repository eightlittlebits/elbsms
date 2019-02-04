using System;

namespace elbemu_shared
{
    public interface IAudioDevice : IDisposable
    {
        void Initialise(IntPtr windowHandle, int sampleRate, int channels);
        void AddSample(short left, short right);
        void RenderAudio();
    }
}
