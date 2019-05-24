using elb_utilities.Configuration;

namespace elbemu_shared.Configuration
{
    public abstract class SystemConfiguration<T> : XmlConfiguration<T>, ISystemConfiguration where T : SystemConfiguration<T>, new()
    {
        public ISystemConfiguration Clone() => (ISystemConfiguration)MemberwiseClone();
    }
}
