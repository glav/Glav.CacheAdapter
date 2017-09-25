using Glav.CacheAdapter.Bootstrap;

namespace Glav.CacheAdapter.Redis
{
    public static class AppFabricCacheConfigExtensions
    {
        public static CacheConfig UseAppFabricCache(this CacheConfig config)
        {
            config.CacheToUse = CacheTypes.AppFabricCache;
            return config;
        }
    }
}
