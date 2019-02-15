using System;
using elbemu_shared.Audio;

namespace elbsms_ui.Audio
{
    internal class NullAudioDevice : IAudioDevice
    {
        public void Initialise(IntPtr windowHandle, int sampleRate, int channels)
        {

        }

        public void AddSample(AudioFrame frame)
        {

        }

        public void RenderAudio()
        {

        }

        public void Dispose()
        {

        }
    }
}
