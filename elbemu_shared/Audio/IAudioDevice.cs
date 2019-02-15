using System;

namespace elbemu_shared.Audio
{
    public interface IAudioDevice : IDisposable
    {
        void Initialise(IntPtr windowHandle, int sampleRate, int channels);
        void AddSample(AudioFrame frame);
        void RenderAudio();
    }
}
