namespace elb_utilities.Media
{
    public enum WaveFormatTag : ushort
    {
        Pcm = 1
    }

    public class WaveFormat
    {
        private readonly WaveFormatTag _formatTag;
        private readonly ushort _channelCount;
        private readonly uint _sampleRate;
        private readonly uint _averageBytesPerSecond;
        private readonly ushort _blockAlign;
        private readonly ushort _bitsPerSample;

        public WaveFormatTag FormatTag => _formatTag;
        public ushort ChannelCount => _channelCount;
        public uint SampleRate => _sampleRate;
        public uint AverageBytesPerSecond => _averageBytesPerSecond;
        public ushort BlockAlign => _blockAlign;
        public ushort BitsPerSample => _bitsPerSample;

        public static WaveFormat Default => new WaveFormat(44100, 16, 2);

        public WaveFormat(uint sampleRate, ushort channelCount)
            : this(sampleRate, 16, channelCount)
        {

        }

        public WaveFormat(uint sampleRate, ushort bitsPerSample, ushort channelCount)
        {
            _formatTag = WaveFormatTag.Pcm; // PCM (uncompressed)
            _channelCount = channelCount;
            _sampleRate = sampleRate;
            _bitsPerSample = bitsPerSample;

            uint bytesPerSample = ((uint)bitsPerSample + 7) / 8;

            _blockAlign = (ushort)(bytesPerSample * channelCount);
            _averageBytesPerSecond = _blockAlign * sampleRate;
        }
    }
}
