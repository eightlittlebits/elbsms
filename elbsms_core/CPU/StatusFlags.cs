using System.Text;

namespace elbsms_core.CPU
{
    struct StatusFlags
    {
        public const byte S = 0b1000_0000;
        public const byte Z = 0b0100_0000;
        public const byte B5 = 0b0010_0000;
        public const byte H = 0b0001_0000;
        public const byte B3 = 0b0000_1000;
        public const byte P = 0b0000_0100;
        public const byte V = 0b0000_0100;
        public const byte N = 0b0000_0010;
        public const byte C = 0b0000_0001;

        private byte _flags;

        public StatusFlags(int i)
        {
            _flags = (byte)i;
        }

        public bool this[int bit]
        {
            get => (_flags & bit) == bit;
            set
            {
                if (value)
                {
                    _flags |= (byte)bit;
                }
                else
                {
                    _flags &= (byte)~bit;
                }
            }
        }

        public static implicit operator byte(StatusFlags b) => b._flags;

        public static implicit operator StatusFlags(int i) => new StatusFlags(i);

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(8);

            sb.Append(this[S] ? 'S' : '-');
            sb.Append(this[Z] ? 'Z' : '-');
            sb.Append(this[B5] ? '5' : '-');
            sb.Append(this[H] ? 'H' : '-');
            sb.Append(this[B3] ? '3' : '-');
            sb.Append(this[P] ? 'P' : '-');
            sb.Append(this[N] ? 'N' : '-');
            sb.Append(this[C] ? 'C' : '-');

            return sb.ToString();
        }
    }
}