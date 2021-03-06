﻿using System;
using System.Runtime.CompilerServices;
using elbsms_core.Memory;

namespace elbsms_core.CPU
{
    using static StatusFlags;

    internal partial class Z80
    {
        private static byte[] FlagsSZP;

        private readonly SystemClock _clock;
        private readonly Interconnect _interconnect;

        private int _activeAFRegisters;
        private readonly AFRegisters[] _afRegisters;
        private AFRegisters _afr;

        private int _activeGPRegisters;
        private readonly GPRegisters[] _gpRegisters;
        private GPRegisters _gpr;

        private ushort _pc, _sp;

        private PairedRegister _ix, _iy;

        private PairedRegister _memPtr;

        private byte _r;
        private byte _i;

        private bool _iff1, _iff2;
        private int _interruptMode;
        private bool _enableInterrupts;

        private bool _nmiPending;
        private bool _intPending;

        public void RaiseNMI() => _nmiPending = true;
        public void RaiseINT() => _intPending = true;

        #region static initialisation

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
                pf = i.EvenParity() ? P : 0;

                FlagsSZP[i] = (byte)(sf | zf | b5 | b3 | pf);
            }
        }

        #endregion

        public Z80(SystemClock clock, Interconnect interconnect)
        {
            _clock = clock;
            _interconnect = interconnect;

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
            _clock.AddCycles(2);
            byte opcode = _interconnect.ReadByte(address);
            _clock.AddCycles(2);
            return opcode;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private byte ReadByte(ushort address)
        {
            _clock.AddCycles(2);
            byte value = _interconnect.ReadByte(address);
            _clock.AddCycles(1);
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
            _interconnect.WriteByte(address, value);
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
            if (_enableInterrupts)
            {
                _iff1 = _iff2 = true;
                _enableInterrupts = false;
                return;
            }

            if (_nmiPending)
            {
                _nmiPending = false;
                _iff2 = _iff1;
                _iff1 = false;

                IncrementMemoryRefreshRegister();
                _clock.AddCycles(4); // timing of an opcode fetch
                Reset(0x66);
                return;
            }

            if (_intPending && _iff1)
            {
                _iff1 = _iff2 = false;
                IncrementMemoryRefreshRegister();

                // timings from http://www.z80.info/interrup.htm
                switch (_interruptMode)
                {
                    case 0:
                    // mode 0 should read from the data bus but this is not used in the master system
                    // open bus reads FF which is RST 38H so fall through to mode 1 calling RST 38H
                    case 1:
                        _clock.AddCycles(6); // a normal opcode fetch plus two wait cycles
                        Reset(0x38);
                        break;

                    case 2: throw new NotImplementedException("Interrupt Mode 2 not implemented");
                }
            }
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

                case 0x0A: _afr.A = ReadByte(_gpr.BC); _memPtr.word = (ushort)(_gpr.BC + 1); break; // LD A,(BC)
                case 0x1A: _afr.A = ReadByte(_gpr.DE); _memPtr.word = (ushort)(_gpr.DE + 1); break; // LD A,(DE)
                case 0x3A: { ushort address = ReadWord(_pc); _afr.A = ReadByte(address); _pc += 2; _memPtr.word = (ushort)(address + 1); } break; // LD A,(nn)

                case 0x02: WriteByte(_gpr.BC, _afr.A); _memPtr.lo = (byte)(_gpr.BC + 1); _memPtr.hi = _afr.A; break; // LD (BC),A
                case 0x12: WriteByte(_gpr.DE, _afr.A); _memPtr.lo = (byte)(_gpr.DE + 1); _memPtr.hi = _afr.A; break; // LD (DE),A
                case 0x32: { ushort addr = ReadWord(_pc); WriteByte(addr, _afr.A); _pc += 2; _memPtr.lo = (byte)(addr + 1); _memPtr.hi = _afr.A; } break; // LD (nn),A

                #endregion

                #region 16-bit load group

                case 0x01: _gpr.BC = ReadWord(_pc); _pc += 2; break; // LD BC,nn
                case 0x11: _gpr.DE = ReadWord(_pc); _pc += 2; break; // LD DE,nn
                case 0x21: _gpr.HL = ReadWord(_pc); _pc += 2; break; // LD HL,nn
                case 0x31: _sp = ReadWord(_pc); _pc += 2; break;     // LD SP,nn

                case 0xF9: _clock.AddCycles(2); _sp = _gpr.HL; break; // LD SP,HL

                case 0x2A: { ushort address = ReadWord(_pc); _gpr.HL = ReadWord(address); _pc += 2; _memPtr.word = (ushort)(address + 1); } break; // LD HL,(nn)

                case 0x22: { ushort address = ReadWord(_pc); WriteWord(address, _gpr.HL); _pc += 2; _memPtr.word = (ushort)(address + 1); } break; // LD (nn),HL

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

                case 0xEB: { ushort temp = _gpr.DE; _gpr.DE = _gpr.HL; _gpr.HL = temp; } break; // EX DE,HL

                case 0x08: SwitchAFRegisters(); break; // EX AF,AF'
                case 0xD9: SwitchGPRegistsers(); break; // EXX

                case 0xE3:
                {
                    ushort temp = ReadWord(_sp);
                    _clock.AddCycles(1);
                    WriteWord(_sp, _gpr.HL);
                    _clock.AddCycles(2);
                    _gpr.HL = temp;
                    _memPtr.word = temp;
                }
                break; // EX (SP),HL

                #endregion

                #region 8-bit arithmetic group

                case 0x80: (_afr.A, _afr.F) = Add8Bit(_afr.A, _gpr.B); break;               // ADD B
                case 0x81: (_afr.A, _afr.F) = Add8Bit(_afr.A, _gpr.C); break;               // ADD C
                case 0x82: (_afr.A, _afr.F) = Add8Bit(_afr.A, _gpr.D); break;               // ADD D
                case 0x83: (_afr.A, _afr.F) = Add8Bit(_afr.A, _gpr.E); break;               // ADD E
                case 0x84: (_afr.A, _afr.F) = Add8Bit(_afr.A, _gpr.H); break;               // ADD H
                case 0x85: (_afr.A, _afr.F) = Add8Bit(_afr.A, _gpr.L); break;               // ADD L
                case 0x86: (_afr.A, _afr.F) = Add8Bit(_afr.A, ReadByte(_gpr.HL)); break;    // ADD (HL)
                case 0x87: (_afr.A, _afr.F) = Add8Bit(_afr.A, _afr.A); break;               // ADD A
                case 0xC6: (_afr.A, _afr.F) = Add8Bit(_afr.A, ReadByte(_pc++)); break;      // ADD A,n

                case 0x88: (_afr.A, _afr.F) = Add8Bit(_afr.A, _gpr.B, _afr.F[C]); break;            // ADC B
                case 0x89: (_afr.A, _afr.F) = Add8Bit(_afr.A, _gpr.C, _afr.F[C]); break;            // ADC C
                case 0x8A: (_afr.A, _afr.F) = Add8Bit(_afr.A, _gpr.D, _afr.F[C]); break;            // ADC D
                case 0x8B: (_afr.A, _afr.F) = Add8Bit(_afr.A, _gpr.E, _afr.F[C]); break;            // ADC E
                case 0x8C: (_afr.A, _afr.F) = Add8Bit(_afr.A, _gpr.H, _afr.F[C]); break;            // ADC H
                case 0x8D: (_afr.A, _afr.F) = Add8Bit(_afr.A, _gpr.L, _afr.F[C]); break;            // ADC L
                case 0x8E: (_afr.A, _afr.F) = Add8Bit(_afr.A, ReadByte(_gpr.HL), _afr.F[C]); break; // ADC (HL)
                case 0x8F: (_afr.A, _afr.F) = Add8Bit(_afr.A, _afr.A, _afr.F[C]); break;            // ADC A
                case 0xCE: (_afr.A, _afr.F) = Add8Bit(_afr.A, ReadByte(_pc++), _afr.F[C]); break;   // ADC A,n

                case 0x90: (_afr.A, _afr.F) = Sub8Bit(_afr.A, _gpr.B); break;               // SUB B
                case 0x91: (_afr.A, _afr.F) = Sub8Bit(_afr.A, _gpr.C); break;               // SUB C
                case 0x92: (_afr.A, _afr.F) = Sub8Bit(_afr.A, _gpr.D); break;               // SUB D
                case 0x93: (_afr.A, _afr.F) = Sub8Bit(_afr.A, _gpr.E); break;               // SUB E
                case 0x94: (_afr.A, _afr.F) = Sub8Bit(_afr.A, _gpr.H); break;               // SUB H
                case 0x95: (_afr.A, _afr.F) = Sub8Bit(_afr.A, _gpr.L); break;               // SUB L
                case 0x96: (_afr.A, _afr.F) = Sub8Bit(_afr.A, ReadByte(_gpr.HL)); break;    // SUB (HL)
                case 0x97: (_afr.A, _afr.F) = Sub8Bit(_afr.A, _afr.A); break;               // SUB A
                case 0xD6: (_afr.A, _afr.F) = Sub8Bit(_afr.A, ReadByte(_pc++)); break;      // SUB A,n

                case 0x98: (_afr.A, _afr.F) = Sub8Bit(_afr.A, _gpr.B, _afr.F[C]); break;            // SBC B
                case 0x99: (_afr.A, _afr.F) = Sub8Bit(_afr.A, _gpr.C, _afr.F[C]); break;            // SBC C
                case 0x9A: (_afr.A, _afr.F) = Sub8Bit(_afr.A, _gpr.D, _afr.F[C]); break;            // SBC D
                case 0x9B: (_afr.A, _afr.F) = Sub8Bit(_afr.A, _gpr.E, _afr.F[C]); break;            // SBC E
                case 0x9C: (_afr.A, _afr.F) = Sub8Bit(_afr.A, _gpr.H, _afr.F[C]); break;            // SBC H
                case 0x9D: (_afr.A, _afr.F) = Sub8Bit(_afr.A, _gpr.L, _afr.F[C]); break;            // SBC L
                case 0x9E: (_afr.A, _afr.F) = Sub8Bit(_afr.A, ReadByte(_gpr.HL), _afr.F[C]); break; // SBC (HL)
                case 0x9F: (_afr.A, _afr.F) = Sub8Bit(_afr.A, _afr.A, _afr.F[C]); break;            // SBC A
                case 0xDE: (_afr.A, _afr.F) = Sub8Bit(_afr.A, ReadByte(_pc++), _afr.F[C]); break;   // SBC A,n

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

                case 0x27: DecimalAdjustAccumulator(); break; // DAA
                case 0x2F: _afr.A = (byte)~_afr.A; _afr.F = (_afr.F & (S | Z | P | C)) | H | N | (_afr.A & (B5 | B3)); break; // CPL

                case 0x3F: _afr.F[H] = _afr.F[C]; _afr.F = ((_afr.F & (S | Z | H | P | C)) ^ C) | (_afr.A & (B5 | B3)); break; // CCF
                case 0x37: _afr.F = (_afr.F & (S | Z | P)) | C | (_afr.A & (B5 | B3)); break; // SCF

                case 0xCB: ExecuteCBPrefixedOpcode(ReadOpcode(_pc++)); break;
                case 0xDD: ExecuteDDFDPrefixedOpcode(opcode, ReadOpcode(_pc++)); break;
                case 0xED: ExecuteEDPrefixedOpcode(ReadOpcode(_pc++)); break;
                case 0xFD: ExecuteDDFDPrefixedOpcode(opcode, ReadOpcode(_pc++)); break;

                case 0x00: break; // NOP

                case 0xF3: DisableInterrupts(); break; // DI
                case 0xFB: _enableInterrupts = true; break; // EI

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

                case 0x07: RotateLeftAccumulator(); break; // RLCA
                case 0x17: RotateLeftThroughCarryAccumulator(); break; // RLA
                case 0x0F: RotateRightAccumulator(); break; // RRCA
                case 0x1F: RotateRightThroughCarryAccumulator(); break; // RRA

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

                case 0x30: JumpRelative(!_afr.F[C]); break; // JR NC,e
                case 0x38: JumpRelative(_afr.F[C]); break; // JR C,e

                case 0x20: JumpRelative(!_afr.F[Z]); break; // JR NZ,e
                case 0x28: JumpRelative(_afr.F[Z]); break; // JR Z,e

                case 0x10: DecrementAndJumpNotZero(); break; // DJNZ e

                case 0xE9: _pc = _gpr.HL; break; // JP HL

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

                case 0xC7: Reset(0x00); break; // RST 00H`
                case 0xCF: Reset(0x08); break; // RST 08H
                case 0xD7: Reset(0x10); break; // RST 10H
                case 0xDF: Reset(0x18); break; // RST 18H
                case 0xE7: Reset(0x20); break; // RST 20H
                case 0xEF: Reset(0x28); break; // RST 28H
                case 0xF7: Reset(0x30); break; // RST 30H
                case 0xFF: Reset(0x38); break; // RST 38H

                #endregion

                #region input and output group

                case 0xDB: { byte temp = _afr.A; _afr.A = In(ReadByte(_pc++)); _memPtr.word = (ushort)((temp << 8) + _afr.A + 1); } break; // IN A,(n)
                case 0xD3: { byte port = ReadByte(_pc++); Out(port, _afr.A); _memPtr.lo = (byte)(port + 1); _memPtr.hi = _afr.A; } break; // OUT (n),A

                #endregion

                default:
                    throw new NotImplementedException($"Unimplemented opcode: 0x{opcode:X2} at address 0x{_pc - 1:X4}");
            }
        }

        private void ExecuteEDPrefixedOpcode(byte opcode)
        {
            switch (opcode)
            {
                #region 8-bit load group

                case 0x47: _i = _afr.A; _clock.AddCycles(1); break; // LD I,A
                case 0x4F: _r = _afr.A; _clock.AddCycles(1); break; // LD R,A
                case 0x57: _afr.A = _i; _clock.AddCycles(1); break; // LD A,I
                case 0x5F: _afr.A = _r; _clock.AddCycles(1); break; // LD A,R

                #endregion

                #region 16-bit load group

                case 0x4B: { ushort address = ReadWord(_pc); _gpr.BC = ReadWord(address); _pc += 2; _memPtr.word = (ushort)(address + 1); } break; // LD BC,(nn)
                case 0x5B: { ushort address = ReadWord(_pc); _gpr.DE = ReadWord(address); _pc += 2; _memPtr.word = (ushort)(address + 1); } break; // LD DE,(nn)
                case 0x6B: { ushort address = ReadWord(_pc); _gpr.HL = ReadWord(address); _pc += 2; _memPtr.word = (ushort)(address + 1); } break; // LD HL,(nn)
                case 0x7B: { ushort address = ReadWord(_pc); _sp = ReadWord(address); _pc += 2; _memPtr.word = (ushort)(address + 1); } break; // LD SP,(nn)

                case 0x43: { ushort address = ReadWord(_pc); WriteWord(address, _gpr.BC); _pc += 2; _memPtr.word = (ushort)(address + 1); } break; // LD (nn),BC
                case 0x53: { ushort address = ReadWord(_pc); WriteWord(address, _gpr.DE); _pc += 2; _memPtr.word = (ushort)(address + 1); } break; // LD (nn),DE
                case 0x63: { ushort address = ReadWord(_pc); WriteWord(address, _gpr.HL); _pc += 2; _memPtr.word = (ushort)(address + 1); } break; // LD (nn),HL
                case 0x73: { ushort address = ReadWord(_pc); WriteWord(address, _sp); _pc += 2; _memPtr.word = (ushort)(address + 1); } break; // LD (nn),SP

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

                case 0x44: (_afr.A, _afr.F) = Sub8Bit(0, _afr.A); break; // NEG

                case 0x46:
                case 0x4E:
                case 0x66:
                case 0x6E: SetInterruptMode(0); break; // IM 0

                case 0x56:
                case 0x76: SetInterruptMode(1); break; // IM 1

                case 0x5E:
                case 0x7E: SetInterruptMode(2); break; // IM 2

                #endregion

                #region 16-bit arithmetic group

                case 0x4A: (_gpr.HL, _afr.F) = AddWithCarry16Bit(_gpr.HL, _gpr.BC, _afr.F[C]); break;   // ADC HL,BC
                case 0x5A: (_gpr.HL, _afr.F) = AddWithCarry16Bit(_gpr.HL, _gpr.DE, _afr.F[C]); break;   // ADC HL,DE
                case 0x6A: (_gpr.HL, _afr.F) = AddWithCarry16Bit(_gpr.HL, _gpr.HL, _afr.F[C]); break;   // ADC HL,HL
                case 0x7A: (_gpr.HL, _afr.F) = AddWithCarry16Bit(_gpr.HL, _sp, _afr.F[C]); break;       // ADC HL,SP


                case 0x42: (_gpr.HL, _afr.F) = SubWithCarry16Bit(_gpr.HL, _gpr.BC, _afr.F[C]); break;   // SBC HL,BC
                case 0x52: (_gpr.HL, _afr.F) = SubWithCarry16Bit(_gpr.HL, _gpr.DE, _afr.F[C]); break;   // SBC HL,DE
                case 0x62: (_gpr.HL, _afr.F) = SubWithCarry16Bit(_gpr.HL, _gpr.HL, _afr.F[C]); break;   // SBC HL,HL
                case 0x72: (_gpr.HL, _afr.F) = SubWithCarry16Bit(_gpr.HL, _sp, _afr.F[C]); break;       // SBC HL,SP

                #endregion

                #region rotate and shift group

                case 0x67: RotateRightDigit(); break; // RRD
                case 0x6F: RotateLeftDigit(); break; // RLD

                #endregion

                #region call and return group

                case 0x45:
                case 0x55:
                case 0x5D:
                case 0x65:
                case 0x6D:
                case 0x75:
                case 0x7D: _iff1 = _iff2; Return(); break; // RETN

                case 0x4D: Return(); break; // RETI

                #endregion

                #region input and output group

                case 0x40: _gpr.B = In(_gpr.C); _afr.F = (_afr.F & C) | (FlagsSZP[_gpr.B] & ~C); break; // IN B,(C)
                case 0x48: _gpr.C = In(_gpr.C); _afr.F = (_afr.F & C) | (FlagsSZP[_gpr.C] & ~C); break; // IN C,(C)
                case 0x50: _gpr.D = In(_gpr.C); _afr.F = (_afr.F & C) | (FlagsSZP[_gpr.D] & ~C); break; // IN D,(C)
                case 0x58: _gpr.E = In(_gpr.C); _afr.F = (_afr.F & C) | (FlagsSZP[_gpr.E] & ~C); break; // IN E,(C)
                case 0x60: _gpr.H = In(_gpr.C); _afr.F = (_afr.F & C) | (FlagsSZP[_gpr.H] & ~C); break; // IN H,(C)
                case 0x68: _gpr.L = In(_gpr.C); _afr.F = (_afr.F & C) | (FlagsSZP[_gpr.L] & ~C); break; // IN L,(C)
                case 0x70: byte temp = In(_gpr.C); _afr.F = (_afr.F & C) | (FlagsSZP[temp] & ~C); break; // IN F,(C)
                case 0x78: _afr.A = In(_gpr.C); _afr.F = (_afr.F & C) | (FlagsSZP[_afr.A] & ~C); _memPtr.word = (ushort)(_gpr.BC + 1); break; // IN A,(C)

                case 0xA2: InAndIncrement(); break; // INI
                case 0xB2: InIncrementAndRepeat(); break; // INIR
                case 0xAA: InAndDecrement(); break; // IND
                case 0xBA: InDecrementAndRepeat(); break; // INDR

                case 0x41: Out(_gpr.C, _gpr.B); break; // OUT (C),B
                case 0x49: Out(_gpr.C, _gpr.C); break; // OUT (C),C
                case 0x51: Out(_gpr.C, _gpr.D); break; // OUT (C),D
                case 0x59: Out(_gpr.C, _gpr.E); break; // OUT (C),E
                case 0x61: Out(_gpr.C, _gpr.H); break; // OUT (C),H
                case 0x69: Out(_gpr.C, _gpr.L); break; // OUT (C),L
                case 0x71: Out(_gpr.C, 0); break; // OUT (C),0
                case 0x79: Out(_gpr.C, _afr.A); _memPtr.word = (ushort)(_gpr.BC + 1); break; // OUT (C),A

                case 0xA3: OutAndIncrement(); break;// OUTI
                case 0xB3: OutIncrementAndRepeat(); break; // OTIR
                case 0xAB: OutAndDecrement(); break;// OUTD
                case 0xBB: OutDecrementAndRepeat(); break; // OTDR

                #endregion

                default:
                    throw new NotImplementedException($"Unimplemented opcode: 0xED {opcode:X2} at address 0x{_pc - 2:X4}");
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0066:Convert switch statement to expression", Justification = "GetRegisterForPrefix uses ref returns which cannot be used in a switch expression")]
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
                _memPtr.word = (ushort)(address + (sbyte)displacement);

                return _memPtr.word;
            }

            ref PairedRegister reg = ref GetRegisterForPrefix();

            switch (opcode)
            {
                #region 8-bit load group

                case 0x46: _gpr.B = ReadByte(Displace(reg.word, ReadByte(_pc++))); break; // LD B,(IX/IY + d)
                case 0x4E: _gpr.C = ReadByte(Displace(reg.word, ReadByte(_pc++))); break; // LD C,(IX/IY + d)
                case 0x56: _gpr.D = ReadByte(Displace(reg.word, ReadByte(_pc++))); break; // LD D,(IX/IY + d)
                case 0x5E: _gpr.E = ReadByte(Displace(reg.word, ReadByte(_pc++))); break; // LD E,(IX/IY + d)
                case 0x66: _gpr.H = ReadByte(Displace(reg.word, ReadByte(_pc++))); break; // LD H,(IX/IY + d)
                case 0x6E: _gpr.L = ReadByte(Displace(reg.word, ReadByte(_pc++))); break; // LD L,(IX/IY + d)
                case 0x7E: _afr.A = ReadByte(Displace(reg.word, ReadByte(_pc++))); break; // LD A,(IX/IY + d)

                case 0x26: reg.hi = ReadByte(_pc++); break; // LD IX/IY H,n
                case 0x2E: reg.lo = ReadByte(_pc++); break; // LD IX/IY L,n

                case 0x44: _gpr.B = reg.hi; break; // LD B,IXH/IYH
                case 0x45: _gpr.B = reg.lo; break; // LD B,IXL/IYL

                case 0x4C: _gpr.C = reg.hi; break; // LD C,IXH/IYH
                case 0x4D: _gpr.C = reg.lo; break; // LD C,IXL/IYL

                case 0x54: _gpr.D = reg.hi; break; // LD D,IXH/IYH
                case 0x55: _gpr.D = reg.lo; break; // LD D,IXL/IYL

                case 0x5C: _gpr.E = reg.hi; break; // LD E,IXH/IYH
                case 0x5D: _gpr.E = reg.lo; break; // LD E,IXL/IYL

                case 0x7C: _afr.A = reg.hi; break; // LD A,IXH/IYH
                case 0x7D: _afr.A = reg.lo; break; // LD A,IXL/IYL

                case 0x60: reg.hi = _gpr.B; break; // LD IXH/IYH,B
                case 0x61: reg.hi = _gpr.C; break; // LD IXH/IYH,C
                case 0x62: reg.hi = _gpr.D; break; // LD IXH/IYH,D
                case 0x63: reg.hi = _gpr.E; break; // LD IXH/IYH,E
                case 0x64: break;                  // LD IXH/IYH,IXH/IYH
                case 0x65: reg.hi = reg.lo; break; // LD IXH/IYH,IXL/IYL
                case 0x67: reg.hi = _afr.A; break; // LD IXH/IYH,A

                case 0x68: reg.lo = _gpr.B; break; // LD IXL/IYL,B
                case 0x69: reg.lo = _gpr.C; break; // LD IXL/IYL,C
                case 0x6A: reg.lo = _gpr.D; break; // LD IXL/IYL,D
                case 0x6B: reg.lo = _gpr.E; break; // LD IXL/IYL,E
                case 0x6C: reg.lo = reg.hi; break; // LD IXL/IYL,IXH/IYH
                case 0x6D: break;                  // LD IXL/IYL,IXL/IYL
                case 0x6F: reg.lo = _afr.A; break; // LD IXL/IYL,A

                case 0x70: WriteByte(Displace(reg.word, ReadByte(_pc++)), _gpr.B); break; // LD (IX/IY + d),B
                case 0x71: WriteByte(Displace(reg.word, ReadByte(_pc++)), _gpr.C); break; // LD (IX/IY + d),C
                case 0x72: WriteByte(Displace(reg.word, ReadByte(_pc++)), _gpr.D); break; // LD (IX/IY + d),D
                case 0x73: WriteByte(Displace(reg.word, ReadByte(_pc++)), _gpr.E); break; // LD (IX/IY + d),E
                case 0x74: WriteByte(Displace(reg.word, ReadByte(_pc++)), _gpr.H); break; // LD (IX/IY + d),H
                case 0x75: WriteByte(Displace(reg.word, ReadByte(_pc++)), _gpr.L); break; // LD (IX/IY + d),L
                case 0x77: WriteByte(Displace(reg.word, ReadByte(_pc++)), _afr.A); break; // LD (IX/IY + d),A

                case 0x36: WriteByte(Displace(reg.word, ReadByte(_pc++)), _interconnect.ReadByte(_pc++)); break; // LD (IX/IY + d),n

                #endregion

                #region 16-bit load group

                case 0x21: reg.word = ReadWord(_pc); _pc += 2; break; // LD IX/IY,nn

                case 0x2A: reg.word = ReadWord(ReadWord(_pc)); _pc += 2; break; // LD IX/IY,(nn)

                case 0x22: WriteWord(ReadWord(_pc), reg.word); _pc += 2; break; // LD (nn),IX/IY

                case 0xE5: PushWord(reg.word); break; // PUSH IX/IY

                case 0xE1: reg.word = PopWord(); break; // POP IX/IY

                #endregion

                #region exchange, block transfer, and search group

                case 0xE3:
                {
                    ushort temp = ReadWord(_sp);
                    _clock.AddCycles(1);
                    WriteWord(_sp, reg.word);
                    _clock.AddCycles(2);
                    reg.word = temp;
                    _memPtr.word = temp;
                }
                break; // EX (SP),IX/IY

                #endregion

                #region 8-bit arithmetic group

                case 0x84: (_afr.A, _afr.F) = Add8Bit(_afr.A, reg.hi); break;                                   // ADD IXH/IYH
                case 0x85: (_afr.A, _afr.F) = Add8Bit(_afr.A, reg.lo); break;                                   // ADD IXL/IYL
                case 0x86: (_afr.A, _afr.F) = Add8Bit(_afr.A, ReadByte(Displace(reg.word, ReadByte(_pc++)))); break; // ADD (IX/IY + d)

                case 0x8C: (_afr.A, _afr.F) = Add8Bit(_afr.A, reg.hi, _afr.F[C]); break;                                    // ADC IXH/IYH
                case 0x8D: (_afr.A, _afr.F) = Add8Bit(_afr.A, reg.lo, _afr.F[C]); break;                                    // ADC IXL/IYL
                case 0x8E: (_afr.A, _afr.F) = Add8Bit(_afr.A, ReadByte(Displace(reg.word, ReadByte(_pc++))), _afr.F[C]); break;  // ADC (IX/IY + d)

                case 0x94: (_afr.A, _afr.F) = Sub8Bit(_afr.A, reg.hi); break;                                   // SUB IXH/IYH
                case 0x95: (_afr.A, _afr.F) = Sub8Bit(_afr.A, reg.lo); break;                                   // SUB IXL/IYL
                case 0x96: (_afr.A, _afr.F) = Sub8Bit(_afr.A, ReadByte(Displace(reg.word, ReadByte(_pc++)))); break; // SUB (IX/IY + d)

                case 0x9C: (_afr.A, _afr.F) = Sub8Bit(_afr.A, reg.hi, _afr.F[C]); break;                                    // SBC IXH/IYH
                case 0x9D: (_afr.A, _afr.F) = Sub8Bit(_afr.A, reg.lo, _afr.F[C]); break;                                    // SBC IXL/IYL
                case 0x9E: (_afr.A, _afr.F) = Sub8Bit(_afr.A, ReadByte(Displace(reg.word, ReadByte(_pc++))), _afr.F[C]); break;  // SBC (IX/IY + d)

                case 0xA4: (_afr.A, _afr.F) = And8Bit(_afr.A, reg.hi); break;                                   // AND IXH/IYH
                case 0xA5: (_afr.A, _afr.F) = And8Bit(_afr.A, reg.lo); break;                                   // AND IXL/IYL
                case 0xA6: (_afr.A, _afr.F) = And8Bit(_afr.A, ReadByte(Displace(reg.word, ReadByte(_pc++)))); break; // AND (IX/IY + d)

                case 0xB4: (_afr.A, _afr.F) = Or8Bit(_afr.A, reg.hi); break;                                    // OR IXH/IYH
                case 0xB5: (_afr.A, _afr.F) = Or8Bit(_afr.A, reg.lo); break;                                    // OR IXL/IYL
                case 0xB6: (_afr.A, _afr.F) = Or8Bit(_afr.A, ReadByte(Displace(reg.word, ReadByte(_pc++)))); break;  // OR (IX/IY + d)

                case 0xAC: (_afr.A, _afr.F) = Xor8Bit(_afr.A, reg.hi); break;                                   // XOR IXH/IYH
                case 0xAD: (_afr.A, _afr.F) = Xor8Bit(_afr.A, reg.lo); break;                                   // XOR IXL/IYL
                case 0xAE: (_afr.A, _afr.F) = Xor8Bit(_afr.A, ReadByte(Displace(reg.word, ReadByte(_pc++)))); break; // XOR (IX/IY + d)

                case 0xBC: _afr.F = Compare8Bit(_afr.A, reg.hi); break;                                     // CP IXH/IYH
                case 0xBD: _afr.F = Compare8Bit(_afr.A, reg.lo); break;                                     // CP IXL/IYL
                case 0xBE: _afr.F = Compare8Bit(_afr.A, ReadByte(Displace(reg.word, ReadByte(_pc++)))); break;   // CP (IX/IY + d)

                case 0x24: reg.hi = Inc8Bit(reg.hi); break;                                                                             // INC IXH/IYH
                case 0x2C: reg.lo = Inc8Bit(reg.lo); break;                                                                             // INC IXL/IYL
                case 0x34: { ushort address = Displace(reg.word, ReadByte(_pc++)); WriteByte(address, Inc8Bit(ReadByte(address))); } break;  // INC (IX/IY + d)

                case 0x25: reg.hi = Dec8Bit(reg.hi); break;                                                                             // DEC IXH/IYH
                case 0x2D: reg.lo = Dec8Bit(reg.lo); break;                                                                             // DEC IXL/IYL
                case 0x35: { ushort address = Displace(reg.word, ReadByte(_pc++)); WriteByte(address, Dec8Bit(ReadByte(address))); } break;  // DEC (IX/IY + d)

                #endregion

                #region general-purpose arithmetic and cpu control group

                case 0xCB: ExecuteDisplacedCBPrefixedOpcode(Displace(reg.word, ReadByte(_pc++)), _interconnect.ReadByte(_pc++)); break;

                #endregion

                #region 16-bit arithmetic group

                case 0x09: reg.word = Add16Bit(reg.word, _gpr.BC); break;   // ADD IX/IY,BC
                case 0x19: reg.word = Add16Bit(reg.word, _gpr.DE); break;   // ADD IX/IY,DE
                case 0x29: reg.word = Add16Bit(reg.word, reg.word); break;  // ADD IX/IY,IX/IY
                case 0x39: reg.word = Add16Bit(reg.word, _sp); break;       // ADD IX/IY,SP

                case 0x23: reg.word++; _clock.AddCycles(2); break; // INC IX/IY
                case 0x2B: reg.word--; _clock.AddCycles(2); break; // DEC IX/IY

                #endregion

                default:
                    // any other instructions not handled above behave as if they were not prefixed so pass through
                    ExecuteOpcode(opcode);
                    break;
            }
        }

        #region exchange, block transfer, and search group handlers

        private void LoadAndIncrement()
        {
            byte data = ReadByte(_gpr.HL++);

            WriteByte(_gpr.DE++, data);

            _clock.AddCycles(2);

            int temp = data + _afr.A;

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
            _memPtr.word = (ushort)(_pc + 1);
        }

        private void LoadAndDecrement()
        {
            byte data = ReadByte(_gpr.HL--);

            WriteByte(_gpr.DE--, data);

            _clock.AddCycles(2);

            int temp = data + _afr.A;

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
            _memPtr.word = (ushort)(_pc + 1);
        }

        private void CompareAndIncrement()
        {
            (byte result, StatusFlags flags) = Sub8Bit(_afr.A, ReadByte(_gpr.HL++));

            _clock.AddCycles(5);

            int temp = result - (flags[H] ? 1 : 0);

            flags[B3] = temp.Bit(3);
            flags[B5] = temp.Bit(1);

            flags[C] = _afr.F[C];
            flags[V] = --_gpr.BC != 0;

            _afr.F = flags;

            _memPtr.word++;
        }

        private void CompareIncrementAndRepeat()
        {
            CompareAndIncrement();

            if (_gpr.BC == 0 || _afr.F[Z])
            {
                _memPtr.word++;
                return;
            }

            _clock.AddCycles(5);

            _pc -= 2;
            _memPtr.word = (ushort)(_pc + 1);
        }

        private void CompareAndDecrement()
        {
            (byte result, StatusFlags flags) = Sub8Bit(_afr.A, ReadByte(_gpr.HL--));

            _clock.AddCycles(5);

            int temp = result - (flags[H] ? 1 : 0);

            flags[B3] = temp.Bit(3);
            flags[B5] = temp.Bit(1);

            flags[C] = _afr.F[C];
            flags[V] = --_gpr.BC != 0;

            _afr.F = flags;

            _memPtr.word--;
        }

        private void CompareDecrementAndRepeat()
        {
            CompareAndDecrement();

            if (_gpr.BC == 0 || _afr.F[Z])
            {
                _memPtr.word++;
                return;
            }

            _clock.AddCycles(5);

            _pc -= 2;
            _memPtr.word = (ushort)(_pc + 1);
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

            int flags = FlagsSZP[result] & (~P);

            flags |= carryIn & 0x10;
            flags |= (((carryIn >> 7) & 0x01) != (carryIn >> 8)) ? V : 0;
            flags |= (carryIn & 0x100) >> 8;

            return ((byte)result, new StatusFlags(flags));
        }

        // https://stackoverflow.com/questions/8034566/overflow-and-carry-flags-on-z80/8037485#8037485
        private static (byte, StatusFlags) Sub8Bit(byte a, byte b, bool carry = false)
        {
            // a - b - c = a + ~b + 1 - c = a + ~b + !c
            (byte value, StatusFlags flags) result = Add8Bit(a, (byte)~b, !carry);

            result.flags ^= C | H;
            result.flags |= N;

            return result;
        }

        private static (byte, StatusFlags) Or8Bit(byte a, byte b)
        {
            int result = a | b;

            StatusFlags flags = FlagsSZP[result];

            return ((byte)result, flags);
        }

        private static (byte, StatusFlags) Xor8Bit(byte a, byte b)
        {
            int result = a ^ b;

            StatusFlags flags = FlagsSZP[result];

            return ((byte)result, flags);
        }

        private static StatusFlags Compare8Bit(byte a, byte b)
        {
            (byte _, StatusFlags flags) = Sub8Bit(a, b);

            // flag bits 3 and 5 populated from the operand and not result
            // http://www.z80.info/z80sflag.htm
            flags[B5] = b.Bit(5);
            flags[B3] = b.Bit(3);

            return flags;
        }

        private byte Inc8Bit(byte a)
        {
            (byte result, StatusFlags flags) = Add8Bit(a, 1);

            _afr.F &= C;
            _afr.F |= flags & ~C;

            return result;
        }

        private byte Dec8Bit(byte a)
        {
            (byte result, StatusFlags flags) = Sub8Bit(a, 1);

            _afr.F &= C;
            _afr.F |= flags & ~C;

            return result;
        }

        #endregion

        #region general-purpose arithmetic and cpu control group handlers

        private void DisableInterrupts()
        {
            _iff1 = _iff2 = false;
        }

        private void SetInterruptMode(int interruptMode)
        {
            _interruptMode = interruptMode;
        }

        private void DecimalAdjustAccumulator()
        {
            byte correctionFactor = 0;

            if (_afr.A > 0x99 || _afr.F[C])
            {
                correctionFactor |= 0x60;
                _afr.F[C] = true;
            }

            if ((_afr.A & 0x0F) > 0x09 || _afr.F[H])
                correctionFactor |= 0x06;

            bool subtraction = _afr.F[N];

            _afr.F[H] = (!subtraction && (_afr.A & 0x0F) > 0x09) || (subtraction && _afr.F[H] && (_afr.A & 0x0F) < 0x06);

            _afr.A += (byte)(subtraction ? -correctionFactor : correctionFactor);

            _afr.F = FlagsSZP[_afr.A] | (_afr.F & (H | N | C));
        }

        #endregion

        #region 16-bit arithmetic group

        private (ushort, StatusFlags) AddWithCarry16Bit(ushort a, ushort b, bool carry = false)
        {
            byte hi, lo;
            StatusFlags flags;

            _clock.AddCycles(4);
            (lo, flags) = Add8Bit((byte)(a >> 0), (byte)(b >> 0), carry);

            _clock.AddCycles(3);
            (hi, flags) = Add8Bit((byte)(a >> 8), (byte)(b >> 8), flags[C]);

            ushort result = (ushort)(hi << 8 | lo);

            flags[Z] = result == 0;

            _memPtr.word = (ushort)(a + 1);

            return ((ushort)(hi << 8 | lo), flags);
        }

        private ushort Add16Bit(ushort a, ushort b)
        {
            // reset affected flags
            _afr.F[B5 | H | B3 | N | C] = false;

            (ushort result, StatusFlags flags) = AddWithCarry16Bit(a, b);

            _afr.F |= flags & (B5 | H | B3 | N | C);

            return result;
        }

        private (ushort, StatusFlags) SubWithCarry16Bit(ushort a, ushort b, bool carry = false)
        {
            byte hi, lo;
            StatusFlags flags;

            _clock.AddCycles(4);
            (lo, flags) = Sub8Bit((byte)(a >> 0), (byte)(b >> 0), carry);

            _clock.AddCycles(3);
            (hi, flags) = Sub8Bit((byte)(a >> 8), (byte)(b >> 8), flags[C]);

            ushort result = (ushort)(hi << 8 | lo);

            flags[Z] = result == 0;

            _memPtr.word = (ushort)(a + 1);

            return ((ushort)(hi << 8 | lo), flags);
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

        private void RotateLeftThroughCarryAccumulator()
        {
            bool b7 = _afr.A.Bit(7);

            _afr.A = (byte)((_afr.A << 1) | (_afr.F[C] ? 1 : 0));

            _afr.F[H | N] = false;
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

        private void RotateRightThroughCarryAccumulator()
        {
            bool b0 = _afr.A.Bit(0);

            _afr.A = (byte)((_afr.A >> 1) | (_afr.F[C] ? 0x80 : 0));

            _afr.F[H | N] = false;
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

        private void RotateLeftDigit()
        {
            byte a = _afr.A;
            byte data = ReadByte(_gpr.HL);

            _afr.A = (byte)((a & 0xF0) | (data >> 4));
            _clock.AddCycles(4);

            WriteByte(_gpr.HL, (byte)((data << 4) | (a & 0x0F)));

            _afr.F = FlagsSZP[_afr.A] | (_afr.F & C);
            _memPtr.word = (ushort)(_gpr.HL + 1);
        }

        private void RotateRightDigit()
        {
            byte a = _afr.A;
            byte data = ReadByte(_gpr.HL);

            _afr.A = (byte)((a & 0xF0) | (data & 0x0F));
            _clock.AddCycles(4);

            WriteByte(_gpr.HL, (byte)((data >> 4) | (a << 4)));

            _afr.F = FlagsSZP[_afr.A] | (_afr.F & C);
            _memPtr.word = (ushort)(_gpr.HL + 1);
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
            _memPtr.word = address;
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

            _memPtr.word = address;
        }

        private void JumpRelative()
        {
            sbyte offset = (sbyte)ReadByte(_pc++);
            _clock.AddCycles(5);
            _pc += (ushort)offset;
            _memPtr.word = _pc;
        }

        private void JumpRelative(bool condition)
        {
            sbyte offset = (sbyte)ReadByte(_pc++);

            if (condition)
            {
                _clock.AddCycles(5);
                _pc += (ushort)offset;
                _memPtr.word = _pc;
            }
        }

        private void DecrementAndJumpNotZero()
        {
            _clock.AddCycles(1);

            JumpRelative(--_gpr.B != 0);
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
            _memPtr.word = address;
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

            _memPtr.word = address;
        }

        private void Return()
        {
            _pc = PopWord();
            _memPtr.word = _pc;
        }

        private void Return(bool condition)
        {
            _clock.AddCycles(1);
            if (condition)
            {
                _pc = PopWord();
                _memPtr.word = _pc;
            }
        }

        private void Reset(ushort address)
        {
            _clock.AddCycles(1);
            PushWord(_pc);
            _pc = address;
            _memPtr.word = _pc;
        }

        #endregion

        #region input and output group handlers

        private byte In(byte address)
        {
            byte v = _interconnect.In(address);

            _clock.AddCycles(4);

            return v;
        }

        private void InAndIncrement()
        {
            _clock.AddCycles(1);

            byte data = In(_gpr.C);

            WriteByte(_gpr.HL++, data);

            _memPtr.word = (ushort)(_gpr.BC + 1);

            (byte result, StatusFlags flags) = Sub8Bit(_gpr.B, 1);
            _gpr.B = result;

            // wtf? i don't even... http://www.z80.info/zip/z80-documented.pdf section 4.3
            flags[N] = data.Bit(7);

            int temp = data + ((_gpr.C + 1) & 0xFF);
            flags[H | C] = temp > 255;
            flags[P] = ((temp & 7) ^ _gpr.B).EvenParity();

            _afr.F = flags;
        }

        private void InIncrementAndRepeat()
        {
            InAndIncrement();

            if (_gpr.B == 0)
                return;

            _clock.AddCycles(5);

            _pc -= 2;
        }

        private void InAndDecrement()
        {
            _clock.AddCycles(1);

            byte data = In(_gpr.C);

            WriteByte(_gpr.HL--, data);

            _memPtr.word = (ushort)(_gpr.BC - 1);

            (byte result, StatusFlags flags) = Sub8Bit(_gpr.B, 1);
            _gpr.B = result;

            // wtf? i don't even... http://www.z80.info/zip/z80-documented.pdf section 4.3
            flags[N] = data.Bit(7);

            int temp = data + ((_gpr.C - 1) & 0xFF);
            flags[H | C] = temp > 255;
            flags[P] = ((temp & 7) ^ _gpr.B).EvenParity();

            _afr.F = flags;
        }

        private void InDecrementAndRepeat()
        {
            InAndDecrement();

            if (_gpr.B == 0)
                return;

            _clock.AddCycles(5);

            _pc -= 2;
        }

        private void Out(byte address, byte value)
        {
            _interconnect.Out(address, value);

            _clock.AddCycles(4);
        }

        private void OutAndIncrement()
        {
            _clock.AddCycles(1);

            byte data = ReadByte(_gpr.HL++);

            Out(_gpr.C, data);

            (byte result, StatusFlags flags) = Sub8Bit(_gpr.B, 1);
            _gpr.B = result;

            _memPtr.word = (ushort)(_gpr.BC + 1);

            // wtf? i don't even... http://www.z80.info/zip/z80-documented.pdf section 4.3
            flags[N] = data.Bit(7);

            int temp = data + _gpr.L;
            flags[H | C] = temp > 255;
            flags[P] = ((temp & 7) ^ _gpr.B).EvenParity();

            _afr.F = flags;
        }

        private void OutIncrementAndRepeat()
        {
            OutAndIncrement();

            if (_gpr.B == 0)
                return;

            _clock.AddCycles(5);

            _pc -= 2;
        }

        private void OutAndDecrement()
        {
            _clock.AddCycles(1);

            byte data = ReadByte(_gpr.HL--);

            Out(_gpr.C, data);

            (byte result, StatusFlags flags) = Sub8Bit(_gpr.B, 1);
            _gpr.B = result;

            _memPtr.word = (ushort)(_gpr.BC - 1);

            // wtf? i don't even... http://www.z80.info/zip/z80-documented.pdf section 4.3
            flags[N] = data.Bit(7);

            int temp = data + _gpr.L;
            flags[H | C] = temp > 255;
            flags[P] = ((temp & 7) ^ _gpr.B).EvenParity();

            _afr.F = flags;
        }

        private void OutDecrementAndRepeat()
        {
            OutAndDecrement();

            if (_gpr.B == 0)
                return;

            _clock.AddCycles(5);

            _pc -= 2;
        }

        #endregion
    }
}
