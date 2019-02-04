using System;
using elbemu_shared;

namespace elbsms_ui.Audio
{
    internal class NullAudioDevice : IAudioDevice
    {
        public void Initialise(IntPtr windowHandle, int sampleRate, int channels)
        {
            
        }

        public void AddSample(short left, short right)
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
