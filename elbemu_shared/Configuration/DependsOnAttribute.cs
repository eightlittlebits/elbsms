using System;

namespace elbemu_shared.Configuration
{
    [AttributeUsage(System.AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class DependsOnAttribute : System.Attribute
    {
        public string PropertyName { get; }

        // This is a positional argument
        public DependsOnAttribute(string propertyName)
        {
            PropertyName = propertyName;
        }
    }
}