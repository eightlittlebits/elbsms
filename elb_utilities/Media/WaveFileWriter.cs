using System;
using System.IO;
using System.Text;

namespace elb_utilities.Media
{
    public class WaveFileWriter : Stream
    {
        private Stream _outputStream;
        private BinaryWriter _writer;

        private WaveFormat _format;
        private long _dataSizePosition;
        private long _dataSize;

        public override bool CanWrite => true;

        public WaveFileWriter(Stream outputStream, WaveFormat format)
        {
            if (!outputStream.CanWrite)
            {
                throw new ArgumentException("Output stream is not writable.", nameof(outputStream));
            }

            _outputStream = outputStream;
            _writer = new BinaryWriter(outputStream, Encoding.ASCII, true);

            _format = format;

            WriteRiffChunk();
            WriteFormatChunk();
            WriteDataChunkHeader();
        }

        private void WriteRiffChunk()
        {
            _writer.Write(Encoding.ASCII.GetBytes("RIFF"));
            _writer.Write(0); // to be filled in on close   
            _writer.Write(Encoding.ASCII.GetBytes("WAVE"));
        }

        private void WriteFormatChunk()
        {
            _writer.Write(Encoding.ASCII.GetBytes("fmt "));

            int formatChunkSize = 16;
            
            _writer.Write(formatChunkSize);
            _writer.Write((ushort)_format.FormatTag);
            _writer.Write(_format.ChannelCount);
            _writer.Write(_format.SampleRate);

            uint bytesPerSample = ((uint)_format.BitsPerSample + 7) / 8;
            ushort blockAlignment = (ushort)(bytesPerSample * _format.ChannelCount);
            uint bytesPerSecond = blockAlignment * _format.SampleRate;

            _writer.Write(bytesPerSecond);
            _writer.Write(blockAlignment);

            _writer.Write(_format.BitsPerSample);
        }

        private void WriteDataChunkHeader()
        {
            _writer.Write(Encoding.ASCII.GetBytes("data"));
            _dataSizePosition = _outputStream.Position;
            _writer.Write(0); // to be filled in on close
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _writer.Write(buffer, offset, count);
            _dataSize += count;
        }

        public void Write(short[] buffer, int offset, int count)
        {
            for (int i = 0; i < count; i++)
            {
                _writer.Write(buffer[offset + i]);
            }

            _dataSize += (count * 2);
        }

        public void WriteSample(short sample)
        {
            _writer.Write(sample);
            _dataSize += 2;
        }

        public override void Flush()
        {
            _writer.Flush();
        }

        #region IDisposable

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                try
                {
                    UpdateChunkHeaders();
                }
                finally
                {
                    _writer.Dispose();
                    _outputStream.Dispose();
                }
            }
        }

        private void UpdateChunkHeaders()
        {
            Flush();
            UpdateRiffChunk();
            UpdateDataChunk();
        }

        private void UpdateRiffChunk()
        {
            _writer.Seek(4, SeekOrigin.Begin);
            _writer.Write((int)(_outputStream.Length - 8));
        }

        private void UpdateDataChunk()
        {
            _writer.Seek((int)_dataSizePosition, SeekOrigin.Begin);
            _writer.Write((int)_dataSize);
        }

        #endregion

        #region Unsupported Stream Members

        public override bool CanRead => false;

        public override bool CanSeek => false;

        public override long Length => throw new NotSupportedException("This stream does not support seek operations.");

        public override long Position
        {
            get => throw new NotSupportedException("This stream does not support seek operations.");
            set => throw new NotSupportedException("This stream does not support seek operations.");
        }

        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException("Stream does not support reading.");

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException("This stream does not support seek operations.");

        public override void SetLength(long value) => throw new NotSupportedException("This stream does not support seek operations.");
        
        #endregion
    }
}
