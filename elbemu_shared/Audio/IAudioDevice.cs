using System;

namespace elbemu_shared.Audio
{
    public interface IAudioDevice : IDisposable
    {
        void AddSample(AudioFrame frame);
        void RenderAudio();
    }
}
