using System;
using System.Diagnostics;
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

            //Debug.WriteLine($"PC: 0x{_pc:X4}");

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

                case 0x32: WriteByte(ReadWord(_pc), _afr.A); _pc += 2; break; // LD (nn),A

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

                case 0xEB: ushort temp = _gpr.DE; _gpr.DE = _gpr.HL; _gpr.HL = temp; break; // EX DE,HL

                #endregion

                #region 8-bit arithmetic group

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

                case 0xED: ExecuteEDPrefixOpcode(ReadOpcode(_pc++)); break;
                case 0xFD: ExecuteFDPrefixOpcode(ReadOpcode(_pc++)); break;

                case 0xF3: DisableInterrupts(); break; // DI

                #endregion

                #region 16-bit arithmetic group

                case 0x09: _gpr.HL = Add16Bit(_gpr.HL, _gpr.BC); break; // ADD HL,BC
                case 0x19: _gpr.HL = Add16Bit(_gpr.HL, _gpr.DE); break; // ADD HL,DE
                case 0x29: _gpr.HL = Add16Bit(_gpr.HL, _gpr.HL); break; // ADD HL,HL
                case 0x39: _gpr.HL = Add16Bit(_gpr.HL, _sp); break; // ADD HL,SP

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

                case 0x07: Rlca(); break; // RLCA

                case 0x0F: Rrca(); break; // RRCA

                #endregion

                #region jump group

                case 0xC3: JumpImmediate(); break; // JP nn

                case 0xC2: JumpImmediate(!_afr.F[Z]); break; // JP NZ,nn
                case 0xCA: JumpImmediate(_afr.F[Z]); break; // JP Z,nn

                case 0x18: JumpRelative(); break; // JR e

                case 0x28: JumpRelative(_afr.F[Z]); break; // JR Z,e

                #endregion

                #region call and return group

                case 0xCD: CallImmediate(); break; // CALL nn

                case 0xC4: CallImmediate(!_afr.F[Z]); break; // CALL NZ,nn

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
                #region 16-bit load group

                case 0x43: WriteWord(ReadWord(_pc), _gpr.BC); _pc += 2; break; // LD (nn),BC
                case 0x53: WriteWord(ReadWord(_pc), _gpr.DE); _pc += 2; break; // LD (nn),DE
                case 0x63: WriteWord(ReadWord(_pc), _gpr.HL); _pc += 2; break; // LD (nn),HL
                case 0x73: WriteWord(ReadWord(_pc), _sp); _pc += 2; break; // LD (nn),SP

                #endregion

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

        private void ExecuteFDPrefixOpcode(byte opcode)
        {
            switch (opcode)
            {

                default:
                    throw new NotImplementedException($"Unimplemented opcode: 0xFD {opcode:X2} at address 0x{_pc - 2:X4}");
            }
        }

        #region exchange, block transfer, and search group handlers

        private void LoadAndIncrement()
        {
            byte data = ReadByte(_gpr.HL++);

            WriteByte(_gpr.DE++, data);

            _clock.AddCycles(2);

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

        #endregion

        #region 8-bit arithmetic group handlers

        private static (byte, StatusFlags) And8Bit(byte a, byte b)
        {
            int result = a & b;

            StatusFlags flags = H;

            flags[S] = (result & 0x80) == 0x80;
            flags[Z] = result == 0;
            flags[P] = EvenParity(result);

            return ((byte)result, flags);
        }

        // https://stackoverflow.com/questions/8034566/overflow-and-carry-flags-on-z80/8037485#8037485
        private static (byte, StatusFlags) Add8Bit(byte a, byte b, bool carry = false)
        {
            int result = a + b + (carry ? 1 : 0);

            int carryIn = result ^ a ^ b;

            result &= 0xFF;

            StatusFlags flags = default;

            // sign 
            flags[S] = (result & 0x80) == 0x80;

            // zero
            flags[Z] = result == 0;

            // half carry
            flags[H] = (carryIn & 0x10) == 0x10;

            // overflow
            flags[V] = ((carryIn >> 7) & 0x01) != (carryIn >> 8);

            // carry
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

            StatusFlags flags = default;

            flags[S] = (result & 0x80) == 0x80;
            flags[Z] = result == 0;
            flags[P] = EvenParity(result);

            return ((byte)result, flags);
        }

        private static (byte, StatusFlags) Xor8Bit(byte a, byte b)
        {
            var result = a ^ b;

            StatusFlags flags = default;

            flags[S] = (result & 0x80) == 0x80;
            flags[Z] = result == 0;
            flags[P] = EvenParity(result);

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

        private ushort Add16Bit(ushort a, ushort b, bool carry = false)
        {
            // reset affected flags
            _afr.F[H | N | C] = false;

            byte hi, lo;
            StatusFlags flags = default;

            _clock.AddCycles(4);
            (lo, flags) = Add8Bit((byte)(a >> 0), (byte)(b >> 0), carry);

            _clock.AddCycles(3);
            (hi, flags) = Add8Bit((byte)(a >> 8), (byte)(b >> 8), flags[C]);

            // apply masked result flags to flags register
            _afr.F |= flags & (H | N | C);

            return (ushort)(hi << 8 | lo);
        }

        #endregion

        #region rotate and shift group handlers

        private void Rlca()
        {
            // reset affected flags
            _afr.F[H | N | C] = false;

            int c = (_afr.A >> 7) & 1;

            _afr.A = (byte)((_afr.A << 1) | c);

            if (c == 1)
            {
                _afr.F |= C;
            }
        }

        private void Rrca()
        {
            // reset affected flags
            _afr.F[H | N | C] = false;

            int c = _afr.A & 1;

            _afr.A = (byte)((_afr.A >> 1) | (c << 7));

            if (c == 1)
            {
                _afr.F |= C;
            }
        }

        #endregion

        #region jump group handlers

        private void JumpImmediate()
        {
            _pc = ReadWord(_pc);
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

        #endregion

        private static bool EvenParity(int v)
        {
            v ^= v >> 4;
            v ^= v >> 2;
            v ^= v >> 1;

            return v == 0;
        }
    }
}
