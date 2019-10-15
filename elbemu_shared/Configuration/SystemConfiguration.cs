using elb_utilities.Configuration;

namespace elbemu_shared.Configuration
{
    public abstract class SystemConfiguration<T> : XmlConfiguration<T> where T : SystemConfiguration<T>, new()
    {
        public T Clone() => (T)MemberwiseClone();
    }
}
