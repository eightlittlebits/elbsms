using elbemu_shared.Configuration;

namespace elbemu_shared
{
    public interface IEmulatedSystem
    {
        string SystemName { get; }

        ISystemConfiguration Configuration { get; set; }

        void Shutdown();
    }
}
