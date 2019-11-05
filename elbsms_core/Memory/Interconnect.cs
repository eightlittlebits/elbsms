using System;
using System.Diagnostics;

namespace elbsms_core.Memory
{
    internal class Interconnect
    {
        private const int RamSize = 0x2000; // 8KB
        private const int RamMask = RamSize - 1;
        private readonly byte[] _ram = new byte[RamSize];

        private readonly MemoryControlRegister _memoryControl;

        private GameMedia _cartridge;
        private GameMedia _segaCard;

        public Interconnect()
        {
            _memoryControl = new MemoryControlRegister { Value = 0x00 };
        }

        internal void LoadGameMedia(GameMedia media)
        {
            switch (media.MediaType)
            {
                case GameMediaType.Cartridge:
                    _cartridge = media;
                    break;

                case GameMediaType.SegaCard:
                    _segaCard = media;
                    break;

                default:
                    throw new Exception($"Unsupported GameMediaType value: {media.MediaType}");
            }
        }

        internal byte ReadByte(ushort address)
        {
            // 0x0000 -> 0xBFFF - Bootstrap/Cartridge/Card/Expansion
            if (address < 0xC000)
            {
                if (_memoryControl.CartridgeSlotEnabled && _cartridge != null)
                    return _cartridge.ReadByte(address);
            }
            // 0xC000 -> 0xDFFF - System RAM
            // 0xE000 -> 0xFFFF - System RAM (mirror)
            else
            {
                if (_memoryControl.WorkRamEnabled)
                {
                    return _ram[address & RamMask];
                }
            }

            return 0xFF;
        }

        internal void WriteByte(ushort address, byte value)
        {
            // 0x0000 -> 0xBFFF - Bootstrap/Cartridge/Card/Expansion
            if (address < 0xC000)
            {
                _cartridge.WriteByte(address);
            }
            // 0xC000 -> 0xDFFF - System RAM
            // 0xE000 -> 0xFFFF - System RAM (mirror)
            else
            {
                if (_memoryControl.WorkRamEnabled)
                {
                    _ram[address & RamMask] = value;
                }
            }
        }

        internal void Out(byte address, byte value)
        {
            Debug.WriteLine($"OUT: 0x{address:X4}, {(char)value}(0x{value:X2})");

            // the port is selected with bits 7, 6 and 0 of the address (8 total available ports)
            int port = ((address & 0xC0) >> 5) | (address & 0x01);

            switch (port)
            {
                // memory control register
                case 0x00:
                    _memoryControl.Value = value;
                    break;

                // i/o port control
                // http://www.smspower.org/Development/PeripheralPorts
                // http://www.smspower.org/uploads/Development/port3f.txt
                // http://www.smspower.org/Development/RegionDetection
                case 0x01:
                    break;

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

        internal byte In(byte address)
        {
            Debug.WriteLine($"IN: 0x{address:X4}");

            // the port is selected with bits 7, 6 and 0 of the address (8 total available ports)
            int port = ((address & 0xC0) >> 5) | (address & 0x01);

            return port switch
            {
                // memory control register
                //case 0x00:
                //case 0x01:
                //case 0x02:
                //case 0x03:
                //case 0x04:
                //case 0x05:
                //case 0x06:
                //case 0x07:

                _ => 0xFF,
            };
        }
    }
}
