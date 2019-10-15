using System;
using System.Diagnostics;
using elbemu_shared.Audio;
using SharpDX;
using SharpDX.DirectSound;
using SharpDX.Multimedia;

namespace elbsms_ui.Audio
{
    internal class DirectSoundAudioDevice : IAudioDevice
    {
        private bool _disposed = false;
        private DirectSound _directSound;
        private PrimarySoundBuffer _primarySoundBuffer;

        private SecondarySoundBuffer _soundBuffer;
        private int _soundBufferLength;
        private int _soundBufferCursor = -1;

        private readonly AudioFrame<short>[] _sampleBuffer;
        private int _sampleCount;

        public int SampleRate { get; }

        public DirectSoundAudioDevice(IntPtr windowHandle, int sampleRate)
        {
            _sampleBuffer = new AudioFrame<short>[2048];
            _sampleCount = 0;

            SampleRate = sampleRate;

            InitialiseDirectSound(windowHandle, sampleRate);
        }

        private void InitialiseDirectSound(IntPtr windowHandle, int sampleRate)
        {
            var waveFormat = new WaveFormat(sampleRate, 16, 2);

            _directSound = new DirectSound();
            _directSound.SetCooperativeLevel(windowHandle, CooperativeLevel.Priority);

            var primaryBufferDescription = new SoundBufferDescription()
            {
                Flags = BufferFlags.PrimaryBuffer,
                AlgorithmFor3D = Guid.Empty,
            };

            _primarySoundBuffer = new PrimarySoundBuffer(_directSound, primaryBufferDescription) { Format = waveFormat };

            _soundBufferLength = waveFormat.ConvertLatencyToByteSize(500);

            var secondaryBufferDescription = new SoundBufferDescription()
            {
                Format = waveFormat,
                Flags = BufferFlags.GetCurrentPosition2 | BufferFlags.GlobalFocus,
                BufferBytes = _soundBufferLength,
                AlgorithmFor3D = Guid.Empty,
            };

            _soundBuffer = new SecondarySoundBuffer(_directSound, secondaryBufferDescription);
        }

        public void Play() => _soundBuffer.Play(0, PlayFlags.Looping);

        public void AddSample(AudioFrame<short> frame)
        {
            Debug.Assert(_sampleCount < _sampleBuffer.Length);

            _sampleBuffer[_sampleCount++] = frame;
        }

        public void Stop() => _soundBuffer.Stop();

        public unsafe void QueueAudio()
        {
            // the first time through begin writing from the write cursor
            if (_soundBufferCursor == -1)
            {
                _soundBuffer.GetCurrentPosition(out _, out int writeCursor);
                _soundBufferCursor = writeCursor;
            }

            int bytesToWrite = _sampleCount * sizeof(AudioFrame<short>);

            DataStream ds1 = _soundBuffer.Lock(_soundBufferCursor, bytesToWrite, LockFlags.None, out DataStream ds2);

            fixed (AudioFrame<short>* p = _sampleBuffer)
            {
                byte* b = (byte*)p;

                ds1.WriteRange((IntPtr)b, ds1.Length);

                if (ds2 != null)
                {
                    b += ds1.Length;
                    ds2.WriteRange((IntPtr)b, ds2.Length);
                }
            }

            _soundBuffer.Unlock(ds1, ds2);

            _soundBufferCursor = (_soundBufferCursor + bytesToWrite) % _soundBufferLength;
            _sampleCount = 0;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _soundBuffer?.Stop();
                _soundBuffer?.Dispose();

                _primarySoundBuffer?.Dispose();
                _directSound?.Dispose();

                _disposed = true;
            }
        }
    }
}
