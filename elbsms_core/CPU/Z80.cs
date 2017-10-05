using System;
using System.Runtime.CompilerServices;


namespace elbsms_core.CPU
{
    using static StatusFlags;

    class Z80
    {
        private SystemClock _clock;
        private Bus _bus;

        private int _activeAFRegisters;
        private AFRegisters[] _afRegisters;
        private AFRegisters _afr;

        private int _activeGPRegisters;
        private GPRegisters[] _gpRegisters;
        private GPRegisters _gpr;

#pragma warning disable 0169

        private ushort _pc, _sp, _ix, _iy;

        private byte _i, _r;

#pragma warning disable 0414

        private byte _iff1, _iff2;
        private int _interruptMode;

#pragma warning restore 0414

#pragma warning restore 0169


        public Z80(SystemClock clock, Bus bus)
        {
            _clock = clock;
            _bus = bus;

            //_activeAFRegisters = 1 - _activeAFRegisters;
            _activeAFRegisters = 0;
            _afRegisters = new AFRegisters[2];
            _afRegisters[0] = new AFRegisters();
            _afRegisters[1] = new AFRegisters();
            _afr = _afRegisters[_activeAFRegisters];

            _activeGPRegisters = 0;
            _gpRegisters = new GPRegisters[2];
            _gpRegisters[0] = new GPRegisters();
            _gpRegisters[1] = new GPRegisters();
            _gpr = _gpRegisters[_activeGPRegisters];
        }

        private void SwitchAFRegisters()
        {
            _activeAFRegisters = 1 - _activeAFRegisters;
            _afr = _afRegisters[_activeAFRegisters];
        }

        private void SwitchGPRegistsers()
        {
            _activeGPRegisters = 1 - _activeGPRegisters;
            _gpr = _gpRegisters[_activeGPRegisters];
        }

        #region memory access

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private byte ReadOpcode(ushort address)
        {
            byte opcode = _bus.ReadByte(address);
            _clock.AddCycles(4);
            return opcode;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private byte ReadByte(ushort address)
        {
            byte value = _bus.ReadByte(address);
            _clock.AddCycles(3);
            return value;
        }

        private ushort ReadWord(ushort address)
        {
            byte lo = ReadByte(address);
            byte hi = ReadByte((ushort)(address + 1));

            return (ushort)(hi << 8 | lo);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WriteByte(ushort address, byte value)
        {
            _bus.WriteByte(address, value);
            _clock.AddCycles(3);
        }

        #endregion

        #region stack handling

        // Push a byte on to the stack, decrement SP then write to memory
        private void PushByte(byte value)
        {
            WriteByte(--_sp, value);
        }

        // Pop a byte from the stack, read from memory then increment SP
        private byte PopByte()
        {
            return ReadByte(_sp++);
        }

        // Push a word onto the stack, high byte pushed first
        private void PushWord(ushort value)
        {
            PushByte((byte)(value >> 8));
            PushByte((byte)value);
        }

        // Pop a word from the stack, low byte popped first
        private ushort PopWord()
        {
            byte lo = PopByte();
            byte hi = PopByte();

            return (ushort)(hi << 8 | lo);
        }

        #endregion

        private void ProcessInterrupts()
        {

        }

        public void ExecuteInstruction()
        {
            ProcessInterrupts();

            byte opcode = ReadOpcode(_pc++);
            ExecuteOpcode(opcode);
        }

        private void ExecuteOpcode(byte opcode)
        {
            switch (opcode)
            {
                #region 8-bit load group

                case 0x40: break;                  // LD B,B
                case 0x41: _gpr.B = _gpr.C; break; // LD B,C
                case 0x42: _gpr.B = _gpr.D; break; // LD B,D
                case 0x43: _gpr.B = _gpr.E; break; // LD B,E
                case 0x44: _gpr.B = _gpr.H; break; // LD B,H
                case 0x45: _gpr.B = _gpr.L; break; // LD B,L
                case 0x47: _gpr.B = _afr.A; break; // LD B,A


                case 0x48: _gpr.C = _gpr.B; break; // LD C,B
                case 0x49: break;                  // LD C,C
                case 0x4A: _gpr.C = _gpr.D; break; // LD C,D
                case 0x4B: _gpr.C = _gpr.E; break; // LD C,E
                case 0x4C: _gpr.C = _gpr.H; break; // LD C,H
                case 0x4D: _gpr.C = _gpr.L; break; // LD C,L
                case 0x4F: _gpr.C = _afr.A; break; // LD C,A

                case 0x50: _gpr.D = _gpr.B; break; // LD D,B
                case 0x51: _gpr.D = _gpr.C; break; // LD D,C
                case 0x52: break;                  // LD D,D
                case 0x53: _gpr.D = _gpr.E; break; // LD D,E
                case 0x54: _gpr.D = _gpr.H; break; // LD D,H
                case 0x55: _gpr.D = _gpr.L; break; // LD D,L
                case 0x57: _gpr.D = _afr.A; break; // LD D,A

                case 0x58: _gpr.E = _gpr.B; break; // LD E,B
                case 0x59: _gpr.E = _gpr.C; break; // LD E,C
                case 0x5A: _gpr.E = _gpr.D; break; // LD E,D
                case 0x5B: break;                  // LD E,E
                case 0x5C: _gpr.E = _gpr.H; break; // LD E,H
                case 0x5D: _gpr.E = _gpr.L; break; // LD E,L
                case 0x5F: _gpr.E = _afr.A; break; // LD E,A

                case 0x60: _gpr.H = _gpr.B; break; // LD H,B
                case 0x61: _gpr.H = _gpr.C; break; // LD H,C
                case 0x62: _gpr.H = _gpr.D; break; // LD H,D
                case 0x63: _gpr.H = _gpr.E; break; // LD H,E
                case 0x64: break;                  // LD H,H
                case 0x65: _gpr.H = _gpr.L; break; // LD H,L
                case 0x67: _gpr.H = _afr.A; break; // LD H,A

                case 0x68: _gpr.L = _gpr.B; break; // LD L,B
                case 0x69: _gpr.L = _gpr.C; break; // LD L,C
                case 0x6A: _gpr.L = _gpr.D; break; // LD L,D
                case 0x6B: _gpr.L = _gpr.E; break; // LD L,E
                case 0x6C: _gpr.L = _gpr.H; break; // LD L,H
                case 0x6D: break;                  // LD L,L
                case 0x6F: _gpr.L = _afr.A; break; // LD L,A

                case 0x78: _afr.A = _gpr.B; break; // LD A,B
                case 0x79: _afr.A = _gpr.C; break; // LD A,C
                case 0x7A: _afr.A = _gpr.D; break; // LD A,D
                case 0x7B: _afr.A = _gpr.E; break; // LD A,E
                case 0x7C: _afr.A = _gpr.H; break; // LD A,H
                case 0x7D: _afr.A = _gpr.L; break; // LD A,L
                case 0x7F: break;                  // LD A,A

                case 0x0E: _gpr.C = ReadByte(_pc++); break; // LD C,n
                case 0x3E: _afr.A = ReadByte(_pc++); break; // LD A,n

                case 0x46: _gpr.B = ReadByte(_gpr.HL); break; // LD B,(HL)
                case 0x4E: _gpr.C = ReadByte(_gpr.HL); break; // LD C,(HL)
                case 0x56: _gpr.D = ReadByte(_gpr.HL); break; // LD D,(HL)
                case 0x5E: _gpr.E = ReadByte(_gpr.HL); break; // LD E,(HL)
                case 0x66: _gpr.H = ReadByte(_gpr.HL); break; // LD H,(HL)
                case 0x6E: _gpr.L = ReadByte(_gpr.HL); break; // LD L,(HL)
                case 0x7E: _afr.A = ReadByte(_gpr.HL); break; // LD A,(HL)

                case 0x70: WriteByte(_gpr.HL, _gpr.B); break; // LD (HL),B
                case 0x71: WriteByte(_gpr.HL, _gpr.C); break; // LD (HL),C
                case 0x72: WriteByte(_gpr.HL, _gpr.D); break; // LD (HL),D
                case 0x73: WriteByte(_gpr.HL, _gpr.E); break; // LD (HL),E
                case 0x74: WriteByte(_gpr.HL, _gpr.H); break; // LD (HL),H
                case 0x75: WriteByte(_gpr.HL, _gpr.L); break; // LD (HL),L
                case 0x77: WriteByte(_gpr.HL, _afr.A); break; // LD (HL),A
                    
                #endregion

                #region 16-bit load group

                case 0x01: _gpr.BC = ReadWord(_pc); _pc += 2; break; // LD BC,nn
                case 0x11: _gpr.DE = ReadWord(_pc); _pc += 2; break; // LD DE,nn
                case 0x21: _gpr.HL = ReadWord(_pc); _pc += 2; break; // LD HL,nn
                case 0x31: _sp = ReadWord(_pc); _pc += 2; break;     // LD SP,nn

                case 0xC5: _clock.AddCycles(1); PushWord(_gpr.BC); break; // PUSH BC
                case 0xD5: _clock.AddCycles(1); PushWord(_gpr.DE); break; // PUSH DE
                case 0xE5: _clock.AddCycles(1); PushWord(_gpr.HL); break; // PUSH HL
                case 0xF5: _clock.AddCycles(1); PushWord(_afr.AF); break; // PUSH AF

                case 0xC1: _gpr.BC = PopWord(); break; // POP BC
                case 0xD1: _gpr.DE = PopWord(); break; // POP DE
                case 0xE1: _gpr.HL = PopWord(); break; // POP HL
                case 0xF1: _afr.AF = PopWord(); break; // POP AF

                #endregion

                #region exchange, block transfer, and search group

                #endregion

                #region general-purpose arithmetic and cpu control group

                case 0xED: ExecuteEDPrefixOpcode(ReadOpcode(_pc++)); break;

                case 0xF3: DisableInterrupts(); break; // DI

                #endregion

                #region jump group

                case 0xC3: JumpImmediate(); break; // JP nn

                #endregion

                #region call and return group

                case 0xCD: CallImmediate(); break; // CALL nn

                case 0xC9: Return(); break; // RET

                #endregion

                #region input and output group

                case 0xD3: _bus.Out(ReadByte(_pc++), _afr.A); _clock.AddCycles(4); break; // OUT (n),A

                #endregion

                default:
                    throw new NotImplementedException($"Unimplemented opcode: 0x{opcode:X2} at address 0x{_pc - 1:X4}");
            }
        }

        private void ExecuteEDPrefixOpcode(byte opcode)
        {
            switch (opcode)
            {
                #region exchange, block transfer, and search group

                case 0xB0: LoadIncrementAndRepeat(); break; // LDIR

                #endregion

                #region general-purpose arithmetic and cpu control group

                case 0x56: SetInterruptMode(0); break; // IM 0

                #endregion

                default:
                    throw new NotImplementedException($"Unimplemented opcode: 0xED {opcode:X2} at address 0x{_pc - 2:X4}");
            }
        }

        #region exchange, block transfer, and search group handlers

        private void LoadAndIncrement()
        {
            byte data = ReadByte(_gpr.HL++);

            WriteByte(_gpr.DE++, data);

            _clock.AddCycles(2);

            _afr.F &= ~(H | N);

            if (--_gpr.BC != 0)
                _afr.F |= PV;
            else
                _afr.F &= ~PV;
        }

        private void LoadIncrementAndRepeat()
        {
            LoadAndIncrement();

            if (_gpr.BC == 0)
                return;

            _clock.AddCycles(5);

            _pc -= 2;
        }

        #endregion

        #region general-purpose arithmetic and cpu control group handlers

        private void DisableInterrupts()
        {
            _iff1 = 0; _iff2 = 0;
        }

        private void SetInterruptMode(int interruptMode)
        {
            _interruptMode = interruptMode;
        }

        #endregion

        #region jump group handlers

        private void JumpImmediate()
        {
            _pc = ReadWord(_pc);
        }

        #endregion

        #region call and return group handlers

        private void CallImmediate()
        {
            ushort address = ReadWord(_pc);
            _pc += 2;

            _clock.AddCycles(1);

            PushWord(_pc);
            _pc = address;
        }

        private void Return()
        {
            _pc = PopWord();
        }

        #endregion
    }
}
