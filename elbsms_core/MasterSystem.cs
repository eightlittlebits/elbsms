using elbsms_core.CPU;
using elbsms_core.Memory;
using elbsms_core.Video;

namespace elbsms_core
{
    public class MasterSystem
    {
        internal SystemClock Clock;
        internal VideoDisplayProcessor VDP;
        internal Interconnect Interconnect;
        internal Z80 CPU;

        public MasterSystem()
        {
            Clock = new SystemClock();
            VDP = new VideoDisplayProcessor(Clock);
            Interconnect = new Interconnect(VDP);
            CPU = new Z80(Clock, Interconnect);
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
