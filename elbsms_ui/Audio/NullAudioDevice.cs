﻿using System;
using elbemu_shared.Audio;

namespace elbsms_ui.Audio
{
    internal class NullAudioDevice : IAudioDevice
    {
        public int SampleRate { get; }

        public NullAudioDevice(IntPtr _, int sampleRate)
        {
            SampleRate = sampleRate;
        }

        public void Play() { }

        public void AddSample(AudioFrame<short> frame) { }

        public void Stop() { }

        public void QueueAudio() { }

        public void Dispose() { }
    }
}
