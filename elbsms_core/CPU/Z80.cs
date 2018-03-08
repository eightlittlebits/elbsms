using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace elbsms_core.CPU
{
    using static StatusFlags;

    partial class Z80
    {
        private static byte[] FlagsSZP;

        private SystemClock _clock;
        private Bus _bus;

        private int _activeAFRegisters;
        private AFRegisters[] _afRegisters;
        private AFRegisters _afr;

        private int _activeGPRegisters;
        private GPRegisters[] _gpRegisters;
        private GPRegisters _gpr;

#pragma warning disable 0169

        private ushort _pc, _sp;

        private PairedRegister _ix, _iy;

        private byte _i, _r;

#pragma warning disable 0414

        private byte _iff1, _iff2;
        private int _interruptMode;

#pragma warning restore 0414

#pragma warning restore 0169

        static Z80()
        {
            InitStatusTables();
        }

        private static void InitStatusTables()
        {
            FlagsSZP = new byte[0x100];

            for (int i = 0; i < 0x100; i++)
            {
                int sf, zf, b5, b3, pf;

                sf = i & S;
                zf = i == 0 ? Z : 0;
                b5 = i & B5;
                b3 = i & B3;
                pf = evenParity(i) ? P : 0;

                FlagsSZP[i] = (byte)(sf | zf | b5 | b3 | pf);
            }

            bool evenParity(int v)
            {
                v ^= v >> 4;
                v ^= v >> 2;
                v ^= v >> 1;

                return (v & 1) != 1;
            }
        }

        public Z80(SystemClock clock, Bus bus)
        {
            _clock = clock;
            _bus = bus;

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

        private void IncrementMemoryRefreshRegister()
        {
            // only the low 7 bits are incremented, high bit preserved
            _r = (byte)(((_r + 1) & 0x7F) | (_r & 0x80));
        }

        #region memory access

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private byte ReadOpcode(ushort address)
        {
            IncrementMemoryRefreshRegister();
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

        private void WriteWord(ushort address, ushort value)
        {
            WriteByte(address, (byte)value);
            WriteByte((ushort)(address + 1), (byte)(value >> 8));
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

                case 0x06: _gpr.B = ReadByte(_pc++); break; // LD B,n
                case 0x0E: _gpr.C = ReadByte(_pc++); break; // LD C,n
                case 0x16: _gpr.D = ReadByte(_pc++); break; // LD D,n
                case 0x1E: _gpr.E = ReadByte(_pc++); break; // LD E,n
                case 0x26: _gpr.H = ReadByte(_pc++); break; // LD H,n
                case 0x2E: _gpr.L = ReadByte(_pc++); break; // LD L,n
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

                case 0x36: WriteByte(_gpr.HL, ReadByte(_pc++)); break; // LD (HL),n

                case 0x0A: _afr.A = ReadByte(_gpr.BC); break; // LD A,(BC)
                case 0x1A: _afr.A = ReadByte(_gpr.DE); break; // LD A,(DE)

                case 0x3A: _afr.A = ReadByte(ReadWord(_pc)); _pc += 2; break; // LD A,(nn)

                case 0x02: WriteByte(_gpr.BC, _afr.A); break; // LD (BC),A
                case 0x12: WriteByte(_gpr.DE, _afr.A); break; // LD (DE),A
                case 0x32: WriteByte(ReadWord(_pc), _afr.A); _pc += 2; break; // LD (nn),A

                #endregion

                #region 16-bit load group

                case 0x01: _gpr.BC = ReadWord(_pc); _pc += 2; break; // LD BC,nn
                case 0x11: _gpr.DE = ReadWord(_pc); _pc += 2; break; // LD DE,nn
                case 0x21: _gpr.HL = ReadWord(_pc); _pc += 2; break; // LD HL,nn
                case 0x31: _sp = ReadWord(_pc); _pc += 2; break;     // LD SP,nn

                case 0x2A: _gpr.HL = ReadWord(ReadWord(_pc)); _pc += 2; break; // LD HL,(nn)

                case 0x22: WriteWord(ReadWord(_pc), _gpr.HL); _pc += 2; break; // LD (nn),HL

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

                case 0xEB: ushort temp = _gpr.DE; _gpr.DE = _gpr.HL; _gpr.HL = temp; break; // EX DE,HL

                #endregion

                #region 8-bit arithmetic group

                case 0xC6: (_afr.A, _afr.F) = Add8Bit(_afr.A, ReadByte(_pc++)); break;  // ADD A,n

                case 0xA0: (_afr.A, _afr.F) = And8Bit(_afr.A, _gpr.B); break;               // AND B
                case 0xA1: (_afr.A, _afr.F) = And8Bit(_afr.A, _gpr.C); break;               // AND C
                case 0xA2: (_afr.A, _afr.F) = And8Bit(_afr.A, _gpr.D); break;               // AND D
                case 0xA3: (_afr.A, _afr.F) = And8Bit(_afr.A, _gpr.E); break;               // AND E
                case 0xA4: (_afr.A, _afr.F) = And8Bit(_afr.A, _gpr.H); break;               // AND H
                case 0xA5: (_afr.A, _afr.F) = And8Bit(_afr.A, _gpr.L); break;               // AND L
                case 0xA6: (_afr.A, _afr.F) = And8Bit(_afr.A, ReadByte(_gpr.HL)); break;    // AND (HL)
                case 0xA7: (_afr.A, _afr.F) = And8Bit(_afr.A, _afr.A); break;               // AND A
                case 0xE6: (_afr.A, _afr.F) = And8Bit(_afr.A, ReadByte(_pc++)); break;      // AND n

                case 0xB0: (_afr.A, _afr.F) = Or8Bit(_afr.A, _gpr.B); break;            // OR B
                case 0xB1: (_afr.A, _afr.F) = Or8Bit(_afr.A, _gpr.C); break;            // OR C
                case 0xB2: (_afr.A, _afr.F) = Or8Bit(_afr.A, _gpr.D); break;            // OR D
                case 0xB3: (_afr.A, _afr.F) = Or8Bit(_afr.A, _gpr.E); break;            // OR E
                case 0xB4: (_afr.A, _afr.F) = Or8Bit(_afr.A, _gpr.H); break;            // OR H
                case 0xB5: (_afr.A, _afr.F) = Or8Bit(_afr.A, _gpr.L); break;            // OR L
                case 0xB6: (_afr.A, _afr.F) = Or8Bit(_afr.A, ReadByte(_gpr.HL)); break; // OR (HL)
                case 0xB7: (_afr.A, _afr.F) = Or8Bit(_afr.A, _afr.A); break;            // OR A
                case 0xF6: (_afr.A, _afr.F) = Or8Bit(_afr.A, ReadByte(_pc++)); break;   // OR n

                case 0xA8: (_afr.A, _afr.F) = Xor8Bit(_afr.A, _gpr.B); break;               // XOR B
                case 0xA9: (_afr.A, _afr.F) = Xor8Bit(_afr.A, _gpr.C); break;               // XOR C
                case 0xAA: (_afr.A, _afr.F) = Xor8Bit(_afr.A, _gpr.D); break;               // XOR D
                case 0xAB: (_afr.A, _afr.F) = Xor8Bit(_afr.A, _gpr.E); break;               // XOR E
                case 0xAC: (_afr.A, _afr.F) = Xor8Bit(_afr.A, _gpr.H); break;               // XOR H
                case 0xAD: (_afr.A, _afr.F) = Xor8Bit(_afr.A, _gpr.L); break;               // XOR L
                case 0xAE: (_afr.A, _afr.F) = Xor8Bit(_afr.A, ReadByte(_gpr.HL)); break;    // XOR (HL)
                case 0xAF: (_afr.A, _afr.F) = Xor8Bit(_afr.A, _afr.A); break;               // XOR A
                case 0xEE: (_afr.A, _afr.F) = Xor8Bit(_afr.A, ReadByte(_pc++)); break;      // XOR n

                case 0xB8: _afr.F = Compare8Bit(_afr.A, _gpr.B); break;             // CP B
                case 0xB9: _afr.F = Compare8Bit(_afr.A, _gpr.C); break;             // CP C
                case 0xBA: _afr.F = Compare8Bit(_afr.A, _gpr.D); break;             // CP D
                case 0xBB: _afr.F = Compare8Bit(_afr.A, _gpr.E); break;             // CP E
                case 0xBC: _afr.F = Compare8Bit(_afr.A, _gpr.H); break;             // CP H
                case 0xBD: _afr.F = Compare8Bit(_afr.A, _gpr.L); break;             // CP L
                case 0xBE: _afr.F = Compare8Bit(_afr.A, ReadByte(_gpr.HL)); break;  // CP (HL)
                case 0xBF: _afr.F = Compare8Bit(_afr.A, _afr.A); break;             // CP A
                case 0xFE: _afr.F = Compare8Bit(_afr.A, ReadByte(_pc++)); break;    // CP n

                case 0x04: _gpr.B = Inc8Bit(_gpr.B); break;                         // INC B
                case 0x0C: _gpr.C = Inc8Bit(_gpr.C); break;                         // INC C
                case 0x14: _gpr.D = Inc8Bit(_gpr.D); break;                         // INC D
                case 0x1C: _gpr.E = Inc8Bit(_gpr.E); break;                         // INC E
                case 0x24: _gpr.H = Inc8Bit(_gpr.H); break;                         // INC H
                case 0x2C: _gpr.L = Inc8Bit(_gpr.L); break;                         // INC L
                case 0x34: WriteByte(_gpr.HL, Inc8Bit(ReadByte(_gpr.HL))); break;   // INC (HL)
                case 0x3C: _afr.A = Inc8Bit(_afr.A); break;                         // INC A

                case 0x05: _gpr.B = Dec8Bit(_gpr.B); break;                         // DEC B
                case 0x0D: _gpr.C = Dec8Bit(_gpr.C); break;                         // DEC C
                case 0x15: _gpr.D = Dec8Bit(_gpr.D); break;                         // DEC D
                case 0x1D: _gpr.E = Dec8Bit(_gpr.E); break;                         // DEC E
                case 0x25: _gpr.H = Dec8Bit(_gpr.H); break;                         // DEC H
                case 0x2D: _gpr.L = Dec8Bit(_gpr.L); break;                         // DEC L
                case 0x35: WriteByte(_gpr.HL, Dec8Bit(ReadByte(_gpr.HL))); break;   // DEC (HL)
                case 0x3D: _afr.A = Dec8Bit(_afr.A); break;                         // DEC A

                #endregion

                #region general-purpose arithmetic and cpu control group

                case 0xDD: ExecuteDDFDPrefixedOpcode(opcode, ReadOpcode(_pc++)); break;
                case 0xED: ExecuteEDPrefixedOpcode(ReadOpcode(_pc++)); break;
                case 0xFD: ExecuteDDFDPrefixedOpcode(opcode, ReadOpcode(_pc++)); break;
                case 0xCB: ExecuteCBPrefixedOpcode(ReadOpcode(_pc++)); break;

                case 0x00: break; // NOP

                case 0xF3: DisableInterrupts(); break; // DI

                #endregion

                #region 16-bit arithmetic group

                case 0x09: AddHL(_gpr.BC); break; // ADD HL,BC
                case 0x19: AddHL(_gpr.DE); break; // ADD HL,DE
                case 0x29: AddHL(_gpr.HL); break; // ADD HL,HL
                case 0x39: AddHL(_sp); break; // ADD HL,SP

                case 0x03: _clock.AddCycles(2); _gpr.BC++; break; // INC BC
                case 0x13: _clock.AddCycles(2); _gpr.DE++; break; // INC DE
                case 0x23: _clock.AddCycles(2); _gpr.HL++; break; // INC HL
                case 0x33: _clock.AddCycles(2); _sp++; break; // INC SP

                case 0x0B: _clock.AddCycles(2); _gpr.BC--; break; // DEC BC
                case 0x1B: _clock.AddCycles(2); _gpr.DE--; break; // DEC DE
                case 0x2B: _clock.AddCycles(2); _gpr.HL--; break; // DEC HL
                case 0x3B: _clock.AddCycles(2); _sp--; break; // DEC SP

                #endregion

                #region rotate and shift group

                case 0x07: RotateLeftAccumulator(); break; // RLCA

                case 0x0F: RotateRightAccumulator(); break; // RRCA

                #endregion

                #region jump group

                case 0xC3: JumpImmediate(); break; // JP nn

                case 0xC2: JumpImmediate(!_afr.F[Z]); break; // JP NZ,nn
                case 0xCA: JumpImmediate(_afr.F[Z]); break; // JP Z,nn
                case 0xD2: JumpImmediate(!_afr.F[C]); break; // JP NC,nn
                case 0xDA: JumpImmediate(_afr.F[C]); break; // JP C,nn
                case 0xE2: JumpImmediate(!_afr.F[P]); break; // JP PO,nn
                case 0xEA: JumpImmediate(_afr.F[P]); break; // JP PE,nn
                case 0xF2: JumpImmediate(!_afr.F[S]); break; // JP P,nn
                case 0xFA: JumpImmediate(_afr.F[S]); break; // JP M,nn

                case 0x18: JumpRelative(); break; // JR e

                case 0x28: JumpRelative(_afr.F[Z]); break; // JR Z,e

                #endregion

                #region call and return group

                case 0xCD: CallImmediate(); break; // CALL nn

                case 0xC4: CallImmediate(!_afr.F[Z]); break; // CALL NZ,nn
                case 0xCC: CallImmediate(_afr.F[Z]); break; // CALL Z,nn   
                case 0xD4: CallImmediate(!_afr.F[C]); break; // CALL NC,nn
                case 0xDC: CallImmediate(_afr.F[C]); break; // CALL C,nn   
                case 0xE4: CallImmediate(!_afr.F[P]); break; // CALL PO,nn
                case 0xEC: CallImmediate(_afr.F[P]); break; // CALL PE,nn   
                case 0xF4: CallImmediate(!_afr.F[S]); break; // CALL P,nn
                case 0xFC: CallImmediate(_afr.F[S]); break; // CALL M,nn   

                case 0xC9: Return(); break; // RET

                case 0xC0: Return(!_afr.F[Z]); break; // RET NZ
                case 0xC8: Return(_afr.F[Z]); break; // RET Z   
                case 0xD0: Return(!_afr.F[C]); break; // RET NC
                case 0xD8: Return(_afr.F[C]); break; // RET C   
                case 0xE0: Return(!_afr.F[P]); break; // RET PO
                case 0xE8: Return(_afr.F[P]); break; // RET PE   
                case 0xF0: Return(!_afr.F[S]); break; // RET P
                case 0xF8: Return(_afr.F[S]); break; // RET M   

                #endregion

                #region input and output group

                case 0xD3: _bus.Out(ReadByte(_pc++), _afr.A); _clock.AddCycles(4); break; // OUT (n),A

                #endregion

                default:
                    throw new NotImplementedException($"Unimplemented opcode: 0x{opcode:X2} at address 0x{_pc - 1:X4}");
            }
        }

        private void ExecuteEDPrefixedOpcode(byte opcode)
        {
            switch (opcode)
            {
                #region 16-bit load group

                case 0x4B: _gpr.BC = ReadWord(ReadWord(_pc)); _pc += 2; break; // LD BC,(nn)
                case 0x5B: _gpr.DE = ReadWord(ReadWord(_pc)); _pc += 2; break; // LD DE,(nn)
                case 0x6B: _gpr.HL = ReadWord(ReadWord(_pc)); _pc += 2; break; // LD HL,(nn)
                case 0x7B: _sp = ReadWord(ReadWord(_pc)); _pc += 2; break; // LD SP,(nn)

                case 0x43: WriteWord(ReadWord(_pc), _gpr.BC); _pc += 2; break; // LD (nn),BC
                case 0x53: WriteWord(ReadWord(_pc), _gpr.DE); _pc += 2; break; // LD (nn),DE
                case 0x63: WriteWord(ReadWord(_pc), _gpr.HL); _pc += 2; break; // LD (nn),HL
                case 0x73: WriteWord(ReadWord(_pc), _sp); _pc += 2; break; // LD (nn),SP

                #endregion

                #region exchange, block transfer, and search group

                case 0xA0: LoadAndIncrement(); break; // LDI
                case 0xB0: LoadIncrementAndRepeat(); break; // LDIR
                case 0xA8: LoadAndDecrement(); break; // LDD
                case 0xB8: LoadDecrementAndRepeat(); break; // LDDR

                case 0xA1: CompareAndIncrement(); break; // CPI
                case 0xB1: CompareIncrementAndRepeat(); break; // CPIR
                case 0xA9: CompareAndDecrement(); break; // CPD
                case 0xB9: CompareDecrementAndRepeat(); break; // CPDR

                #endregion

                #region general-purpose arithmetic and cpu control group

                case 0x56: SetInterruptMode(0); break; // IM 0

                #endregion

                default:
                    throw new NotImplementedException($"Unimplemented opcode: 0xED {opcode:X2} at address 0x{_pc - 2:X4}");
            }
        }

        private void ExecuteDDFDPrefixedOpcode(byte prefix, byte opcode)
        {
            ref PairedRegister GetRegisterForPrefix()
            {
                switch (prefix)
                {
                    case 0xDD: return ref _ix;
                    case 0xFD: return ref _iy;

                    default: throw new ArgumentOutOfRangeException($"Prefix byte 0x{prefix:X2} passed to ExecutePrefixedOpcode");
                }
            }

            ushort Displace(ushort address, byte displacement)
            {
                _clock.AddCycles(5);
                return (ushort)(address + (sbyte)displacement);
            }

            ref PairedRegister reg = ref GetRegisterForPrefix();

            switch (opcode)
            {
                #region 8-bit load group

                case 0x46: _gpr.B = ReadByte(Displace(reg, ReadByte(_pc++))); break; // LD B,(IX/IY + d)
                case 0x4E: _gpr.C = ReadByte(Displace(reg, ReadByte(_pc++))); break; // LD C,(IX/IY + d)
                case 0x56: _gpr.D = ReadByte(Displace(reg, ReadByte(_pc++))); break; // LD D,(IX/IY + d)
                case 0x5E: _gpr.E = ReadByte(Displace(reg, ReadByte(_pc++))); break; // LD E,(IX/IY + d)
                case 0x66: _gpr.H = ReadByte(Displace(reg, ReadByte(_pc++))); break; // LD H,(IX/IY + d)
                case 0x6E: _gpr.L = ReadByte(Displace(reg, ReadByte(_pc++))); break; // LD L,(IX/IY + d)
                case 0x7E: _afr.A = ReadByte(Displace(reg, ReadByte(_pc++))); break; // LD A,(IX/IY + d)

                case 0x70: WriteByte(Displace(reg, ReadByte(_pc++)), _gpr.B); break; // LD (IX/IY + d),B
                case 0x71: WriteByte(Displace(reg, ReadByte(_pc++)), _gpr.C); break; // LD (IX/IY + d),C
                case 0x72: WriteByte(Displace(reg, ReadByte(_pc++)), _gpr.D); break; // LD (IX/IY + d),D
                case 0x73: WriteByte(Displace(reg, ReadByte(_pc++)), _gpr.E); break; // LD (IX/IY + d),E
                case 0x74: WriteByte(Displace(reg, ReadByte(_pc++)), _gpr.H); break; // LD (IX/IY + d),H
                case 0x75: WriteByte(Displace(reg, ReadByte(_pc++)), _gpr.L); break; // LD (IX/IY + d),L
                case 0x77: WriteByte(Displace(reg, ReadByte(_pc++)), _afr.A); break; // LD (IX/IY + d),A

                case 0x36: WriteByte(Displace(reg, ReadByte(_pc++)), _bus.ReadByte(_pc++)); break; // LD (IX/IY + d),n

                #endregion

                #region 16-bit load group

                case 0x26: reg = (ushort)((ReadByte(_pc++) << 8) | (reg & 0x00FF)); break; // LD IX/IY H,n
                case 0x2E: reg = (ushort)((reg & 0xFF00) | ReadByte(_pc++)); break; // LD IX/IY L,n

                case 0x21: reg = ReadWord(_pc); _pc += 2; break; // LD IX/IY,nn

                case 0x2A: reg = ReadWord(ReadWord(_pc)); _pc += 2; break; // LD IX/IY,(nn)    

                case 0x22: WriteWord(ReadWord(_pc), reg); _pc += 2; break; // LD (nn),IX/IY

                case 0xE5: PushWord(reg); break; // PUSH IX/IY

                case 0xE1: reg = PopWord(); break; // POP IX/IY

                #endregion

                #region 8-bit arithmetic group

                case 0x24: reg.hi = Inc8Bit(reg.hi); break;                                                                             // INC IXH/IYH
                case 0x2C: reg.lo = Inc8Bit(reg.lo); break;                                                                             // INC IXL/IYL

                case 0x25: reg.hi = Dec8Bit(reg.hi); break;                                                                             // DEC IXH/IYH
                case 0x2D: reg.lo = Dec8Bit(reg.lo); break;                                                                             // DEC IXL/IYL

                #endregion

                #region general-purpose arithmetic and cpu control group

                case 0xCB: ExecuteDisplacedCBPrefixedOpcode(Displace(reg, ReadByte(_pc++)), _bus.ReadByte(_pc++)); break;

                #endregion

                #region 16-bit arithmetic group

                case 0x23: reg++; _clock.AddCycles(2); break; // INC IX/IY
                case 0x2B: reg--; _clock.AddCycles(2); break; // DEC IX/IY

                #endregion

                default:
                    throw new NotImplementedException($"Unimplemented opcode: 0x{prefix:X2} {opcode:X2} at address 0x{_pc - 2:X4}");
            }
        }

        #region exchange, block transfer, and search group handlers

        private void LoadAndIncrement()
        {
            byte data = ReadByte(_gpr.HL++);

            WriteByte(_gpr.DE++, data);

            _clock.AddCycles(2);

            var temp = data + _afr.A;

            _afr.F[B3] = temp.Bit(3);
            _afr.F[B5] = temp.Bit(1);

            _afr.F[H | N] = false;
            _afr.F[V] = --_gpr.BC != 0;
        }

        private void LoadIncrementAndRepeat()
        {
            LoadAndIncrement();

            if (_gpr.BC == 0)
                return;

            _clock.AddCycles(5);

            _pc -= 2;
        }

        private void LoadAndDecrement()
        {
            byte data = ReadByte(_gpr.HL--);

            WriteByte(_gpr.DE--, data);

            _clock.AddCycles(2);

            var temp = data + _afr.A;

            _afr.F[B3] = temp.Bit(3);
            _afr.F[B5] = temp.Bit(1);

            _afr.F[H | N] = false;
            _afr.F[V] = --_gpr.BC != 0;
        }

        private void LoadDecrementAndRepeat()
        {
            LoadAndDecrement();

            if (_gpr.BC == 0)
                return;

            _clock.AddCycles(5);

            _pc -= 2;
        }

        private void CompareAndIncrement()
        {
            var (result, flags) = Sub8Bit(_afr.A, ReadByte(_gpr.HL++));

            _clock.AddCycles(5);

            var temp = result - (flags[H] ? 1 : 0);

            flags[B3] = temp.Bit(3);
            flags[B5] = temp.Bit(1);

            flags[C] = _afr.F[C];
            flags[V] = --_gpr.BC != 0;

            _afr.F = flags;
        }

        private void CompareIncrementAndRepeat()
        {
            CompareAndIncrement();

            if (_gpr.BC == 0 || _afr.F[Z])
            {
                return;
            }

            _clock.AddCycles(5);

            _pc -= 2;
        }

        private void CompareAndDecrement()
        {
            var (result, flags) = Sub8Bit(_afr.A, ReadByte(_gpr.HL--));

            _clock.AddCycles(5);

            var temp = result - (flags[H] ? 1 : 0);

            flags[B3] = temp.Bit(3);
            flags[B5] = temp.Bit(1);

            flags[C] = _afr.F[C];
            flags[V] = --_gpr.BC != 0;

            _afr.F = flags;
        }

        private void CompareDecrementAndRepeat()
        {
            CompareAndDecrement();

            if (_gpr.BC == 0 || _afr.F[Z])
            {
                return;
            }

            _clock.AddCycles(5);

            _pc -= 2;
        }

        #endregion

        #region 8-bit arithmetic group handlers

        private static (byte, StatusFlags) And8Bit(byte a, byte b)
        {
            int result = a & b;

            StatusFlags flags = FlagsSZP[result] | H;

            return ((byte)result, flags);
        }

        // https://stackoverflow.com/questions/8034566/overflow-and-carry-flags-on-z80/8037485#8037485
        private static (byte, StatusFlags) Add8Bit(byte a, byte b, bool carry = false)
        {
            int result = a + b + (carry ? 1 : 0);

            int carryIn = result ^ a ^ b;

            result &= 0xFF;

            StatusFlags flags = FlagsSZP[result];

            flags[H] = (carryIn & 0x10) == 0x10;
            flags[V] = ((carryIn >> 7) & 0x01) != (carryIn >> 8);
            flags[C] = (carryIn & 0x100) == 0x100;

            return ((byte)result, flags);
        }

        // https://stackoverflow.com/questions/8034566/overflow-and-carry-flags-on-z80/8037485#8037485
        private static (byte, StatusFlags) Sub8Bit(byte a, byte b, bool carry = false)
        {
            // a - b - c = a + ~b + 1 - c = a + ~b + !c
            var (result, flags) = Add8Bit(a, (byte)(~b), !carry);

            flags ^= C | H;
            flags |= N;

            return (result, flags);
        }

        private static (byte, StatusFlags) Or8Bit(byte a, byte b)
        {
            var result = a | b;

            StatusFlags flags = FlagsSZP[result];

            return ((byte)result, flags);
        }

        private static (byte, StatusFlags) Xor8Bit(byte a, byte b)
        {
            var result = a ^ b;

            StatusFlags flags = FlagsSZP[result];

            return ((byte)result, flags);
        }

        private static StatusFlags Compare8Bit(byte a, byte b)
        {
            var (_, flags) = Sub8Bit(a, b);

            return flags;
        }

        private byte Inc8Bit(byte a)
        {
            var (result, flags) = Add8Bit(a, 1);

            _afr.F &= C;
            _afr.F |= (flags & ~C);

            return result;
        }

        private byte Dec8Bit(byte a)
        {
            var (result, flags) = Sub8Bit(a, 1);

            _afr.F &= C;
            _afr.F |= (flags & ~C);

            return result;
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

        #region 16-bit arithmetic group

        private (ushort, StatusFlags) Add16Bit(ushort a, ushort b, bool carry = false)
        {
            byte hi, lo;
            StatusFlags flags = default;

            _clock.AddCycles(4);
            (lo, flags) = Add8Bit((byte)(a >> 0), (byte)(b >> 0), carry);

            _clock.AddCycles(3);
            (hi, flags) = Add8Bit((byte)(a >> 8), (byte)(b >> 8), flags[C]);

            ushort result = (ushort)(hi << 8 | lo);

            flags[Z] = result == 0;

            return ((ushort)(hi << 8 | lo), flags);
        }

        private void AddHL(ushort value)
        {
            // reset affected flags
            _afr.F[B5 | H | B3 | N | C] = false;

            var (result, flags) = Add16Bit(_gpr.HL, value);

            _afr.F |= flags & (B5 | H | B3 | N | C);

            _gpr.HL = result;
        }

        #endregion

        #region rotate and shift group handlers

        private void RotateLeftAccumulator()
        {
            // reset affected flags
            _afr.F[H | N | C] = false;

            bool b7 = _afr.A.Bit(7);

            _afr.A = (byte)((_afr.A << 1) | (b7 ? 1 : 0));

            _afr.F[B5] = _afr.A.Bit(5);
            _afr.F[B3] = _afr.A.Bit(3);
            _afr.F[C] = b7;
        }

        private byte RotateLeft(byte b)
        {
            bool b7 = b.Bit(7);

            byte result = (byte)((b << 1) | (b7 ? 1 : 0));

            StatusFlags flags = FlagsSZP[result];
            flags[C] = b7;

            _afr.F = flags;

            return result;
        }

        private byte RotateLeftThroughCarry(byte b)
        {
            byte result = (byte)((b << 1) | (_afr.F[C] ? 1 : 0));

            StatusFlags flags = FlagsSZP[result];
            flags[C] = b.Bit(7);

            _afr.F = flags;

            return result;
        }

        private void RotateRightAccumulator()
        {
            // reset affected flags
            _afr.F[H | N | C] = false;

            bool b0 = _afr.A.Bit(0);

            _afr.A = (byte)((_afr.A >> 1) | (b0 ? 0x80 : 0));

            _afr.F[B5] = _afr.A.Bit(5);
            _afr.F[B3] = _afr.A.Bit(3);
            _afr.F[C] = b0;
        }

        private byte RotateRight(byte b)
        {
            bool b0 = b.Bit(0);

            byte result = (byte)((b >> 1) | (b0 ? 0x80 : 0));

            StatusFlags flags = FlagsSZP[result];
            flags[C] = b0;

            _afr.F = flags;

            return result;
        }

        private byte RotateRightThroughCarry(byte b)
        {
            byte result = (byte)((b >> 1) | (_afr.F[C] ? 0x80 : 0));

            StatusFlags flags = FlagsSZP[result];
            flags[C] = b.Bit(0);

            _afr.F = flags;

            return result;
        }

        private byte ShiftLeftArithmetic(byte b)
        {
            byte result = (byte)(b << 1);

            StatusFlags flags = FlagsSZP[result];
            flags[C] = b.Bit(7);

            _afr.F = flags;

            return result;
        }

        private byte ShiftLeftInsertingOne(byte b)
        {
            byte result = (byte)((b << 1) | 1);

            StatusFlags flags = FlagsSZP[result];
            flags[C] = b.Bit(7);

            _afr.F = flags;

            return result;
        }

        private byte ShiftRightArithmetic(byte b)
        {
            byte result = (byte)((sbyte)b >> 1);

            StatusFlags flags = FlagsSZP[result];
            flags[C] = b.Bit(0);

            _afr.F = flags;

            return result;
        }

        private byte ShiftRightLogical(byte b)
        {
            byte result = (byte)(b >> 1);

            StatusFlags flags = FlagsSZP[result];
            flags[C] = b.Bit(0);

            _afr.F = flags;

            return result;
        }

        #endregion

        #region jump group handlers

        private void JumpImmediate()
        {
            ushort address = ReadWord(_pc);

            if (address == _pc - 1)
            {
                throw new InfiniteLoopException();
            }

            _pc = address;
        }

        private void JumpImmediate(bool condition)
        {
            ushort address = ReadWord(_pc);

            if (condition)
            {
                _pc = address;
            }
            else
            {
                _pc += 2;
            }
        }

        private void JumpRelative()
        {
            _clock.AddCycles(5);
            sbyte offset = (sbyte)ReadByte(_pc++);
            _pc += (ushort)offset;
        }

        private void JumpRelative(bool condition)
        {
            sbyte offset = (sbyte)ReadByte(_pc++);

            if (condition)
            {
                _clock.AddCycles(5);
                _pc += (ushort)offset;
            }
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

        private void CallImmediate(bool condition)
        {
            ushort address = ReadWord(_pc);
            _pc += 2;

            if (condition)
            {
                _clock.AddCycles(1);
                PushWord(_pc);
                _pc = address;
            }
        }

        private void Return()
        {
            _pc = PopWord();
        }

        private void Return(bool condition)
        {
            if (condition)
            {
                _pc = PopWord();
            }
        }

        #endregion
    }

    static class IntExtensions
    {
        internal static bool Bit(this byte v, int bit)
        {
            int mask = 1 << bit;

            return (v & mask) == mask;
        }

        internal static bool Bit(this int v, int bit)
        {
            int mask = 1 << bit;

            return (v & mask) == mask;
        }
    }
}
