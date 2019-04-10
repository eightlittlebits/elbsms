using System;

namespace elbemu_shared.Audio
{
    public interface IAudioDevice : IDisposable
    {
        int SampleRate { get; }

        void AddSample(AudioFrame frame);
        void QueueAudio();
    }
}
