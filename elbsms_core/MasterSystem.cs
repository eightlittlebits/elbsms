using System;
using elbemu_shared;
using elbemu_shared.Configuration;
using elbsms_core.CPU;
using elbsms_core.Memory;

namespace elbsms_core
{
    public class MasterSystem
    {
        internal static MasterSystemConfiguration Configuration;

        internal SystemClock Clock;
        internal Interconnect Interconnect;

        internal Z80 CPU;

        public MasterSystem()
        {
            Clock = new SystemClock();
            Interconnect = new Interconnect();

            CPU = new Z80(Clock, Interconnect);
        }

        internal void LoadGameMedia(GameMedia media)
        {
            Interconnect.LoadGameMedia(media);
        }

        public void Initialise()
        {
            Configuration = MasterSystemConfiguration.Load();
        }

        public void SingleStep()
        {
            CPU.ExecuteInstruction();
        }

        public void Shutdown()
        {
            Configuration.Save();
        }
    }
}
