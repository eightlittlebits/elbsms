using System.Text;

namespace elbsms_core.CPU
{
    struct StatusFlags
    {
        public const int S = 0b1000_0000;
        public const int Z = 0b0100_0000;
        public const int B5 = 0b0010_0000;
        public const int H = 0b0001_0000;
        public const int B3 = 0b0000_1000;
        public const int P = 0b0000_0100;
        public const int V = 0b0000_0100;
        public const int N = 0b0000_0010;
        public const int C = 0b0000_0001;
        
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
            sb.Append(this[S] ? '5' : '-');
            sb.Append(this[H] ? 'H' : '-');
            sb.Append(this[S] ? '3' : '-');
            sb.Append(this[P] ? 'P' : '-');
            sb.Append(this[N] ? 'N' : '-');
            sb.Append(this[C] ? 'C' : '-');

            return sb.ToString();
        }
    }
}