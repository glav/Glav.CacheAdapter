using Glav.CacheAdapter.Bootstrap;

namespace Glav.CacheAdapter.SystemWeb
{
    public static class SystemWebCacheConfigExtensions
    {
        public static CacheConfig UseWebCache(this CacheConfig config)
        {
            config.CacheToUse = CacheTypes.WebCache;
            return config;
        }
    }
}
