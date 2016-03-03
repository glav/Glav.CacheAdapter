using Glav.CacheAdapter.Core;
using Glav.CacheAdapter.Core.Diagnostics;

namespace Glav.CacheAdapter.DependencyInjection
{
    public interface ICacheAdapterResolver
    {
        //ICacheProvider ResolveCacheFromConfig(CacheConfig config, ILogging logger);
        ICacheProvider ResolveCacheFromConfig(CacheConfig config);
        void SetLogger(ILogging logger);

    }
}
