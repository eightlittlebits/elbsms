namespace elbsms_core.Memory
{
    class MemoryControlRegister
    {
        private byte _value;

        public byte Value { set => _value = value; }

        public bool ExpansionSlotEnabled => (_value & 0x80) == 0;
        public bool CartridgeSlotEnabled => (_value & 0x40) == 0;
        public bool CardSlotEnabled => (_value & 0x20) == 0;
        public bool WorkRamEnabled => (_value & 0x10) == 0;
        public bool BiosEnabled => (_value & 0x08) == 0;
        public bool IoChipEnabled => (_value & 0x04) == 0;
    }
}
