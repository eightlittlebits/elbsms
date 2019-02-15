using System;
using System.IO;
using elb_utilities.Media;
using elbemu_shared.Audio;

namespace elbsms_ui.Audio
{
    class WaveFileAudioDevice : IAudioDevice
    {
        private bool _disposed = false;
        private readonly WaveFileWriter _waveFile;

        public WaveFileAudioDevice(IntPtr windowHandle, int sampleRate)
        {
            _waveFile = new WaveFileWriter(File.Open("waveout.wav", FileMode.Create),
                                            new WaveFormat((uint)sampleRate, 2));
        }

        public void AddSample(AudioFrame frame)
        {
            _waveFile.WriteSample(frame.Left);
            _waveFile.WriteSample(frame.Right);
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
