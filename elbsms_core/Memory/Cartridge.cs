namespace elbsms_core.Memory
{
    public class Cartridge
    {
        public CartridgeHeader Header { get; }

        private readonly byte[] _romData;

        public Cartridge(byte[] romData)
        {
            _romData = romData;

            Header = new CartridgeHeader(romData);
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