using elbemu_shared;
using elbemu_shared.Configuration;
using elbsms_core.CPU;

namespace elbsms_core
{
    public class MasterSystem : IEmulatedSystem
    {
        public string SystemName => "Sega Master System";

        private MasterSystemConfiguration _config;
        public ISystemConfiguration Configuration { get => _config; set => _config = (MasterSystemConfiguration)value; }

        internal SystemClock Clock;
        internal Bus Bus;
        internal Z80 CPU;       

        public MasterSystem(Cartridge cartridge)
        {
            _config = MasterSystemConfiguration.Load();

            Clock = new SystemClock();
            Bus = new Bus(cartridge);

            CPU = new Z80(Clock, Bus);
        }

        public void SingleStep()
        {
            CPU.ExecuteInstruction();
        }

        public void Shutdown()
        {
            _config.Save();
        }
    }
}
