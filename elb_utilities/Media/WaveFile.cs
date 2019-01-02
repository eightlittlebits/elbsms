using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace elb_utilities.Media
{
    using DWORD = UInt32;
    using WORD = UInt16;

    public class WaveFile
    {
        private const DWORD ChunkID_Riff = 0x46464952; // FourCC('R', 'I', 'F', 'F');
        private const DWORD RiffForm_Wave = 0x45564157; // FourCC('W', 'A', 'V', 'E');

        private const DWORD ChunkID_Fmt = 0x20746d66; //  FourCC('f', 'm', 't', ' ');
        private const DWORD ChunkID_Data = 0x61746164; // FourCC('d', 'a', 't', 'a');

        static DWORD FourCC(char a, char b, char c, char d)
        {
            return (uint)(a << 0 | b << 8 | c << 16 | d << 24);
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct RiffChunk
        {
            public uint ChunkID;
            public uint ChunkSize;
        }

        public enum WaveFormat : WORD
        {
            PCM = 0x0001
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct PCMWaveFormat
        {
            public WaveFormat FormatTag;
            public WORD Channels;
            public DWORD SamplesPerSec;
            public DWORD AvgBytesPerSec;
            public WORD BlockAlign;
            public WORD BitsPerSample;
        }

        public PCMWaveFormat Format { get; }
        public byte[] SampleData { get; }

        private WaveFile(PCMWaveFormat format, byte[] sampleData)
        {
            Format = format;
            SampleData = sampleData;
        }

        public static WaveFile LoadFromFile(string path)
        {
            PCMWaveFormat format = default;
            byte[] sampleData = null;

            ref var foo = ref format;

            using (var reader = new BinaryReader(new FileStream(path, FileMode.Open, FileAccess.Read)))
            {
                while (reader.BaseStream.Position != reader.BaseStream.Length)
                {
                    RiffChunk riffChunk = ReadRiffChunk(reader);

                    switch (riffChunk.ChunkID)
                    {
                        case ChunkID_Riff:
                            // read the RIFF form type to check we're loading a wave file
                            var formType = reader.ReadUInt32();
                            if (formType != RiffForm_Wave)
                            {
                                throw new InvalidDataException("RIFF form does not match 'WAVE'");
                            }
                            break;

                        case ChunkID_Fmt:
                            format = reader.ReadStruct<PCMWaveFormat>();
                            if (format.FormatTag != WaveFormat.PCM)
                            {
                                throw new InvalidDataException("Format tag does not match WAVE_FORMAT_PCM (1)");
                            }

                            // if the size of the header is larger than the struct then read the extended size and 
                            // skip that
                            WORD extendedSize = 0;
                            if (riffChunk.ChunkSize > Marshal.SizeOf<PCMWaveFormat>() && (extendedSize = reader.ReadUInt16()) > 0)
                            {
                                reader.BaseStream.Seek(extendedSize, SeekOrigin.Current);
                            }
                            break;

                        case ChunkID_Data:
                            sampleData = reader.ReadBytes((int)riffChunk.ChunkSize);
                            break;

                        default:
                            // skip over this chunk if we don't recognise it
                            Debug.WriteLine("Unrecognised chunk ID 0x{0:X2} '{1}'", riffChunk.ChunkID, Encoding.ASCII.GetString(BitConverter.GetBytes(riffChunk.ChunkID)));
                            reader.BaseStream.Seek(riffChunk.ChunkSize, SeekOrigin.Current);
                            break;
                    }
                }
            }

            return new WaveFile(format, sampleData);
        }

        private static RiffChunk ReadRiffChunk(BinaryReader reader)
        {
            return reader.ReadStruct<RiffChunk>();
        }
    }

    static class BinaryReaderExtensions
    {
        public static T ReadStruct<T>(this BinaryReader reader) where T : struct
        {
            byte[] data = reader.ReadBytes(Marshal.SizeOf<T>());

            var handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            try
            {
                return Marshal.PtrToStructure<T>(handle.AddrOfPinnedObject());
            }
            finally
            {
                handle.Free();
            }
        }
    }
}
