using elbsms_core.CPU;

namespace elbsms_core
{
    public class MasterSystem
    {
        internal SystemClock Clock;
        internal Interconnect Interconnect;

        internal Z80 CPU;

        public MasterSystem(Cartridge cartridge)
        {
            Clock = new SystemClock();
            Interconnect = new Interconnect(cartridge);

            CPU = new Z80(Clock, Interconnect);
        }

        public void SingleStep()
        {
            CPU.ExecuteInstruction();
        }
    }
}
