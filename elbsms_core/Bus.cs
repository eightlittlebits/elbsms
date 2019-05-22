using System;
using System.Diagnostics;

namespace elbsms_core
{
    class Bus
    {
        private const int RamSize = 0x2000; // 8KB
        private readonly byte[] _ram = new byte[RamSize];

        private GameMedia _cartridge;
        private GameMedia _segaCard;

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
            // 0x0000 -> 0xBFFF - Cartridge
            if (address < 0xC000)
            {
                return _cartridge.ReadByte(address);
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
            // TODO(david): implement out
            Debug.WriteLine($"OUT: 0x{address:X4}, {(char)value}(0x{value:X2})");

            if (address == 0xFD)
            {
                Console.Write((char)value);
            }
        }
    }
}
