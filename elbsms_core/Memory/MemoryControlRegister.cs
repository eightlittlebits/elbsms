namespace elbsms_core.Memory
{
    internal class MemoryControlRegister
    {
        private byte _value;

        public bool ExpansionSlotEnabled;
        public bool CartridgeSlotEnabled;
        public bool CardSlotEnabled;
        public bool WorkRamEnabled;
        public bool BiosEnabled;
        public bool IoChipEnabled;

        public byte Value
        {
            set
            {
                _value = value;

                ExpansionSlotEnabled = (_value & 0x80) == 0;
                CartridgeSlotEnabled = (_value & 0x40) == 0;
                CardSlotEnabled = (_value & 0x20) == 0;
                WorkRamEnabled = (_value & 0x10) == 0;
                BiosEnabled = (_value & 0x08) == 0;
                IoChipEnabled = (_value & 0x04) == 0;
            }
        }
    }
}
