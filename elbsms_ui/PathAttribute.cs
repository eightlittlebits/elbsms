using System;

namespace elbsms_ui
{
    enum PathType
    {
        File,
        Folder,
    }

    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    sealed class PathAttribute : Attribute
    {
        public PathType PathType { get; }

        public PathAttribute(PathType pathType)
        {
            PathType = pathType;
        }
    }
}
