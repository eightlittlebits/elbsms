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
        public GameMediaHeader Header { get; }

        public static GameMedia LoadFromFile(string filename, GameMediaType mediaType)
        {
            byte[] fileData;

            using (FileStream file = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                // some roms have a 512 (0x200) byte header left by the dumper/copier. A normal ROM
                // will have a length a multiple of 0x4000. If we mod with 0x4000 and have a 512 byte 
                // remainder then skip the first 512 bytes of the rom.
                if ((file.Length % 0x4000) == 0x200)
                {
                    fileData = new byte[file.Length - 0x200];
                    file.Seek(0x200, SeekOrigin.Begin);
                }
                else
                {
                    /* Normal ROM */
                    fileData = new byte[file.Length];
                }

                file.Read(fileData, 0, fileData.Length);
            }

            return new GameMedia(fileData, mediaType);
        }

        private GameMedia(byte[] romData, GameMediaType mediaType)
        {
            _romData = romData;
            MediaType = mediaType;
            Header = new GameMediaHeader(romData);
        }

        internal byte ReadByte(ushort address)
        {
            return _romData[address];
        }

        internal void WriteByte(ushort _)
        {
            return;//throw new NotImplementedException();
        }
    }
}