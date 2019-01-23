using System.IO;

namespace elbsms_core
{
    public class Cartridge
    {
        private readonly byte[] _romData;

        public static Cartridge LoadFromFile(string filename)
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

            return new Cartridge(fileData);
        }

        private Cartridge(byte[] romData)
        {
            _romData = romData;
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