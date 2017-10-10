using elbsms_core.CPU;

namespace elbsms_core
{
    public class MasterSystem
    {
        internal SystemClock Clock;
        internal Bus Bus;

        internal Z80 CPU; 

        public MasterSystem(Cartridge cartridge)
        {
            Clock = new SystemClock();
            Bus = new Bus(cartridge);

            CPU = new Z80(Clock, Bus);
        }

        public void SingleStep()
        {
            CPU.ExecuteInstruction();
        }
    }
}
