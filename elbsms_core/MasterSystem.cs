using elbsms_core.CPU;
using elbsms_core.Memory;

namespace elbsms_core
{
    public class MasterSystem
    {
        internal SystemClock Clock;
        internal Interconnect Interconnect;

        internal Z80 CPU;

        public MasterSystem()
        {
            Clock = new SystemClock();
            Interconnect = new Interconnect();

            CPU = new Z80(Clock, Interconnect);
        }

        public void LoadRom(byte[] romData)
        {
            Interconnect.LoadCartridge(new Cartridge(romData));
        }

        public void SingleStep()
        {
            CPU.ExecuteInstruction();
        }
    }
}
