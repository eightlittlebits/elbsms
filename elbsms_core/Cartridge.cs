using System.IO;

namespace elbsms_core
{
    public class Cartridge
    {
        private byte[] _romData;

        public static Cartridge LoadFromFile(string filename)
        {
            byte[] fileData = File.ReadAllBytes(filename);

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