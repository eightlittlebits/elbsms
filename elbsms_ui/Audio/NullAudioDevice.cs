using System;
using elbemu_shared.Audio;

namespace elbsms_ui.Audio
{
    internal class NullAudioDevice : IAudioDevice
    {

        public NullAudioDevice(IntPtr windowHandle, int sampleRate)
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
