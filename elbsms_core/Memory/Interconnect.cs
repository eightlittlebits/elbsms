using System;
using System.Diagnostics;
using elbsms_core.Video;

namespace elbsms_core.Memory
{
    internal class Interconnect
    {
        private const int RamSize = 0x2000;
        private const int RamMask = RamSize - 1;
        private readonly byte[] _ram = new byte[RamSize];

        private readonly MemoryControlRegister _memoryControl;
        private readonly VideoDisplayProcessor _vdp;

        private Cartridge _cartridge;

        public Interconnect(VideoDisplayProcessor vdp)
        {
            _memoryControl = new MemoryControlRegister { Value = 0x00 };
            _vdp = vdp;
        }

        internal void LoadCartridge(Cartridge cartridge)
        {
            _cartridge = cartridge;
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
                    break;

                case 0x04:
                    _vdp.DataPort = value;
                    break;

                case 0x05:
                    _vdp.ControlPort = value;
                    break;

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
            var port = ((address & 0xC0) >> 5) | (address & 0x01);

            switch (port)
            {
                // memory control register
                //case 0x00:
                //case 0x01:

                case 0x02: return _vdp.VCounter; 
                case 0x03: return _vdp.HCounter;
                case 0x04: return _vdp.DataPort;
                case 0x05: return _vdp.ControlPort;

                //case 0x06:
                //case 0x07:

                default:
                    return 0xFF;
            }
        }
    }
}
