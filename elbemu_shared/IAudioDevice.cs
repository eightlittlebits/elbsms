using System;

namespace elbemu_shared
{
    public interface IAudioDevice : IDisposable
    {
        void Initialise(IntPtr windowHandle);
        void AddSample(short sample);
        void RenderAudio();
    }
}
