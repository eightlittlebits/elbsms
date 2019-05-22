using elbsms_core.CPU;

namespace elbsms_core
{
    public class MasterSystem
    {
        internal SystemClock Clock;
        internal Bus Bus;

        internal Z80 CPU;

        public MasterSystem()
        {
            Clock = new SystemClock();
            Bus = new Bus();

            CPU = new Z80(Clock, Bus);
        }

        internal void LoadGameMedia(GameMedia media)
        {
            Bus.LoadGameMedia(media);
        }

        public void SingleStep()
        {
            CPU.ExecuteInstruction();
        }
    }
}
