using System;

namespace elbsms_core.Video
{
    // SMS VDP is based on the Texas Instruments TMS9918a
    // http://www.smspower.org/uploads/Development/msvdp-20021112.txt
    internal class VideoDisplayProcessor : ClockedComponent
    {
        private const int VRamSize = 0x4000;
        private const int VRamMask = VRamSize - 1;
        private readonly byte[] _vram = new byte[VRamSize];

        private const int CRamSize = 0x20;
        private const int CRamMask = CRamSize - 1;
        private readonly byte[] _cram = new byte[CRamSize];

        private bool _firstControlWrite;
        private byte _addressBuffer;

        private ushort _addressRegister;
        private int _codeRegister;

        private byte _readBuffer;

        private VDPStatusFlags _statusFlags;

        public byte VCounter { get; internal set; }
        public byte HCounter { get; internal set; }

        public VideoDisplayProcessor(SystemClock clock) : base(clock)
        {
            _firstControlWrite = true;
            _statusFlags = 0;
        }

        private void IncrementAddressRegister() => _addressRegister = (ushort)((_addressRegister + 1) & VRamMask);

        public byte ControlPort
        {
            get
            {
                SynchroniseWithSystemClock();
                _firstControlWrite = true;

                byte flags = (byte)_statusFlags;

                _statusFlags = 0;

                return flags;
            }
            set
            {
                SynchroniseWithSystemClock();

                if (_firstControlWrite)
                {
                    _firstControlWrite = false;
                    _addressBuffer = value;
                }
                else
                {
                    _firstControlWrite = true;

                    _codeRegister = (value & 0xC0) >> 6;
                    _addressRegister = (ushort)(((value & 0x3F) << 8) | _addressBuffer);

                    switch (_codeRegister)
                    {
                        case 0: _readBuffer = _vram[_addressRegister]; IncrementAddressRegister(); break;
                        case 1: break;
                        case 2: /* VDP register write */ throw new NotImplementedException();
                        case 3: break;
                    }
                }
            }
        }

        public byte DataPort
        {
            get
            {
                SynchroniseWithSystemClock();
                _firstControlWrite = true;

                byte data = _readBuffer;
                _readBuffer = _vram[_addressRegister];
                IncrementAddressRegister();

                return data;
            }

            set
            {
                SynchroniseWithSystemClock();
                _firstControlWrite = true;

                switch (_codeRegister)
                {
                    case 0:
                    case 1:
                    case 2:
                        _vram[_addressRegister] = value;
                        break;

                    case 3:
                        _cram[_addressRegister & CRamMask] = value;
                        break;
                }

                _readBuffer = value;
                IncrementAddressRegister();
            }
        }

        public override void Update(uint cycleCount)
        {

        }
    }
}
