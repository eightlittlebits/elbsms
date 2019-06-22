﻿using System;
using System.Runtime.CompilerServices;

// todo(david): remove this when merging vdp
namespace elbsms_core
{
    public enum VideoStandard
    {
        NTSC,
        PAL
    }
}

namespace elbsms_core.Video
{
    // SMS VDP is based on the Texas Instruments TMS9918a
    // http://www.smspower.org/uploads/Development/msvdp-20021112.txt
    // we're going with an SMS2 vdp to start with
    internal sealed class VideoDisplayProcessor : ClockedComponent
    {
        private const int VRamSize = 0x4000;
        private const int VRamMask = VRamSize - 1;
        private readonly byte[] _vram = new byte[VRamSize];

        private const int CRamSize = 0x20;
        private const int CRamMask = CRamSize - 1;
        private readonly byte[] _cram = new byte[CRamSize];

        private const int NTSCScanlinesPerFrame = 262;
        private const int NTSCVCounterOffset = 6;

        private const int PALScanlinesPerFrame = 313;
        private const int PALVCounterOffset = 57;

        private const int CyclesPerScanline = 684;

        private const int VDPMode224Lines = 0b1011;
        private const int VDPMode240Lines = 0b1110;

        private static class VdpOp
        {
            public const int ReadVram = 0;
            public const int WriteVram = 1;
            public const int WriteRegister = 2;
            public const int WriteCram = 3;
        }

        private readonly VideoStandard _videoStandard;
        private readonly uint _scanlinesPerFrame;

        private bool _firstControlWrite;
        private byte _addressBuffer;
        private ushort _addressRegister;
        private int _codeRegister;

        private byte _readBuffer;
        private VDPStatusFlags _statusFlags;

        private readonly byte[] _registers;

        private int _vdpMode;

        // control register 1
        private bool _verticalScrollLock;
        private bool _horizontalScrollLock;
        private bool _maskColumn0;
        private bool _lineInterruptEnable;
        private bool _shiftSpritesLeft;
        private bool _syncEnabled;

        // control register 2
        private bool _displayEnabled;
        private bool _frameInterruptEnabled;
        private bool _largeSprites;
        private bool _zoomedSprites;

        private ushort _nameTableBaseAddress;
        private ushort _spriteAttributeTableBaseAddress;
        private ushort _spritePatternTableBaseAddress;
        private int _overscanColourIndex;
        private byte _backgroundXScroll;
        private byte _backgroundYScroll;
        private byte _lineInterruptValue;

        private uint _currentScanlineCycles;

        private uint _vCounter;
        private int _lineInterruptCounter;

        public VideoDisplayProcessor(SystemClock clock, VideoStandard videoStandard) : base(clock)
        {
            _registers = new byte[16];

            _firstControlWrite = true;
            _statusFlags = 0;

            _videoStandard = videoStandard;

            // todo(david): convert to switch expression with c# 8
            switch (videoStandard)
            {
                case VideoStandard.NTSC:
                    _scanlinesPerFrame = NTSCScanlinesPerFrame;
                    break;

                case VideoStandard.PAL:
                    _scanlinesPerFrame = PALScanlinesPerFrame;
                    break;
            }
        }

        public byte HCounter { get; private set; }

        public byte VCounter
        {
            get
            {
                // the vcounter has an offset applied on the first line of vertical blanking
                // to make the 262/313 scanlines fit into the range 00-FF.
                switch (_videoStandard)
                {
                    case VideoStandard.NTSC:
                        switch (_vdpMode)
                        {
                            default: return (byte)(_vCounter < 219 ? _vCounter : _vCounter - NTSCVCounterOffset);
                            case VDPMode224Lines: return (byte)(_vCounter < 235 ? _vCounter : _vCounter - NTSCVCounterOffset);
                            case VDPMode240Lines: return (byte)_vCounter; // unsupported
                        }

                    case VideoStandard.PAL:
                        switch (_vdpMode)
                        {
                            default: return (byte)(_vCounter < 243 ? _vCounter : _vCounter - PALVCounterOffset);
                            case VDPMode224Lines: return (byte)(_vCounter < 259 ? _vCounter : _vCounter - PALVCounterOffset);
                            case VDPMode240Lines: return (byte)(_vCounter < 267 ? _vCounter : _vCounter - PALVCounterOffset);
                        }

                    default:
                        throw new InvalidOperationException("Invalid Video Standard");
                }
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

                // writes to the control port consist of a 2 byte "command word"
                // the first byte written sets the low 8 bits of the address buffer
                // the second byte written sets the high 6 bits of the address buffer and the code register
                // the vdp may then carry out additional processing based on the code register
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
                        case VdpOp.ReadVram: _readBuffer = _vram[_addressRegister]; IncrementAddressRegister(); break;
                        case VdpOp.WriteRegister: RegisterWrite(value & 0x0F, _addressBuffer); break;
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
                    case VdpOp.ReadVram:
                    case VdpOp.WriteVram:
                    case VdpOp.WriteRegister:
                        _vram[_addressRegister] = value;
                        break;

                    case VdpOp.WriteCram:
                        // https://lospec.com/palette-list/6-bit-rgb
                        _cram[_addressRegister & CRamMask] = value;
                        break;
                }

                _readBuffer = value;
                IncrementAddressRegister();
            }
        }

        // http://www.smspower.org/Development/VDPRegisters
        private void RegisterWrite(int register, byte value)
        {
            _registers[register] = value;

            switch (register)
            {
                // Register $00 - Mode Control No. 1
                case 0x0:
                    _verticalScrollLock = value.Bit(7);     // D7 - 1= Disable vertical scrolling for columns 24-31
                    _horizontalScrollLock = value.Bit(6);   // D6 - 1= Disable horizontal scrolling for rows 0-1
                    _maskColumn0 = value.Bit(5);            // D5 - 1= Mask column 0 with overscan color from register #7
                    _lineInterruptEnable = value.Bit(4);    // D4 - (IE1) 1= Line interrupt enable
                    _shiftSpritesLeft = value.Bit(3);       // D3 - (EC) 1= Shift sprites left by 8 pixels

                    _vdpMode &= 0b0101; // clear bits 1 and 3
                    _vdpMode |= value.Bit(2) ? 0b1000 : 0;  // D2 - (M4) 1= Use Mode 4, 0= Use TMS9918 modes (selected with M1, M2, M3)
                    _vdpMode |= value.Bit(1) ? 0b0010 : 0;  // D1 - (M2) Must be 1 for M1/M3 to change screen height in Mode 4.

                    _syncEnabled = !value.Bit(0);           // D0 - 1= No sync, display is monochrome, 0= Normal display
                    break;

                // Register $01 - Mode Control No. 2
                case 0x1:
                    // D7 - No effect
                    _displayEnabled = value.Bit(6);         // D6 - (BLK) 1= Display visible, 0= display blanked.
                    _frameInterruptEnabled = value.Bit(5);  // D5 - (IE0) 1= Frame interrupt enable.

                    _vdpMode &= 0b1010; // clear bits 0 and 2
                    _vdpMode |= value.Bit(4) ? 0b0001 : 0;  // D4 - (M1) Selects 224-line screen for Mode 4 if M2=1, else has no effect.
                    _vdpMode |= value.Bit(3) ? 0b0100 : 0;  // D3 - (M3) Selects 240-line screen for Mode 4 if M2=1, else has no effect.
                                                            // D2 - No effect
                    _largeSprites = value.Bit(1);           // D1 - Sprites are 1=16x16,0=8x8 (TMS9918), Sprites are 1=8x16,0=8x8 (Mode 4)
                    _zoomedSprites = value.Bit(0);          // D0 - Sprite pixels are doubled in size.
                    break;

                // Register $02 - Name Table Base Address
                case 0x2: _nameTableBaseAddress = (ushort)((value & 0x0E) << 10); break;

                // Register $03 - Color Table Base Address
                case 0x3: break;

                // Register $04 - Background Pattern Generator Base Address
                case 0x4: break;

                // Register $05 - Sprite Attribute Table Base Address
                case 0x5: _spriteAttributeTableBaseAddress = (ushort)((value & 0x7E) << 7); break;

                // Register $06 - Sprite Pattern Generator Base Address
                case 0x6: _spritePatternTableBaseAddress = (ushort)((value & 0x04) << 11); break;

                // Register $07 - Overscan/Backdrop Color
                case 0x7: _overscanColourIndex = value & 0x0F; break;

                // Register $08 - Background X Scroll
                case 0x8: _backgroundXScroll = value; break;

                // Register $09 - Background Y Scroll
                case 0x9: _backgroundYScroll = value; break;

                // Register $0A - Line counter
                case 0xA: _lineInterruptValue = value; break;
            }
        }

        public void LatchHCounter()
        {
            SynchroniseWithSystemClock();

            // the dot clock is half the speed of the master clock
            // the hcounter is the top 8 bits of a 9 bit counter
            // seems to be offset 16 pixels (32 cycles), HC = 0 is 3 pixels before left border
            // http://www.smspower.org/forums/8161-SMSDisplayTiming#51750
            uint shiftedPixelCount = ((_currentScanlineCycles + 32) % CyclesPerScanline) >> 2;

            HCounter = (byte)(shiftedPixelCount <= 0x93 ? shiftedPixelCount : shiftedPixelCount + 0x55);
        }

        // todo(david): convert to switch expression with c# 8
        private int ActiveDisplayLineCount()
        {
            switch (_vdpMode)
            {
                default: return 192;
                case VDPMode224Lines: return 224;
                case VDPMode240Lines: return 240;
            }
        }

        // http://www.smspower.org/forums/13530-VDPClockSpeed
        // http://www.smspower.org/forums/8161-SMSDisplayTiming
        public override void Update(uint cycleCount)
        {
            _currentScanlineCycles += cycleCount;

            // have we completed a scanline?
            if (_currentScanlineCycles >= CyclesPerScanline)
            {
                _vCounter = (_vCounter + 1) % _scanlinesPerFrame;

                _currentScanlineCycles -= CyclesPerScanline;
            }
        }
    }
}
