using System;
using System.IO;
using elb_utilities.Media;
using elbemu_shared.Audio;

namespace elbsms_ui.Audio
{
    internal class WaveFileAudioDevice : IAudioDevice
    {
        private bool _disposed = false;
        private readonly WaveFileWriter _waveFile;

        public int SampleRate { get; }

        public WaveFileAudioDevice(IntPtr windowHandle, int sampleRate)
        {
            SampleRate = sampleRate;

            _waveFile = new WaveFileWriter(File.Open("waveout.wav", FileMode.Create),
                                            new WaveFormat((uint)sampleRate, 2));
        }

        public void Play() { }

        public void AddSample(AudioFrame<short> frame)
        {
            _waveFile.WriteSample(frame.Left);
            _waveFile.WriteSample(frame.Right);
        }

        public void Stop() { }

        public void QueueAudio()
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
