using System;
using System.Diagnostics;

namespace elbsms_core.Memory
{
    class Interconnect
    {
        MemoryControlRegister _memoryControl;

        private Cartridge _cartridge;
        private readonly byte[] _ram;

        public Interconnect()
        {
            _memoryControl = new MemoryControlRegister();

            _ram = new byte[0x2000];
        }

        internal void LoadCartridge(Cartridge cartridge)
        {
            _cartridge = cartridge;
        }

        internal byte ReadByte(ushort address)
        {
            // 0x0000 -> 0xBFFF - Cartridge
            if (address < 0xC000)
            {
                return _cartridge?.ReadByte(address) ?? 0xFF;
            }
            // 0xC000 -> 0xDFFF - System RAM
            else if (address < 0xE000)
            {
                return _ram[address - 0xC000];
            }
            // 0xE000 -> 0xFFFF - System RAM (mirror)
            else
            {
                return _ram[address - 0xE000];
            }
        }

        internal void WriteByte(ushort address, byte value)
        {
            // 0x0000 -> 0xBFFF - Cartridge
            if (address < 0xC000)
            {
                _cartridge.WriteByte(address);
            }
            // 0xC000 -> 0xDFFF - System RAM
            else if (address < 0xE000)
            {
                _ram[address - 0xC000] = value;
            }
            // 0xE000 -> 0xFFFF - System RAM (mirror)
            else
            {
                _ram[address - 0xE000] = value;
            }
        }

        internal void Out(byte address, byte value)
        {
            Debug.WriteLine($"OUT: 0x{address:X4}, {(char)value}(0x{value:X2})");

            // the port is selected with bits 7, 6 and 0 of the address (8 total available ports)
            var port = ((address & 0xC0) >> 5) | (address & 0x01);

            switch (port)
            {
                // memory control register
                case 0x00:
                    _memoryControl.Value = value;
                    break;

                case 0x01:
                case 0x02:
                case 0x03:
                case 0x04:
                case 0x05:
                case 0x06:
                case 0x07:
                    Console.Write((char)value);
                    break;
            }
        }
    }
}
