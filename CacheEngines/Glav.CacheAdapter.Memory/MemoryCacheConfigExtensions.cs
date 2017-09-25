using Glav.CacheAdapter.Bootstrap;

namespace Glav.CacheAdapter.Memory
{
    public static class MemoryCacheConfigExtensions
    {
        public static CacheConfig UseMemoryCache(this CacheConfig config)
        {
            config.CacheToUse = CacheTypes.MemoryCache;
            return config;
        }
    }
}
