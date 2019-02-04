using System;
using System.IO;
using elb_utilities.Media;
using elbemu_shared;

namespace elbsms_ui.Audio
{
    class WaveFileAudioDevice : IAudioDevice
    {
        private bool _disposed = false;
        private WaveFileWriter _waveFile;

        public void Initialise(IntPtr windowHandle, int sampleRate, int channels)
        {
            _waveFile = new WaveFileWriter(File.Open("waveout.wav", FileMode.Create),
                                            new WaveFormat((uint)sampleRate, (ushort)channels));
        }

        public void AddSample(short left, short right)
        {
            _waveFile.WriteSample(left);
            _waveFile.WriteSample(right);
        }

        public void RenderAudio()
        {
            // we're adding directly to the stream in AddSample so nothing to do here
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _waveFile?.Dispose();

                _disposed = true;
            }
        }
    }
}
