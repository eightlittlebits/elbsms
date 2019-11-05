using System;

namespace elb_utilities.Addins
{
    internal class AppDomainWithType<T> : IDisposable where T : MarshalByRefObject, new()
    {
        private bool _disposed = false;
        private readonly AppDomain _appDomain;

        public T TypeObject { get; }

        public AppDomainWithType()
            : this(typeof(T).Name.ToLower() + "-domain")
        {

        }

        public AppDomainWithType(string friendlyName)
        {
            _appDomain = AppDomain.CreateDomain(friendlyName);

            TypeObject = (T)_appDomain.CreateInstanceAndUnwrap(typeof(T).Assembly.FullName, typeof(T).FullName);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    AppDomain.Unload(_appDomain);
                }

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}