using System;

namespace elbemu_shared.Configuration
{
    public enum PathType
    {
        File,
        Folder,
    }

    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class BrowseAttribute : Attribute
    {
        public PathType PathType { get; }

        public BrowseAttribute(PathType pathType)
        {
            PathType = pathType;
        }
    }
}
