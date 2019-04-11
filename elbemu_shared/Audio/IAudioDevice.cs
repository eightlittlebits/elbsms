using System;

namespace elbemu_shared.Audio
{
    public interface IAudioDevice : IDisposable
    {
        int SampleRate { get; }

        void Play();
        void AddSample(AudioFrame frame);
        void Stop();

        void QueueAudio();
    }
}
