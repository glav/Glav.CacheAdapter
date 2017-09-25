using Glav.CacheAdapter.Bootstrap;

namespace Glav.CacheAdapter.Redis
{
    public static class RedisCacheConfigExtensions
    {
        public static CacheConfig UseRedisCache(this CacheConfig config)
        {
            config.CacheToUse = CacheTypes.redis;
            return config;
        }

    }
}
