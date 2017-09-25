using Glav.CacheAdapter.Bootstrap;

namespace Glav.CacheAdapter.Memcached
{
    public static class MemcachedCacheConfigExtensions
    {
        public static CacheConfig UseMemcachedCache(this CacheConfig config)
        {
            config.CacheToUse = CacheTypes.memcached;
            return config;
        }

    }
}
