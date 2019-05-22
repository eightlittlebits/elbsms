using System.IO;

namespace elbsms_core
{
    public enum GameMediaType
    {
        Cartridge,
        SegaCard
    }

    public class GameMedia
    {
        private readonly byte[] _romData;

        public GameMediaType MediaType { get; }

        public static GameMedia LoadFromFile(string filename, GameMediaType mediaType)
        {
            byte[] fileData = File.ReadAllBytes(filename);

            return new GameMedia(fileData, mediaType);
        }

        private GameMedia(byte[] romData, GameMediaType mediaType)
        {
            _romData = romData;
            MediaType = mediaType;
        }

        internal byte ReadByte(ushort address)
        {
            return _romData[address];
        }

        internal void WriteByte(ushort address)
        {
            return;//throw new NotImplementedException();
        }
    }
}