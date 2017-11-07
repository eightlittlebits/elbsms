using System;

namespace elbsms_core.CPU
{
    using static StatusFlags;

    partial class Z80
    {
        private void ExecuteCBPrefixedOpcode(byte opcode)
        {
            switch (opcode)
            {
                #region rotate and shift group

                case 0x00: _gpr.B = RotateLeft(_gpr.B); break; // RLC B
                case 0x01: _gpr.C = RotateLeft(_gpr.C); break; // RLC C
                case 0x02: _gpr.D = RotateLeft(_gpr.D); break; // RLC D
                case 0x03: _gpr.E = RotateLeft(_gpr.E); break; // RLC E
                case 0x04: _gpr.H = RotateLeft(_gpr.H); break; // RLC H
                case 0x05: _gpr.L = RotateLeft(_gpr.L); break; // RLC L
                case 0x06: WriteByte(_gpr.HL, RotateLeft(ReadByte(_gpr.HL))); break; // RLC (HL)
                case 0x07: _afr.A = RotateLeft(_afr.A); break; // RLC A

                case 0x08: _gpr.B = RotateRight(_gpr.B); break; // RRC B
                case 0x09: _gpr.C = RotateRight(_gpr.C); break; // RRC C
                case 0x0A: _gpr.D = RotateRight(_gpr.D); break; // RRC D
                case 0x0B: _gpr.E = RotateRight(_gpr.E); break; // RRC E
                case 0x0C: _gpr.H = RotateRight(_gpr.H); break; // RRC H
                case 0x0D: _gpr.L = RotateRight(_gpr.L); break; // RRC L
                case 0x0E: WriteByte(_gpr.HL, RotateRight(ReadByte(_gpr.HL))); break; // RRC (HL)
                case 0x0F: _afr.A = RotateRight(_afr.A); break; // RRC A

                case 0x10: _gpr.B = RotateLeftThroughCarry(_gpr.B); break; // RL B
                case 0x11: _gpr.C = RotateLeftThroughCarry(_gpr.C); break; // RL C
                case 0x12: _gpr.D = RotateLeftThroughCarry(_gpr.D); break; // RL D
                case 0x13: _gpr.E = RotateLeftThroughCarry(_gpr.E); break; // RL E
                case 0x14: _gpr.H = RotateLeftThroughCarry(_gpr.H); break; // RL H
                case 0x15: _gpr.L = RotateLeftThroughCarry(_gpr.L); break; // RL L
                case 0x16: WriteByte(_gpr.HL, RotateLeftThroughCarry(ReadByte(_gpr.HL))); break; // RL (HL)
                case 0x17: _afr.A = RotateLeftThroughCarry(_afr.A); break; // RL A

                case 0x18: _gpr.B = RotateRightThroughCarry(_gpr.B); break; // RR B
                case 0x19: _gpr.C = RotateRightThroughCarry(_gpr.C); break; // RR C
                case 0x1A: _gpr.D = RotateRightThroughCarry(_gpr.D); break; // RR D
                case 0x1B: _gpr.E = RotateRightThroughCarry(_gpr.E); break; // RR E
                case 0x1C: _gpr.H = RotateRightThroughCarry(_gpr.H); break; // RR H
                case 0x1D: _gpr.L = RotateRightThroughCarry(_gpr.L); break; // RR L
                case 0x1E: WriteByte(_gpr.HL, RotateRightThroughCarry(ReadByte(_gpr.HL))); break; // RR (HL)
                case 0x1F: _afr.A = RotateRightThroughCarry(_afr.A); break; // RR A

                case 0x20: _gpr.B = ShiftLeftArithmetic(_gpr.B); break; // SLA B
                case 0x21: _gpr.C = ShiftLeftArithmetic(_gpr.C); break; // SLA C
                case 0x22: _gpr.D = ShiftLeftArithmetic(_gpr.D); break; // SLA D
                case 0x23: _gpr.E = ShiftLeftArithmetic(_gpr.E); break; // SLA E
                case 0x24: _gpr.H = ShiftLeftArithmetic(_gpr.H); break; // SLA H
                case 0x25: _gpr.L = ShiftLeftArithmetic(_gpr.L); break; // SLA L
                case 0x26: WriteByte(_gpr.HL, ShiftLeftArithmetic(ReadByte(_gpr.HL))); break; // SLA (HL)
                case 0x27: _afr.A = ShiftLeftArithmetic(_afr.A); break; // SLA A

                case 0x28: _gpr.B = ShiftRightArithmetic(_gpr.B); break; // SRA B
                case 0x29: _gpr.C = ShiftRightArithmetic(_gpr.C); break; // SRA C
                case 0x2A: _gpr.D = ShiftRightArithmetic(_gpr.D); break; // SRA D
                case 0x2B: _gpr.E = ShiftRightArithmetic(_gpr.E); break; // SRA E
                case 0x2C: _gpr.H = ShiftRightArithmetic(_gpr.H); break; // SRA H
                case 0x2D: _gpr.L = ShiftRightArithmetic(_gpr.L); break; // SRA L
                case 0x2E: WriteByte(_gpr.HL, ShiftRightArithmetic(ReadByte(_gpr.HL))); break; // SRA (HL)
                case 0x2F: _afr.A = ShiftRightArithmetic(_afr.A); break; // SRA A

                case 0x30: _gpr.B = ShiftLeftInsertingOne(_gpr.B); break; // SLL B
                case 0x31: _gpr.C = ShiftLeftInsertingOne(_gpr.C); break; // SLL C
                case 0x32: _gpr.D = ShiftLeftInsertingOne(_gpr.D); break; // SLL D
                case 0x33: _gpr.E = ShiftLeftInsertingOne(_gpr.E); break; // SLL E
                case 0x34: _gpr.H = ShiftLeftInsertingOne(_gpr.H); break; // SLL H
                case 0x35: _gpr.L = ShiftLeftInsertingOne(_gpr.L); break; // SLL L
                case 0x36: WriteByte(_gpr.HL, ShiftLeftInsertingOne(ReadByte(_gpr.HL))); break; // SLL (HL)
                case 0x37: _afr.A = ShiftLeftInsertingOne(_afr.A); break; // SLL A

                case 0x38: _gpr.B = ShiftRightLogical(_gpr.B); break; // SRL B
                case 0x39: _gpr.C = ShiftRightLogical(_gpr.C); break; // SRL C
                case 0x3A: _gpr.D = ShiftRightLogical(_gpr.D); break; // SRL D
                case 0x3B: _gpr.E = ShiftRightLogical(_gpr.E); break; // SRL E
                case 0x3C: _gpr.H = ShiftRightLogical(_gpr.H); break; // SRL H
                case 0x3D: _gpr.L = ShiftRightLogical(_gpr.L); break; // SRL L
                case 0x3E: WriteByte(_gpr.HL, ShiftRightLogical(ReadByte(_gpr.HL))); break; // SRL (HL)
                case 0x3F: _afr.A = ShiftRightLogical(_afr.A); break; // SRL A

                #endregion

                #region bit set, reset, and test group

                case 0x40: _afr.F = TestBit(_gpr.B, 0) | (_afr.F & C); break; // BIT 0,B
                case 0x41: _afr.F = TestBit(_gpr.C, 0) | (_afr.F & C); break; // BIT 0,C
                case 0x42: _afr.F = TestBit(_gpr.D, 0) | (_afr.F & C); break; // BIT 0,D
                case 0x43: _afr.F = TestBit(_gpr.E, 0) | (_afr.F & C); break; // BIT 0,E
                case 0x44: _afr.F = TestBit(_gpr.H, 0) | (_afr.F & C); break; // BIT 0,H
                case 0x45: _afr.F = TestBit(_gpr.L, 0) | (_afr.F & C); break; // BIT 0,L
                case 0x46: _afr.F = TestBitMemory(_gpr.HL, 0); break; // BIT 0,(HL)
                case 0x47: _afr.F = TestBit(_afr.A, 0) | (_afr.F & C); break; // BIT 0,A

                case 0x48: _afr.F = TestBit(_gpr.B, 1) | (_afr.F & C); break; // BIT 1,B
                case 0x49: _afr.F = TestBit(_gpr.C, 1) | (_afr.F & C); break; // BIT 1,C
                case 0x4A: _afr.F = TestBit(_gpr.D, 1) | (_afr.F & C); break; // BIT 1,D
                case 0x4B: _afr.F = TestBit(_gpr.E, 1) | (_afr.F & C); break; // BIT 1,E
                case 0x4C: _afr.F = TestBit(_gpr.H, 1) | (_afr.F & C); break; // BIT 1,H
                case 0x4D: _afr.F = TestBit(_gpr.L, 1) | (_afr.F & C); break; // BIT 1,L
                case 0x4E: _afr.F = TestBitMemory(_gpr.HL, 1); break; // BIT 1,(HL)
                case 0x4F: _afr.F = TestBit(_afr.A, 1) | (_afr.F & C); break; // BIT 1,A

                case 0x50: _afr.F = TestBit(_gpr.B, 2) | (_afr.F & C); break; // BIT 2,B
                case 0x51: _afr.F = TestBit(_gpr.C, 2) | (_afr.F & C); break; // BIT 2,C
                case 0x52: _afr.F = TestBit(_gpr.D, 2) | (_afr.F & C); break; // BIT 2,D
                case 0x53: _afr.F = TestBit(_gpr.E, 2) | (_afr.F & C); break; // BIT 2,E
                case 0x54: _afr.F = TestBit(_gpr.H, 2) | (_afr.F & C); break; // BIT 2,H
                case 0x55: _afr.F = TestBit(_gpr.L, 2) | (_afr.F & C); break; // BIT 2,L
                case 0x56: _afr.F = TestBitMemory(_gpr.HL, 2); break; // BIT 2,(HL)
                case 0x57: _afr.F = TestBit(_afr.A, 2) | (_afr.F & C); break; // BIT 2,A

                case 0x58: _afr.F = TestBit(_gpr.B, 3) | (_afr.F & C); break; // BIT 3,B
                case 0x59: _afr.F = TestBit(_gpr.C, 3) | (_afr.F & C); break; // BIT 3,C
                case 0x5A: _afr.F = TestBit(_gpr.D, 3) | (_afr.F & C); break; // BIT 3,D
                case 0x5B: _afr.F = TestBit(_gpr.E, 3) | (_afr.F & C); break; // BIT 3,E
                case 0x5C: _afr.F = TestBit(_gpr.H, 3) | (_afr.F & C); break; // BIT 3,H
                case 0x5D: _afr.F = TestBit(_gpr.L, 3) | (_afr.F & C); break; // BIT 3,L
                case 0x5E: _afr.F = TestBitMemory(_gpr.HL, 3); break; // BIT 3,(HL)
                case 0x5F: _afr.F = TestBit(_afr.A, 3) | (_afr.F & C); break; // BIT 3,A

                case 0x60: _afr.F = TestBit(_gpr.B, 4) | (_afr.F & C); break; // BIT 4,B
                case 0x61: _afr.F = TestBit(_gpr.C, 4) | (_afr.F & C); break; // BIT 4,C
                case 0x62: _afr.F = TestBit(_gpr.D, 4) | (_afr.F & C); break; // BIT 4,D
                case 0x63: _afr.F = TestBit(_gpr.E, 4) | (_afr.F & C); break; // BIT 4,E
                case 0x64: _afr.F = TestBit(_gpr.H, 4) | (_afr.F & C); break; // BIT 4,H
                case 0x65: _afr.F = TestBit(_gpr.L, 4) | (_afr.F & C); break; // BIT 4,L
                case 0x66: _afr.F = TestBitMemory(_gpr.HL, 4); break; // BIT 4,(HL)
                case 0x67: _afr.F = TestBit(_afr.A, 4) | (_afr.F & C); break; // BIT 4,A

                case 0x68: _afr.F = TestBit(_gpr.B, 5) | (_afr.F & C); break; // BIT 5,B
                case 0x69: _afr.F = TestBit(_gpr.C, 5) | (_afr.F & C); break; // BIT 5,C
                case 0x6A: _afr.F = TestBit(_gpr.D, 5) | (_afr.F & C); break; // BIT 5,D
                case 0x6B: _afr.F = TestBit(_gpr.E, 5) | (_afr.F & C); break; // BIT 5,E
                case 0x6C: _afr.F = TestBit(_gpr.H, 5) | (_afr.F & C); break; // BIT 5,H
                case 0x6D: _afr.F = TestBit(_gpr.L, 5) | (_afr.F & C); break; // BIT 5,L
                case 0x6E: _afr.F = TestBitMemory(_gpr.HL, 5); break; // BIT 5,(HL)
                case 0x6F: _afr.F = TestBit(_afr.A, 5) | (_afr.F & C); break; // BIT 5,A

                case 0x70: _afr.F = TestBit(_gpr.B, 6) | (_afr.F & C); break; // BIT 6,B
                case 0x71: _afr.F = TestBit(_gpr.C, 6) | (_afr.F & C); break; // BIT 6,C
                case 0x72: _afr.F = TestBit(_gpr.D, 6) | (_afr.F & C); break; // BIT 6,D
                case 0x73: _afr.F = TestBit(_gpr.E, 6) | (_afr.F & C); break; // BIT 6,E
                case 0x74: _afr.F = TestBit(_gpr.H, 6) | (_afr.F & C); break; // BIT 6,H
                case 0x75: _afr.F = TestBit(_gpr.L, 6) | (_afr.F & C); break; // BIT 6,L
                case 0x76: _afr.F = TestBitMemory(_gpr.HL, 6); break; // BIT 6,(HL)
                case 0x77: _afr.F = TestBit(_afr.A, 6) | (_afr.F & C); break; // BIT 6,A

                case 0x78: _afr.F = TestBit(_gpr.B, 7) | (_afr.F & C); break; // BIT 7,B
                case 0x79: _afr.F = TestBit(_gpr.C, 7) | (_afr.F & C); break; // BIT 7,C
                case 0x7A: _afr.F = TestBit(_gpr.D, 7) | (_afr.F & C); break; // BIT 7,D
                case 0x7B: _afr.F = TestBit(_gpr.E, 7) | (_afr.F & C); break; // BIT 7,E
                case 0x7C: _afr.F = TestBit(_gpr.H, 7) | (_afr.F & C); break; // BIT 7,H
                case 0x7D: _afr.F = TestBit(_gpr.L, 7) | (_afr.F & C); break; // BIT 7,L
                case 0x7E: _afr.F = TestBitMemory(_gpr.HL, 7); break; // BIT 7,(HL)
                case 0x7F: _afr.F = TestBit(_afr.A, 7) | (_afr.F & C); break; // BIT 7,A

                case 0x80: _gpr.B = ResetBit(_gpr.B, 0); break; // RES 0,B
                case 0x81: _gpr.C = ResetBit(_gpr.C, 0); break; // RES 0,C
                case 0x82: _gpr.D = ResetBit(_gpr.D, 0); break; // RES 0,D
                case 0x83: _gpr.E = ResetBit(_gpr.E, 0); break; // RES 0,E
                case 0x84: _gpr.H = ResetBit(_gpr.H, 0); break; // RES 0,H
                case 0x85: _gpr.L = ResetBit(_gpr.L, 0); break; // RES 0,L
                case 0x86: WriteByte(_gpr.HL, ResetBit(ReadByte(_gpr.HL), 0)); break; // RES 0,(HL)
                case 0x87: _afr.A = ResetBit(_afr.A, 0); break; // RES 0,A

                case 0x88: _gpr.B = ResetBit(_gpr.B, 1); break; // RES 1,B
                case 0x89: _gpr.C = ResetBit(_gpr.C, 1); break; // RES 1,C
                case 0x8A: _gpr.D = ResetBit(_gpr.D, 1); break; // RES 1,D
                case 0x8B: _gpr.E = ResetBit(_gpr.E, 1); break; // RES 1,E
                case 0x8C: _gpr.H = ResetBit(_gpr.H, 1); break; // RES 1,H
                case 0x8D: _gpr.L = ResetBit(_gpr.L, 1); break; // RES 1,L
                case 0x8E: WriteByte(_gpr.HL, ResetBit(ReadByte(_gpr.HL), 1)); break; // RES 1,(HL)
                case 0x8F: _afr.A = ResetBit(_afr.A, 1); break; // RES 1,A

                case 0x90: _gpr.B = ResetBit(_gpr.B, 2); break; // RES 2,B
                case 0x91: _gpr.C = ResetBit(_gpr.C, 2); break; // RES 2,C
                case 0x92: _gpr.D = ResetBit(_gpr.D, 2); break; // RES 2,D
                case 0x93: _gpr.E = ResetBit(_gpr.E, 2); break; // RES 2,E
                case 0x94: _gpr.H = ResetBit(_gpr.H, 2); break; // RES 2,H
                case 0x95: _gpr.L = ResetBit(_gpr.L, 2); break; // RES 2,L
                case 0x96: WriteByte(_gpr.HL, ResetBit(ReadByte(_gpr.HL), 2)); break; // RES 2,(HL)
                case 0x97: _afr.A = ResetBit(_afr.A, 2); break; // RES 2,A

                case 0x98: _gpr.B = ResetBit(_gpr.B, 3); break; // RES 3,B
                case 0x99: _gpr.C = ResetBit(_gpr.C, 3); break; // RES 3,C
                case 0x9A: _gpr.D = ResetBit(_gpr.D, 3); break; // RES 3,D
                case 0x9B: _gpr.E = ResetBit(_gpr.E, 3); break; // RES 3,E
                case 0x9C: _gpr.H = ResetBit(_gpr.H, 3); break; // RES 3,H
                case 0x9D: _gpr.L = ResetBit(_gpr.L, 3); break; // RES 3,L
                case 0x9E: WriteByte(_gpr.HL, ResetBit(ReadByte(_gpr.HL), 3)); break; // RES 3,(HL)
                case 0x9F: _afr.A = ResetBit(_afr.A, 3); break; // RES 3,A

                case 0xA0: _gpr.B = ResetBit(_gpr.B, 4); break; // RES 4,B
                case 0xA1: _gpr.C = ResetBit(_gpr.C, 4); break; // RES 4,C
                case 0xA2: _gpr.D = ResetBit(_gpr.D, 4); break; // RES 4,D
                case 0xA3: _gpr.E = ResetBit(_gpr.E, 4); break; // RES 4,E
                case 0xA4: _gpr.H = ResetBit(_gpr.H, 4); break; // RES 4,H
                case 0xA5: _gpr.L = ResetBit(_gpr.L, 4); break; // RES 4,L
                case 0xA6: WriteByte(_gpr.HL, ResetBit(ReadByte(_gpr.HL), 4)); break; // RES 4,(HL)
                case 0xA7: _afr.A = ResetBit(_afr.A, 4); break; // RES 4,A

                case 0xA8: _gpr.B = ResetBit(_gpr.B, 5); break; // RES 5,B
                case 0xA9: _gpr.C = ResetBit(_gpr.C, 5); break; // RES 5,C
                case 0xAA: _gpr.D = ResetBit(_gpr.D, 5); break; // RES 5,D
                case 0xAB: _gpr.E = ResetBit(_gpr.E, 5); break; // RES 5,E
                case 0xAC: _gpr.H = ResetBit(_gpr.H, 5); break; // RES 5,H
                case 0xAD: _gpr.L = ResetBit(_gpr.L, 5); break; // RES 5,L
                case 0xAE: WriteByte(_gpr.HL, ResetBit(ReadByte(_gpr.HL), 5)); break; // RES 5,(HL)
                case 0xAF: _afr.A = ResetBit(_afr.A, 5); break; // RES 5,A

                case 0xB0: _gpr.B = ResetBit(_gpr.B, 6); break; // RES 6,B
                case 0xB1: _gpr.C = ResetBit(_gpr.C, 6); break; // RES 6,C
                case 0xB2: _gpr.D = ResetBit(_gpr.D, 6); break; // RES 6,D
                case 0xB3: _gpr.E = ResetBit(_gpr.E, 6); break; // RES 6,E
                case 0xB4: _gpr.H = ResetBit(_gpr.H, 6); break; // RES 6,H
                case 0xB5: _gpr.L = ResetBit(_gpr.L, 6); break; // RES 6,L
                case 0xB6: WriteByte(_gpr.HL, ResetBit(ReadByte(_gpr.HL), 6)); break; // RES 6,(HL)
                case 0xB7: _afr.A = ResetBit(_afr.A, 6); break; // RES 6,A

                case 0xB8: _gpr.B = ResetBit(_gpr.B, 7); break; // RES 7,B
                case 0xB9: _gpr.C = ResetBit(_gpr.C, 7); break; // RES 7,C
                case 0xBA: _gpr.D = ResetBit(_gpr.D, 7); break; // RES 7,D
                case 0xBB: _gpr.E = ResetBit(_gpr.E, 7); break; // RES 7,E
                case 0xBC: _gpr.H = ResetBit(_gpr.H, 7); break; // RES 7,H
                case 0xBD: _gpr.L = ResetBit(_gpr.L, 7); break; // RES 7,L
                case 0xBE: WriteByte(_gpr.HL, ResetBit(ReadByte(_gpr.HL), 7)); break; // RES 7,(HL)
                case 0xBF: _afr.A = ResetBit(_afr.A, 7); break; // RES 7,A

                case 0xC0: _gpr.B = SetBit(_gpr.B, 0); break; // SET 0,B
                case 0xC1: _gpr.C = SetBit(_gpr.C, 0); break; // SET 0,C
                case 0xC2: _gpr.D = SetBit(_gpr.D, 0); break; // SET 0,D
                case 0xC3: _gpr.E = SetBit(_gpr.E, 0); break; // SET 0,E
                case 0xC4: _gpr.H = SetBit(_gpr.H, 0); break; // SET 0,H
                case 0xC5: _gpr.L = SetBit(_gpr.L, 0); break; // SET 0,L
                case 0xC6: WriteByte(_gpr.HL, SetBit(ReadByte(_gpr.HL), 0)); break; // SET 0,(HL)
                case 0xC7: _afr.A = SetBit(_afr.A, 0); break; // SET 0,A

                case 0xC8: _gpr.B = SetBit(_gpr.B, 1); break; // SET 1,B
                case 0xC9: _gpr.C = SetBit(_gpr.C, 1); break; // SET 1,C
                case 0xCA: _gpr.D = SetBit(_gpr.D, 1); break; // SET 1,D
                case 0xCB: _gpr.E = SetBit(_gpr.E, 1); break; // SET 1,E
                case 0xCC: _gpr.H = SetBit(_gpr.H, 1); break; // SET 1,H
                case 0xCD: _gpr.L = SetBit(_gpr.L, 1); break; // SET 1,L
                case 0xCE: WriteByte(_gpr.HL, SetBit(ReadByte(_gpr.HL), 1)); break; // SET 1,(HL)
                case 0xCF: _afr.A = SetBit(_afr.A, 1); break; // SET 1,A

                case 0xD0: _gpr.B = SetBit(_gpr.B, 2); break; // SET 2,B
                case 0xD1: _gpr.C = SetBit(_gpr.C, 2); break; // SET 2,C
                case 0xD2: _gpr.D = SetBit(_gpr.D, 2); break; // SET 2,D
                case 0xD3: _gpr.E = SetBit(_gpr.E, 2); break; // SET 2,E
                case 0xD4: _gpr.H = SetBit(_gpr.H, 2); break; // SET 2,H
                case 0xD5: _gpr.L = SetBit(_gpr.L, 2); break; // SET 2,L
                case 0xD6: WriteByte(_gpr.HL, SetBit(ReadByte(_gpr.HL), 2)); break; // SET 2,(HL)
                case 0xD7: _afr.A = SetBit(_afr.A, 2); break; // SET 2,A

                case 0xD8: _gpr.B = SetBit(_gpr.B, 3); break; // SET 3,B
                case 0xD9: _gpr.C = SetBit(_gpr.C, 3); break; // SET 3,C
                case 0xDA: _gpr.D = SetBit(_gpr.D, 3); break; // SET 3,D
                case 0xDB: _gpr.E = SetBit(_gpr.E, 3); break; // SET 3,E
                case 0xDC: _gpr.H = SetBit(_gpr.H, 3); break; // SET 3,H
                case 0xDD: _gpr.L = SetBit(_gpr.L, 3); break; // SET 3,L
                case 0xDE: WriteByte(_gpr.HL, SetBit(ReadByte(_gpr.HL), 3)); break; // SET 3,(HL)
                case 0xDF: _afr.A = SetBit(_afr.A, 3); break; // SET 3,A

                case 0xE0: _gpr.B = SetBit(_gpr.B, 4); break; // SET 4,B
                case 0xE1: _gpr.C = SetBit(_gpr.C, 4); break; // SET 4,C
                case 0xE2: _gpr.D = SetBit(_gpr.D, 4); break; // SET 4,D
                case 0xE3: _gpr.E = SetBit(_gpr.E, 4); break; // SET 4,E
                case 0xE4: _gpr.H = SetBit(_gpr.H, 4); break; // SET 4,H
                case 0xE5: _gpr.L = SetBit(_gpr.L, 4); break; // SET 4,L
                case 0xE6: WriteByte(_gpr.HL, SetBit(ReadByte(_gpr.HL), 4)); break; // SET 4,(HL)
                case 0xE7: _afr.A = SetBit(_afr.A, 4); break; // SET 4,A

                case 0xE8: _gpr.B = SetBit(_gpr.B, 5); break; // SET 5,B
                case 0xE9: _gpr.C = SetBit(_gpr.C, 5); break; // SET 5,C
                case 0xEA: _gpr.D = SetBit(_gpr.D, 5); break; // SET 5,D
                case 0xEB: _gpr.E = SetBit(_gpr.E, 5); break; // SET 5,E
                case 0xEC: _gpr.H = SetBit(_gpr.H, 5); break; // SET 5,H
                case 0xED: _gpr.L = SetBit(_gpr.L, 5); break; // SET 5,L
                case 0xEE: WriteByte(_gpr.HL, SetBit(ReadByte(_gpr.HL), 5)); break; // SET 5,(HL)
                case 0xEF: _afr.A = SetBit(_afr.A, 5); break; // SET 5,A

                case 0xF0: _gpr.B = SetBit(_gpr.B, 6); break; // SET 6,B
                case 0xF1: _gpr.C = SetBit(_gpr.C, 6); break; // SET 6,C
                case 0xF2: _gpr.D = SetBit(_gpr.D, 6); break; // SET 6,D
                case 0xF3: _gpr.E = SetBit(_gpr.E, 6); break; // SET 6,E
                case 0xF4: _gpr.H = SetBit(_gpr.H, 6); break; // SET 6,H
                case 0xF5: _gpr.L = SetBit(_gpr.L, 6); break; // SET 6,L
                case 0xF6: WriteByte(_gpr.HL, SetBit(ReadByte(_gpr.HL), 6)); break; // SET 6,(HL)
                case 0xF7: _afr.A = SetBit(_afr.A, 6); break; // SET 6,A

                case 0xF8: _gpr.B = SetBit(_gpr.B, 7); break; // SET 7,B
                case 0xF9: _gpr.C = SetBit(_gpr.C, 7); break; // SET 7,C
                case 0xFA: _gpr.D = SetBit(_gpr.D, 7); break; // SET 7,D
                case 0xFB: _gpr.E = SetBit(_gpr.E, 7); break; // SET 7,E
                case 0xFC: _gpr.H = SetBit(_gpr.H, 7); break; // SET 7,H
                case 0xFD: _gpr.L = SetBit(_gpr.L, 7); break; // SET 7,L
                case 0xFE: WriteByte(_gpr.HL, SetBit(ReadByte(_gpr.HL), 7)); break; // SET 7,(HL)
                case 0xFF: _afr.A = SetBit(_afr.A, 7); break; // SET 7,A

                #endregion

                default:
                    throw new NotImplementedException($"Unimplemented opcode: 0xCB {opcode:X2} at address 0x{_pc - 2:X4}");
            }
        }

        private void ExecuteDisplacedCBPrefixedOpcode(ushort address, byte opcode)
        {
            switch (opcode)
            {
                #region rotate and shift group

                case 0x00: WriteByte(address, (_gpr.B = RotateLeft(ReadByte(address)))); _clock.AddCycles(1); break; // RLC (IX/IY + d)
                case 0x01: WriteByte(address, (_gpr.C = RotateLeft(ReadByte(address)))); _clock.AddCycles(1); break; // RLC (IX/IY + d)
                case 0x02: WriteByte(address, (_gpr.D = RotateLeft(ReadByte(address)))); _clock.AddCycles(1); break; // RLC (IX/IY + d)
                case 0x03: WriteByte(address, (_gpr.E = RotateLeft(ReadByte(address)))); _clock.AddCycles(1); break; // RLC (IX/IY + d)
                case 0x04: WriteByte(address, (_gpr.H = RotateLeft(ReadByte(address)))); _clock.AddCycles(1); break; // RLC (IX/IY + d)
                case 0x05: WriteByte(address, (_gpr.L = RotateLeft(ReadByte(address)))); _clock.AddCycles(1); break; // RLC (IX/IY + d)
                case 0x06: WriteByte(address, RotateLeft(ReadByte(address))); _clock.AddCycles(1); break; // RLC (IX/IY + d)
                case 0x07: WriteByte(address, (_afr.A = RotateLeft(ReadByte(address)))); _clock.AddCycles(1); break; // RLC (IX/IY + d)

                case 0x08: WriteByte(address, (_gpr.B = RotateRight(ReadByte(address)))); _clock.AddCycles(1); break; // RRC (IX/IY + d)
                case 0x09: WriteByte(address, (_gpr.C = RotateRight(ReadByte(address)))); _clock.AddCycles(1); break; // RRC (IX/IY + d)
                case 0x0A: WriteByte(address, (_gpr.D = RotateRight(ReadByte(address)))); _clock.AddCycles(1); break; // RRC (IX/IY + d)
                case 0x0B: WriteByte(address, (_gpr.E = RotateRight(ReadByte(address)))); _clock.AddCycles(1); break; // RRC (IX/IY + d)
                case 0x0C: WriteByte(address, (_gpr.H = RotateRight(ReadByte(address)))); _clock.AddCycles(1); break; // RRC (IX/IY + d)
                case 0x0D: WriteByte(address, (_gpr.L = RotateRight(ReadByte(address)))); _clock.AddCycles(1); break; // RRC (IX/IY + d)
                case 0x0E: WriteByte(address, RotateRight(ReadByte(address))); _clock.AddCycles(1); break; // RRC (IX/IY + d)
                case 0x0F: WriteByte(address, (_afr.A = RotateRight(ReadByte(address)))); _clock.AddCycles(1); break; // RRC (IX/IY + d)

                case 0x10: WriteByte(address, (_gpr.B = RotateLeftThroughCarry(ReadByte(address)))); _clock.AddCycles(1); break; // RL (IX/IY + d)
                case 0x11: WriteByte(address, (_gpr.C = RotateLeftThroughCarry(ReadByte(address)))); _clock.AddCycles(1); break; // RL (IX/IY + d)
                case 0x12: WriteByte(address, (_gpr.D = RotateLeftThroughCarry(ReadByte(address)))); _clock.AddCycles(1); break; // RL (IX/IY + d)
                case 0x13: WriteByte(address, (_gpr.E = RotateLeftThroughCarry(ReadByte(address)))); _clock.AddCycles(1); break; // RL (IX/IY + d)
                case 0x14: WriteByte(address, (_gpr.H = RotateLeftThroughCarry(ReadByte(address)))); _clock.AddCycles(1); break; // RL (IX/IY + d)
                case 0x15: WriteByte(address, (_gpr.L = RotateLeftThroughCarry(ReadByte(address)))); _clock.AddCycles(1); break; // RL (IX/IY + d)
                case 0x16: WriteByte(address, RotateLeftThroughCarry(ReadByte(address))); _clock.AddCycles(1); break; // RL (IX/IY + d)
                case 0x17: WriteByte(address, (_afr.A = RotateLeftThroughCarry(ReadByte(address)))); _clock.AddCycles(1); break; // RL (IX/IY + d)

                case 0x18: WriteByte(address, (_gpr.B = RotateRightThroughCarry(ReadByte(address)))); _clock.AddCycles(1); break; // RR (IX/IY + d)
                case 0x19: WriteByte(address, (_gpr.C = RotateRightThroughCarry(ReadByte(address)))); _clock.AddCycles(1); break; // RR (IX/IY + d)
                case 0x1A: WriteByte(address, (_gpr.D = RotateRightThroughCarry(ReadByte(address)))); _clock.AddCycles(1); break; // RR (IX/IY + d)
                case 0x1B: WriteByte(address, (_gpr.E = RotateRightThroughCarry(ReadByte(address)))); _clock.AddCycles(1); break; // RR (IX/IY + d)
                case 0x1C: WriteByte(address, (_gpr.H = RotateRightThroughCarry(ReadByte(address)))); _clock.AddCycles(1); break; // RR (IX/IY + d)
                case 0x1D: WriteByte(address, (_gpr.L = RotateRightThroughCarry(ReadByte(address)))); _clock.AddCycles(1); break; // RR (IX/IY + d)
                case 0x1E: WriteByte(address, RotateRightThroughCarry(ReadByte(address))); _clock.AddCycles(1); break; // RR (IX/IY + d)
                case 0x1F: WriteByte(address, (_afr.A = RotateRightThroughCarry(ReadByte(address)))); _clock.AddCycles(1); break; // RR (IX/IY + d)

                case 0x20: WriteByte(address, (_gpr.B = ShiftLeftArithmetic(ReadByte(address)))); _clock.AddCycles(1); break; // SLA (IX/IY + d)
                case 0x21: WriteByte(address, (_gpr.C = ShiftLeftArithmetic(ReadByte(address)))); _clock.AddCycles(1); break; // SLA (IX/IY + d)
                case 0x22: WriteByte(address, (_gpr.D = ShiftLeftArithmetic(ReadByte(address)))); _clock.AddCycles(1); break; // SLA (IX/IY + d)
                case 0x23: WriteByte(address, (_gpr.E = ShiftLeftArithmetic(ReadByte(address)))); _clock.AddCycles(1); break; // SLA (IX/IY + d)
                case 0x24: WriteByte(address, (_gpr.H = ShiftLeftArithmetic(ReadByte(address)))); _clock.AddCycles(1); break; // SLA (IX/IY + d)
                case 0x25: WriteByte(address, (_gpr.L = ShiftLeftArithmetic(ReadByte(address)))); _clock.AddCycles(1); break; // SLA (IX/IY + d)
                case 0x26: WriteByte(address, ShiftLeftArithmetic(ReadByte(address))); _clock.AddCycles(1); break; // SLA (IX/IY + d)
                case 0x27: WriteByte(address, (_afr.A = ShiftLeftArithmetic(ReadByte(address)))); _clock.AddCycles(1); break; // SLA (IX/IY + d)

                case 0x28: WriteByte(address, (_gpr.B = ShiftRightArithmetic(ReadByte(address)))); _clock.AddCycles(1); break; // SRA (IX/IY + d)
                case 0x29: WriteByte(address, (_gpr.C = ShiftRightArithmetic(ReadByte(address)))); _clock.AddCycles(1); break; // SRA (IX/IY + d)
                case 0x2A: WriteByte(address, (_gpr.D = ShiftRightArithmetic(ReadByte(address)))); _clock.AddCycles(1); break; // SRA (IX/IY + d)
                case 0x2B: WriteByte(address, (_gpr.E = ShiftRightArithmetic(ReadByte(address)))); _clock.AddCycles(1); break; // SRA (IX/IY + d)
                case 0x2C: WriteByte(address, (_gpr.H = ShiftRightArithmetic(ReadByte(address)))); _clock.AddCycles(1); break; // SRA (IX/IY + d)
                case 0x2D: WriteByte(address, (_gpr.L = ShiftRightArithmetic(ReadByte(address)))); _clock.AddCycles(1); break; // SRA (IX/IY + d)
                case 0x2E: WriteByte(address, ShiftRightArithmetic(ReadByte(address))); _clock.AddCycles(1); break; // SRA (IX/IY + d)
                case 0x2F: WriteByte(address, (_afr.A = ShiftRightArithmetic(ReadByte(address)))); _clock.AddCycles(1); break; // SRA (IX/IY + d)

                case 0x30: WriteByte(address, (_gpr.B = ShiftLeftInsertingOne(ReadByte(address)))); _clock.AddCycles(1); break; // SLL (IX/IY + d)
                case 0x31: WriteByte(address, (_gpr.C = ShiftLeftInsertingOne(ReadByte(address)))); _clock.AddCycles(1); break; // SLL (IX/IY + d)
                case 0x32: WriteByte(address, (_gpr.D = ShiftLeftInsertingOne(ReadByte(address)))); _clock.AddCycles(1); break; // SLL (IX/IY + d)
                case 0x33: WriteByte(address, (_gpr.E = ShiftLeftInsertingOne(ReadByte(address)))); _clock.AddCycles(1); break; // SLL (IX/IY + d)
                case 0x34: WriteByte(address, (_gpr.H = ShiftLeftInsertingOne(ReadByte(address)))); _clock.AddCycles(1); break; // SLL (IX/IY + d)
                case 0x35: WriteByte(address, (_gpr.L = ShiftLeftInsertingOne(ReadByte(address)))); _clock.AddCycles(1); break; // SLL (IX/IY + d)
                case 0x36: WriteByte(address, ShiftLeftInsertingOne(ReadByte(address))); _clock.AddCycles(1); break; // SLL (IX/IY + d)
                case 0x37: WriteByte(address, (_afr.A = ShiftLeftInsertingOne(ReadByte(address)))); _clock.AddCycles(1); break; // SLL (IX/IY + d)

                case 0x38: WriteByte(address, (_gpr.B = ShiftRightLogical(ReadByte(address)))); _clock.AddCycles(1); break; // SRL (IX/IY + d)
                case 0x39: WriteByte(address, (_gpr.C = ShiftRightLogical(ReadByte(address)))); _clock.AddCycles(1); break; // SRL (IX/IY + d)
                case 0x3A: WriteByte(address, (_gpr.D = ShiftRightLogical(ReadByte(address)))); _clock.AddCycles(1); break; // SRL (IX/IY + d)
                case 0x3B: WriteByte(address, (_gpr.E = ShiftRightLogical(ReadByte(address)))); _clock.AddCycles(1); break; // SRL (IX/IY + d)
                case 0x3C: WriteByte(address, (_gpr.H = ShiftRightLogical(ReadByte(address)))); _clock.AddCycles(1); break; // SRL (IX/IY + d)
                case 0x3D: WriteByte(address, (_gpr.L = ShiftRightLogical(ReadByte(address)))); _clock.AddCycles(1); break; // SRL (IX/IY + d)
                case 0x3E: WriteByte(address, ShiftRightLogical(ReadByte(address))); _clock.AddCycles(1); break; // SRL (IX/IY + d)
                case 0x3F: WriteByte(address, (_afr.A = ShiftRightLogical(ReadByte(address)))); _clock.AddCycles(1); break; // SRL (IX/IY + d)

                #endregion

                #region bit set, reset, and test group

                case 0x40: _afr.F = TestBitMemory(address, 0); break; // BIT 0,(IX/IY + d)
                case 0x41: _afr.F = TestBitMemory(address, 0); break; // BIT 0,(IX/IY + d)
                case 0x42: _afr.F = TestBitMemory(address, 0); break; // BIT 0,(IX/IY + d)
                case 0x43: _afr.F = TestBitMemory(address, 0); break; // BIT 0,(IX/IY + d)
                case 0x44: _afr.F = TestBitMemory(address, 0); break; // BIT 0,(IX/IY + d)
                case 0x45: _afr.F = TestBitMemory(address, 0); break; // BIT 0,(IX/IY + d)
                case 0x46: _afr.F = TestBitMemory(address, 0); break; // BIT 0,(IX/IY + d)
                case 0x47: _afr.F = TestBitMemory(address, 0); break; // BIT 0,(IX/IY + d)

                case 0x48: _afr.F = TestBitMemory(address, 1); break; // BIT 1,(IX/IY + d)
                case 0x49: _afr.F = TestBitMemory(address, 1); break; // BIT 1,(IX/IY + d)
                case 0x4A: _afr.F = TestBitMemory(address, 1); break; // BIT 1,(IX/IY + d)
                case 0x4B: _afr.F = TestBitMemory(address, 1); break; // BIT 1,(IX/IY + d)
                case 0x4C: _afr.F = TestBitMemory(address, 1); break; // BIT 1,(IX/IY + d)
                case 0x4D: _afr.F = TestBitMemory(address, 1); break; // BIT 1,(IX/IY + d)
                case 0x4E: _afr.F = TestBitMemory(address, 1); break; // BIT 1,(IX/IY + d)
                case 0x4F: _afr.F = TestBitMemory(address, 1); break; // BIT 1,(IX/IY + d)

                case 0x50: _afr.F = TestBitMemory(address, 2); break; // BIT 2,(IX/IY + d)
                case 0x51: _afr.F = TestBitMemory(address, 2); break; // BIT 2,(IX/IY + d)
                case 0x52: _afr.F = TestBitMemory(address, 2); break; // BIT 2,(IX/IY + d)
                case 0x53: _afr.F = TestBitMemory(address, 2); break; // BIT 2,(IX/IY + d)
                case 0x54: _afr.F = TestBitMemory(address, 2); break; // BIT 2,(IX/IY + d)
                case 0x55: _afr.F = TestBitMemory(address, 2); break; // BIT 2,(IX/IY + d)
                case 0x56: _afr.F = TestBitMemory(address, 2); break; // BIT 2,(IX/IY + d)
                case 0x57: _afr.F = TestBitMemory(address, 2); break; // BIT 2,(IX/IY + d)

                case 0x58: _afr.F = TestBitMemory(address, 3); break; // BIT 3,(IX/IY + d)
                case 0x59: _afr.F = TestBitMemory(address, 3); break; // BIT 3,(IX/IY + d)
                case 0x5A: _afr.F = TestBitMemory(address, 3); break; // BIT 3,(IX/IY + d)
                case 0x5B: _afr.F = TestBitMemory(address, 3); break; // BIT 3,(IX/IY + d)
                case 0x5C: _afr.F = TestBitMemory(address, 3); break; // BIT 3,(IX/IY + d)
                case 0x5D: _afr.F = TestBitMemory(address, 3); break; // BIT 3,(IX/IY + d)
                case 0x5E: _afr.F = TestBitMemory(address, 3); break; // BIT 3,(IX/IY + d)
                case 0x5F: _afr.F = TestBitMemory(address, 3); break; // BIT 3,(IX/IY + d)

                case 0x60: _afr.F = TestBitMemory(address, 4); break; // BIT 4,(IX/IY + d)
                case 0x61: _afr.F = TestBitMemory(address, 4); break; // BIT 4,(IX/IY + d)
                case 0x62: _afr.F = TestBitMemory(address, 4); break; // BIT 4,(IX/IY + d)
                case 0x63: _afr.F = TestBitMemory(address, 4); break; // BIT 4,(IX/IY + d)
                case 0x64: _afr.F = TestBitMemory(address, 4); break; // BIT 4,(IX/IY + d)
                case 0x65: _afr.F = TestBitMemory(address, 4); break; // BIT 4,(IX/IY + d)
                case 0x66: _afr.F = TestBitMemory(address, 4); break; // BIT 4,(IX/IY + d)
                case 0x67: _afr.F = TestBitMemory(address, 4); break; // BIT 4,(IX/IY + d)

                case 0x68: _afr.F = TestBitMemory(address, 5); break; // BIT 5,(IX/IY + d)
                case 0x69: _afr.F = TestBitMemory(address, 5); break; // BIT 5,(IX/IY + d)
                case 0x6A: _afr.F = TestBitMemory(address, 5); break; // BIT 5,(IX/IY + d)
                case 0x6B: _afr.F = TestBitMemory(address, 5); break; // BIT 5,(IX/IY + d)
                case 0x6C: _afr.F = TestBitMemory(address, 5); break; // BIT 5,(IX/IY + d)
                case 0x6D: _afr.F = TestBitMemory(address, 5); break; // BIT 5,(IX/IY + d)
                case 0x6E: _afr.F = TestBitMemory(address, 5); break; // BIT 5,(IX/IY + d)
                case 0x6F: _afr.F = TestBitMemory(address, 5); break; // BIT 5,(IX/IY + d)

                case 0x70: _afr.F = TestBitMemory(address, 6); break; // BIT 6,(IX/IY + d)
                case 0x71: _afr.F = TestBitMemory(address, 6); break; // BIT 6,(IX/IY + d)
                case 0x72: _afr.F = TestBitMemory(address, 6); break; // BIT 6,(IX/IY + d)
                case 0x73: _afr.F = TestBitMemory(address, 6); break; // BIT 6,(IX/IY + d)
                case 0x74: _afr.F = TestBitMemory(address, 6); break; // BIT 6,(IX/IY + d)
                case 0x75: _afr.F = TestBitMemory(address, 6); break; // BIT 6,(IX/IY + d)
                case 0x76: _afr.F = TestBitMemory(address, 6); break; // BIT 6,(IX/IY + d)
                case 0x77: _afr.F = TestBitMemory(address, 6); break; // BIT 6,(IX/IY + d)

                case 0x78: _afr.F = TestBitMemory(address, 7); break; // BIT 7,(IX/IY + d)
                case 0x79: _afr.F = TestBitMemory(address, 7); break; // BIT 7,(IX/IY + d)
                case 0x7A: _afr.F = TestBitMemory(address, 7); break; // BIT 7,(IX/IY + d)
                case 0x7B: _afr.F = TestBitMemory(address, 7); break; // BIT 7,(IX/IY + d)
                case 0x7C: _afr.F = TestBitMemory(address, 7); break; // BIT 7,(IX/IY + d)
                case 0x7D: _afr.F = TestBitMemory(address, 7); break; // BIT 7,(IX/IY + d)
                case 0x7E: _afr.F = TestBitMemory(address, 7); break; // BIT 7,(IX/IY + d)
                case 0x7F: _afr.F = TestBitMemory(address, 7); break; // BIT 7,(IX/IY + d)

                case 0x80: WriteByte(address, (_gpr.B = ResetBit(ReadByte(address), 0))); _clock.AddCycles(1); break; // RES 0,(IX/IY + d),B
                case 0x81: WriteByte(address, (_gpr.C = ResetBit(ReadByte(address), 0))); _clock.AddCycles(1); break; // RES 0,(IX/IY + d),C
                case 0x82: WriteByte(address, (_gpr.D = ResetBit(ReadByte(address), 0))); _clock.AddCycles(1); break; // RES 0,(IX/IY + d),D
                case 0x83: WriteByte(address, (_gpr.E = ResetBit(ReadByte(address), 0))); _clock.AddCycles(1); break; // RES 0,(IX/IY + d),E
                case 0x84: WriteByte(address, (_gpr.H = ResetBit(ReadByte(address), 0))); _clock.AddCycles(1); break; // RES 0,(IX/IY + d),H
                case 0x85: WriteByte(address, (_gpr.L = ResetBit(ReadByte(address), 0))); _clock.AddCycles(1); break; // RES 0,(IX/IY + d),L
                case 0x86: WriteByte(address, ResetBit(ReadByte(address), 0)); _clock.AddCycles(1); break; // RES 0,(IX/IY + d)
                case 0x87: WriteByte(address, (_afr.A = ResetBit(ReadByte(address), 0))); _clock.AddCycles(1); break; // RES 0,(IX/IY + d),A

                case 0x88: WriteByte(address, (_gpr.B = ResetBit(ReadByte(address), 1))); _clock.AddCycles(1); break; // RES 1,(IX/IY + d),B
                case 0x89: WriteByte(address, (_gpr.C = ResetBit(ReadByte(address), 1))); _clock.AddCycles(1); break; // RES 1,(IX/IY + d),C
                case 0x8A: WriteByte(address, (_gpr.D = ResetBit(ReadByte(address), 1))); _clock.AddCycles(1); break; // RES 1,(IX/IY + d),D
                case 0x8B: WriteByte(address, (_gpr.E = ResetBit(ReadByte(address), 1))); _clock.AddCycles(1); break; // RES 1,(IX/IY + d),E
                case 0x8C: WriteByte(address, (_gpr.H = ResetBit(ReadByte(address), 1))); _clock.AddCycles(1); break; // RES 1,(IX/IY + d),H
                case 0x8D: WriteByte(address, (_gpr.L = ResetBit(ReadByte(address), 1))); _clock.AddCycles(1); break; // RES 1,(IX/IY + d),L
                case 0x8E: WriteByte(address, ResetBit(ReadByte(address), 1)); _clock.AddCycles(1); break; // RES 1,(IX/IY + d)
                case 0x8F: WriteByte(address, (_afr.A = ResetBit(ReadByte(address), 1))); _clock.AddCycles(1); break; // RES 1,(IX/IY + d),A

                case 0x90: WriteByte(address, (_gpr.B = ResetBit(ReadByte(address), 2))); _clock.AddCycles(1); break; // RES 2,(IX/IY + d),B
                case 0x91: WriteByte(address, (_gpr.C = ResetBit(ReadByte(address), 2))); _clock.AddCycles(1); break; // RES 2,(IX/IY + d),C
                case 0x92: WriteByte(address, (_gpr.D = ResetBit(ReadByte(address), 2))); _clock.AddCycles(1); break; // RES 2,(IX/IY + d),D
                case 0x93: WriteByte(address, (_gpr.E = ResetBit(ReadByte(address), 2))); _clock.AddCycles(1); break; // RES 2,(IX/IY + d),E
                case 0x94: WriteByte(address, (_gpr.H = ResetBit(ReadByte(address), 2))); _clock.AddCycles(1); break; // RES 2,(IX/IY + d),H
                case 0x95: WriteByte(address, (_gpr.L = ResetBit(ReadByte(address), 2))); _clock.AddCycles(1); break; // RES 2,(IX/IY + d),L
                case 0x96: WriteByte(address, ResetBit(ReadByte(address), 2)); _clock.AddCycles(1); break; // RES 2,(IX/IY + d)
                case 0x97: WriteByte(address, (_afr.A = ResetBit(ReadByte(address), 2))); _clock.AddCycles(1); break; // RES 2,(IX/IY + d),A

                case 0x98: WriteByte(address, (_gpr.B = ResetBit(ReadByte(address), 3))); _clock.AddCycles(1); break; // RES 3,(IX/IY + d),B
                case 0x99: WriteByte(address, (_gpr.C = ResetBit(ReadByte(address), 3))); _clock.AddCycles(1); break; // RES 3,(IX/IY + d),C
                case 0x9A: WriteByte(address, (_gpr.D = ResetBit(ReadByte(address), 3))); _clock.AddCycles(1); break; // RES 3,(IX/IY + d),D
                case 0x9B: WriteByte(address, (_gpr.E = ResetBit(ReadByte(address), 3))); _clock.AddCycles(1); break; // RES 3,(IX/IY + d),E
                case 0x9C: WriteByte(address, (_gpr.H = ResetBit(ReadByte(address), 3))); _clock.AddCycles(1); break; // RES 3,(IX/IY + d),H
                case 0x9D: WriteByte(address, (_gpr.L = ResetBit(ReadByte(address), 3))); _clock.AddCycles(1); break; // RES 3,(IX/IY + d),L
                case 0x9E: WriteByte(address, ResetBit(ReadByte(address), 3)); _clock.AddCycles(1); break; // RES 3,(IX/IY + d)
                case 0x9F: WriteByte(address, (_afr.A = ResetBit(ReadByte(address), 3))); _clock.AddCycles(1); break; // RES 3,(IX/IY + d),A

                case 0xA0: WriteByte(address, (_gpr.B = ResetBit(ReadByte(address), 4))); _clock.AddCycles(1); break; // RES 4,(IX/IY + d),B
                case 0xA1: WriteByte(address, (_gpr.C = ResetBit(ReadByte(address), 4))); _clock.AddCycles(1); break; // RES 4,(IX/IY + d),C
                case 0xA2: WriteByte(address, (_gpr.D = ResetBit(ReadByte(address), 4))); _clock.AddCycles(1); break; // RES 4,(IX/IY + d),D
                case 0xA3: WriteByte(address, (_gpr.E = ResetBit(ReadByte(address), 4))); _clock.AddCycles(1); break; // RES 4,(IX/IY + d),E
                case 0xA4: WriteByte(address, (_gpr.H = ResetBit(ReadByte(address), 4))); _clock.AddCycles(1); break; // RES 4,(IX/IY + d),H
                case 0xA5: WriteByte(address, (_gpr.L = ResetBit(ReadByte(address), 4))); _clock.AddCycles(1); break; // RES 4,(IX/IY + d),L
                case 0xA6: WriteByte(address, ResetBit(ReadByte(address), 4)); _clock.AddCycles(1); break; // RES 4,(IX/IY + d)
                case 0xA7: WriteByte(address, (_afr.A = ResetBit(ReadByte(address), 4))); _clock.AddCycles(1); break; // RES 4,(IX/IY + d),A

                case 0xA8: WriteByte(address, (_gpr.B = ResetBit(ReadByte(address), 5))); _clock.AddCycles(1); break; // RES 5,(IX/IY + d),B
                case 0xA9: WriteByte(address, (_gpr.C = ResetBit(ReadByte(address), 5))); _clock.AddCycles(1); break; // RES 5,(IX/IY + d),C
                case 0xAA: WriteByte(address, (_gpr.D = ResetBit(ReadByte(address), 5))); _clock.AddCycles(1); break; // RES 5,(IX/IY + d),D
                case 0xAB: WriteByte(address, (_gpr.E = ResetBit(ReadByte(address), 5))); _clock.AddCycles(1); break; // RES 5,(IX/IY + d),E
                case 0xAC: WriteByte(address, (_gpr.H = ResetBit(ReadByte(address), 5))); _clock.AddCycles(1); break; // RES 5,(IX/IY + d),H
                case 0xAD: WriteByte(address, (_gpr.L = ResetBit(ReadByte(address), 5))); _clock.AddCycles(1); break; // RES 5,(IX/IY + d),L
                case 0xAE: WriteByte(address, ResetBit(ReadByte(address), 5)); _clock.AddCycles(1); break; // RES 5,(IX/IY + d)
                case 0xAF: WriteByte(address, (_afr.A = ResetBit(ReadByte(address), 5))); _clock.AddCycles(1); break; // RES 5,(IX/IY + d),A

                case 0xB0: WriteByte(address, (_gpr.B = ResetBit(ReadByte(address), 6))); _clock.AddCycles(1); break; // RES 6,(IX/IY + d),B
                case 0xB1: WriteByte(address, (_gpr.C = ResetBit(ReadByte(address), 6))); _clock.AddCycles(1); break; // RES 6,(IX/IY + d),C
                case 0xB2: WriteByte(address, (_gpr.D = ResetBit(ReadByte(address), 6))); _clock.AddCycles(1); break; // RES 6,(IX/IY + d),D
                case 0xB3: WriteByte(address, (_gpr.E = ResetBit(ReadByte(address), 6))); _clock.AddCycles(1); break; // RES 6,(IX/IY + d),E
                case 0xB4: WriteByte(address, (_gpr.H = ResetBit(ReadByte(address), 6))); _clock.AddCycles(1); break; // RES 6,(IX/IY + d),H
                case 0xB5: WriteByte(address, (_gpr.L = ResetBit(ReadByte(address), 6))); _clock.AddCycles(1); break; // RES 6,(IX/IY + d),L
                case 0xB6: WriteByte(address, ResetBit(ReadByte(address), 6)); _clock.AddCycles(1); break; // RES 6,(IX/IY + d)
                case 0xB7: WriteByte(address, (_afr.A = ResetBit(ReadByte(address), 6))); _clock.AddCycles(1); break; // RES 6,(IX/IY + d),A

                case 0xB8: WriteByte(address, (_gpr.B = ResetBit(ReadByte(address), 7))); _clock.AddCycles(1); break; // RES 7,(IX/IY + d),B
                case 0xB9: WriteByte(address, (_gpr.C = ResetBit(ReadByte(address), 7))); _clock.AddCycles(1); break; // RES 7,(IX/IY + d),C
                case 0xBA: WriteByte(address, (_gpr.D = ResetBit(ReadByte(address), 7))); _clock.AddCycles(1); break; // RES 7,(IX/IY + d),D
                case 0xBB: WriteByte(address, (_gpr.E = ResetBit(ReadByte(address), 7))); _clock.AddCycles(1); break; // RES 7,(IX/IY + d),E
                case 0xBC: WriteByte(address, (_gpr.H = ResetBit(ReadByte(address), 7))); _clock.AddCycles(1); break; // RES 7,(IX/IY + d),H
                case 0xBD: WriteByte(address, (_gpr.L = ResetBit(ReadByte(address), 7))); _clock.AddCycles(1); break; // RES 7,(IX/IY + d),L
                case 0xBE: WriteByte(address, ResetBit(ReadByte(address), 7)); _clock.AddCycles(1); break; // RES 7,(IX/IY + d)
                case 0xBF: WriteByte(address, (_afr.A = ResetBit(ReadByte(address), 7))); _clock.AddCycles(1); break; // RES 7,(IX/IY + d),A

                case 0xC0: WriteByte(address, (_gpr.B = SetBit(ReadByte(address), 0))); _clock.AddCycles(1); break; // SET 0,(IX/IY + d),B
                case 0xC1: WriteByte(address, (_gpr.C = SetBit(ReadByte(address), 0))); _clock.AddCycles(1); break; // SET 0,(IX/IY + d),C
                case 0xC2: WriteByte(address, (_gpr.D = SetBit(ReadByte(address), 0))); _clock.AddCycles(1); break; // SET 0,(IX/IY + d),D
                case 0xC3: WriteByte(address, (_gpr.E = SetBit(ReadByte(address), 0))); _clock.AddCycles(1); break; // SET 0,(IX/IY + d),E
                case 0xC4: WriteByte(address, (_gpr.H = SetBit(ReadByte(address), 0))); _clock.AddCycles(1); break; // SET 0,(IX/IY + d),H
                case 0xC5: WriteByte(address, (_gpr.L = SetBit(ReadByte(address), 0))); _clock.AddCycles(1); break; // SET 0,(IX/IY + d),L
                case 0xC6: WriteByte(address, SetBit(ReadByte(address), 0)); _clock.AddCycles(1); break; // SET 0,(IX/IY + d)
                case 0xC7: WriteByte(address, (_afr.A = SetBit(ReadByte(address), 0))); _clock.AddCycles(1); break; // SET 0,(IX/IY + d),A

                case 0xC8: WriteByte(address, (_gpr.B = SetBit(ReadByte(address), 1))); _clock.AddCycles(1); break; // SET 1,(IX/IY + d),B
                case 0xC9: WriteByte(address, (_gpr.C = SetBit(ReadByte(address), 1))); _clock.AddCycles(1); break; // SET 1,(IX/IY + d),C
                case 0xCA: WriteByte(address, (_gpr.D = SetBit(ReadByte(address), 1))); _clock.AddCycles(1); break; // SET 1,(IX/IY + d),D
                case 0xCB: WriteByte(address, (_gpr.E = SetBit(ReadByte(address), 1))); _clock.AddCycles(1); break; // SET 1,(IX/IY + d),E
                case 0xCC: WriteByte(address, (_gpr.H = SetBit(ReadByte(address), 1))); _clock.AddCycles(1); break; // SET 1,(IX/IY + d),H
                case 0xCD: WriteByte(address, (_gpr.L = SetBit(ReadByte(address), 1))); _clock.AddCycles(1); break; // SET 1,(IX/IY + d),L
                case 0xCE: WriteByte(address, SetBit(ReadByte(address), 1)); _clock.AddCycles(1); break; // SET 1,(IX/IY + d)
                case 0xCF: WriteByte(address, (_afr.A = SetBit(ReadByte(address), 1))); _clock.AddCycles(1); break; // SET 1,(IX/IY + d),A

                case 0xD0: WriteByte(address, (_gpr.B = SetBit(ReadByte(address), 2))); _clock.AddCycles(1); break; // SET 2,(IX/IY + d),B
                case 0xD1: WriteByte(address, (_gpr.C = SetBit(ReadByte(address), 2))); _clock.AddCycles(1); break; // SET 2,(IX/IY + d),C
                case 0xD2: WriteByte(address, (_gpr.D = SetBit(ReadByte(address), 2))); _clock.AddCycles(1); break; // SET 2,(IX/IY + d),D
                case 0xD3: WriteByte(address, (_gpr.E = SetBit(ReadByte(address), 2))); _clock.AddCycles(1); break; // SET 2,(IX/IY + d),E
                case 0xD4: WriteByte(address, (_gpr.H = SetBit(ReadByte(address), 2))); _clock.AddCycles(1); break; // SET 2,(IX/IY + d),H
                case 0xD5: WriteByte(address, (_gpr.L = SetBit(ReadByte(address), 2))); _clock.AddCycles(1); break; // SET 2,(IX/IY + d),L
                case 0xD6: WriteByte(address, SetBit(ReadByte(address), 2)); _clock.AddCycles(1); break; // SET 2,(IX/IY + d)
                case 0xD7: WriteByte(address, (_afr.A = SetBit(ReadByte(address), 2))); _clock.AddCycles(1); break; // SET 2,(IX/IY + d),A

                case 0xD8: WriteByte(address, (_gpr.B = SetBit(ReadByte(address), 3))); _clock.AddCycles(1); break; // SET 3,(IX/IY + d),B
                case 0xD9: WriteByte(address, (_gpr.C = SetBit(ReadByte(address), 3))); _clock.AddCycles(1); break; // SET 3,(IX/IY + d),C
                case 0xDA: WriteByte(address, (_gpr.D = SetBit(ReadByte(address), 3))); _clock.AddCycles(1); break; // SET 3,(IX/IY + d),D
                case 0xDB: WriteByte(address, (_gpr.E = SetBit(ReadByte(address), 3))); _clock.AddCycles(1); break; // SET 3,(IX/IY + d),E
                case 0xDC: WriteByte(address, (_gpr.H = SetBit(ReadByte(address), 3))); _clock.AddCycles(1); break; // SET 3,(IX/IY + d),H
                case 0xDD: WriteByte(address, (_gpr.L = SetBit(ReadByte(address), 3))); _clock.AddCycles(1); break; // SET 3,(IX/IY + d),L
                case 0xDE: WriteByte(address, SetBit(ReadByte(address), 3)); _clock.AddCycles(1); break; // SET 3,(IX/IY + d)
                case 0xDF: WriteByte(address, (_afr.A = SetBit(ReadByte(address), 3))); _clock.AddCycles(1); break; // SET 3,(IX/IY + d),A

                case 0xE0: WriteByte(address, (_gpr.B = SetBit(ReadByte(address), 4))); _clock.AddCycles(1); break; // SET 4,(IX/IY + d),B
                case 0xE1: WriteByte(address, (_gpr.C = SetBit(ReadByte(address), 4))); _clock.AddCycles(1); break; // SET 4,(IX/IY + d),C
                case 0xE2: WriteByte(address, (_gpr.D = SetBit(ReadByte(address), 4))); _clock.AddCycles(1); break; // SET 4,(IX/IY + d),D
                case 0xE3: WriteByte(address, (_gpr.E = SetBit(ReadByte(address), 4))); _clock.AddCycles(1); break; // SET 4,(IX/IY + d),E
                case 0xE4: WriteByte(address, (_gpr.H = SetBit(ReadByte(address), 4))); _clock.AddCycles(1); break; // SET 4,(IX/IY + d),H
                case 0xE5: WriteByte(address, (_gpr.L = SetBit(ReadByte(address), 4))); _clock.AddCycles(1); break; // SET 4,(IX/IY + d),L
                case 0xE6: WriteByte(address, SetBit(ReadByte(address), 4)); _clock.AddCycles(1); break; // SET 4,(IX/IY + d)
                case 0xE7: WriteByte(address, (_afr.A = SetBit(ReadByte(address), 4))); _clock.AddCycles(1); break; // SET 4,(IX/IY + d),A

                case 0xE8: WriteByte(address, (_gpr.B = SetBit(ReadByte(address), 5))); _clock.AddCycles(1); break; // SET 5,(IX/IY + d),B
                case 0xE9: WriteByte(address, (_gpr.C = SetBit(ReadByte(address), 5))); _clock.AddCycles(1); break; // SET 5,(IX/IY + d),C
                case 0xEA: WriteByte(address, (_gpr.D = SetBit(ReadByte(address), 5))); _clock.AddCycles(1); break; // SET 5,(IX/IY + d),D
                case 0xEB: WriteByte(address, (_gpr.E = SetBit(ReadByte(address), 5))); _clock.AddCycles(1); break; // SET 5,(IX/IY + d),E
                case 0xEC: WriteByte(address, (_gpr.H = SetBit(ReadByte(address), 5))); _clock.AddCycles(1); break; // SET 5,(IX/IY + d),H
                case 0xED: WriteByte(address, (_gpr.L = SetBit(ReadByte(address), 5))); _clock.AddCycles(1); break; // SET 5,(IX/IY + d),L
                case 0xEE: WriteByte(address, SetBit(ReadByte(address), 5)); _clock.AddCycles(1); break; // SET 5,(IX/IY + d)
                case 0xEF: WriteByte(address, (_afr.A = SetBit(ReadByte(address), 5))); _clock.AddCycles(1); break; // SET 5,(IX/IY + d),A

                case 0xF0: WriteByte(address, (_gpr.B = SetBit(ReadByte(address), 6))); _clock.AddCycles(1); break; // SET 6,(IX/IY + d),B
                case 0xF1: WriteByte(address, (_gpr.C = SetBit(ReadByte(address), 6))); _clock.AddCycles(1); break; // SET 6,(IX/IY + d),C
                case 0xF2: WriteByte(address, (_gpr.D = SetBit(ReadByte(address), 6))); _clock.AddCycles(1); break; // SET 6,(IX/IY + d),D
                case 0xF3: WriteByte(address, (_gpr.E = SetBit(ReadByte(address), 6))); _clock.AddCycles(1); break; // SET 6,(IX/IY + d),E
                case 0xF4: WriteByte(address, (_gpr.H = SetBit(ReadByte(address), 6))); _clock.AddCycles(1); break; // SET 6,(IX/IY + d),H
                case 0xF5: WriteByte(address, (_gpr.L = SetBit(ReadByte(address), 6))); _clock.AddCycles(1); break; // SET 6,(IX/IY + d),L
                case 0xF6: WriteByte(address, SetBit(ReadByte(address), 6)); _clock.AddCycles(1); break; // SET 6,(IX/IY + d)
                case 0xF7: WriteByte(address, (_afr.A = SetBit(ReadByte(address), 6))); _clock.AddCycles(1); break; // SET 6,(IX/IY + d),A

                case 0xF8: WriteByte(address, (_gpr.B = SetBit(ReadByte(address), 7))); _clock.AddCycles(1); break; // SET 7,(IX/IY + d),B
                case 0xF9: WriteByte(address, (_gpr.C = SetBit(ReadByte(address), 7))); _clock.AddCycles(1); break; // SET 7,(IX/IY + d),C
                case 0xFA: WriteByte(address, (_gpr.D = SetBit(ReadByte(address), 7))); _clock.AddCycles(1); break; // SET 7,(IX/IY + d),D
                case 0xFB: WriteByte(address, (_gpr.E = SetBit(ReadByte(address), 7))); _clock.AddCycles(1); break; // SET 7,(IX/IY + d),E
                case 0xFC: WriteByte(address, (_gpr.H = SetBit(ReadByte(address), 7))); _clock.AddCycles(1); break; // SET 7,(IX/IY + d),H
                case 0xFD: WriteByte(address, (_gpr.L = SetBit(ReadByte(address), 7))); _clock.AddCycles(1); break; // SET 7,(IX/IY + d),L
                case 0xFE: WriteByte(address, SetBit(ReadByte(address), 7)); _clock.AddCycles(1); break; // SET 7,(IX/IY + d)
                case 0xFF: WriteByte(address, (_afr.A = SetBit(ReadByte(address), 7))); _clock.AddCycles(1); break; // SET 7,(IX/IY + d),A

                #endregion

                default:
                    throw new NotImplementedException($"Unimplemented opcode: 0xCB {opcode:X2} at address 0x{_pc - 2:X4}");
            }
        }

        #region bit set, reset, and test group handlers

        private StatusFlags TestBit(byte b, int bit)
        {
            var (_, flags) = And8Bit(b, (byte)(1 << bit));

			// this contradicts the documentation I could find, but appears to pass zexall
            flags |= b & (B5 | B3);

            return flags;
        }

        private StatusFlags TestBitMemory(ushort address, int bit)
        {
            var value = ReadByte(address);
            var (_, flags) = And8Bit(value, (byte)(1 << bit));

            flags[B5 | B3] = false;
            
            flags |= _memPtr.hi & (B5 | B3);

            return flags | (_afr.F & C);
        }

        private static byte SetBit(byte b, int bit)
        {
            return (byte)(b | (1 << bit));
        }

        private static byte ResetBit(byte b, int bit)
        {
            return (byte)(b & ~(1 << bit));
        }

        #endregion
    }
}
