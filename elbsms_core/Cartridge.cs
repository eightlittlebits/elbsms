using System.IO;

namespace elbsms_core
{
    public class Cartridge
    {
        private readonly byte[] _romData;

        internal Cartridge(byte[] romData)
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