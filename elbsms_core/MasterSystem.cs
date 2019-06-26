using elbsms_core.CPU;
using elbsms_core.Memory;
using elbsms_core.Video;

namespace elbsms_core
{
    public class MasterSystem
    {
        // the master clock in the master system runs at 3 times the cpu clock
        private const uint ClockMultiplier = 3;

        internal SystemClock Clock;
        internal InterruptController InterruptController;
        internal VideoDisplayProcessor VDP;
        internal Interconnect Interconnect;
        internal Z80 CPU;

        public MasterSystem()
        {
            Clock = new SystemClock(ClockMultiplier);
            InterruptController = new InterruptController();
            VDP = new VideoDisplayProcessor(Clock, InterruptController, VideoStandard.NTSC);
            Interconnect = new Interconnect(VDP);
            CPU = new Z80(Clock, Interconnect, InterruptController);
        }

        public void LoadRom(byte[] romData)
        {
            Interconnect.LoadCartridge(new Cartridge(romData));
        }

        public void LoadCartridge(Cartridge cartridge)
        {
            Interconnect.LoadCartridge(cartridge);
        }

        public void SingleStep()
        {
            CPU.ExecuteInstruction();

            VDP.SynchroniseWithSystemClock();
        }
    }
}
