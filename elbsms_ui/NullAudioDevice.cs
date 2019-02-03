using System;
using elbemu_shared;

namespace elbsms_ui
{
    internal class NullAudioDevice : IAudioDevice
    {
        public void Initialise(IntPtr windowHandle)
        {
            
        }

        public void AddSample(short sample)
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
