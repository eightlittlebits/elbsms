using System;

namespace elbemu_shared.Configuration
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class DependsOnAttribute : Attribute
    {
        public string PropertyName { get; }

        public DependsOnAttribute(string propertyName)
        {
            PropertyName = propertyName;
        }
    }
}