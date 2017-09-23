using Glav.CacheAdapter.Bootstrap;
using System.Collections.Generic;

namespace Glav.CacheAdapter.Core
{

    public static class CacheSettingExtensions
    {
        private static readonly Dictionary<CacheSetting, string> _cacheAssemblies = new Dictionary<CacheSetting, string>();
        private static readonly Dictionary<CacheSetting, string> _cacheFactoryTypes = new Dictionary<CacheSetting, string>();

        static CacheSettingExtensions()
        {
            PopulateCacheAssemblies();
            PopulateCacheFactoryTypes();
        }

        private static void PopulateCacheFactoryTypes()
        {
            _cacheFactoryTypes.Add(CacheSetting.AppFabric, "Glav.CacheAdapter.AppFabric.AppFabricCacheFactory");
            _cacheFactoryTypes.Add(CacheSetting.Hybrid, null);
            _cacheFactoryTypes.Add(CacheSetting.memcached, "Glav.CacheAdapter.Memcached.MemcachedCacheFactory");
            _cacheFactoryTypes.Add(CacheSetting.Memory, null);
            _cacheFactoryTypes.Add(CacheSetting.Redis, "Glav.CacheAdapter.Redis.RedisCacheFactory");
            _cacheFactoryTypes.Add(CacheSetting.Web, "Glav.CacheAdapter.SystemWeb.WebCacheFactory");
        }

        private static void PopulateCacheAssemblies()
        {
            _cacheAssemblies.Add(CacheSetting.AppFabric, "Glav.CacheAdapter.AppFabric.dll");
            _cacheAssemblies.Add(CacheSetting.Hybrid, null);
            _cacheAssemblies.Add(CacheSetting.memcached, "Glav.CacheAdapter.Memcached.dll");
            _cacheAssemblies.Add(CacheSetting.Memory, null);
            _cacheAssemblies.Add(CacheSetting.Redis, "Glav.CacheAdapter.redis.dll");
            _cacheAssemblies.Add(CacheSetting.Web, "Glav.CacheAdapter.SystemWeb.dll");
        }

        public static string ToStringType(this CacheSetting cacheSetting)
        {
            switch (cacheSetting)
            {
                case CacheSetting.AppFabric:
                    return CacheTypes.AppFabricCache;
                case CacheSetting.memcached:
                    return CacheTypes.memcached;
                case CacheSetting.Memory:
                    return CacheTypes.MemoryCache;
                case CacheSetting.Redis:
                    return CacheTypes.redis;
                case CacheSetting.Web:
                    return CacheTypes.WebCache;
                case CacheSetting.Hybrid:
                    return CacheTypes.hybrid;
                default:
                    return CacheTypes.MemoryCache;
            }
        }

        public static CacheSetting ToCacheSetting(this string cacheSettingValue)
        {
            switch (cacheSettingValue.ToLowerInvariant())
            {
                case CacheTypes.AppFabricCache:
                    return CacheSetting.AppFabric;
                case CacheTypes.memcached:
                    return CacheSetting.memcached;
                case CacheTypes.MemoryCache:
                    return CacheSetting.Memory;
                case CacheTypes.redis:
                    return CacheSetting.Redis;
                case CacheTypes.WebCache:
                    return CacheSetting.Web;
                case CacheTypes.hybrid:
                    return CacheSetting.Hybrid;
                default:
                    return CacheSetting.Memory;
            }
        }

        public static string GetCacheFactoryAssemblyName(this CacheSetting cacheSetting)
        {
            return _cacheAssemblies[cacheSetting];
        }
        public static string GetCacheFactoryTypeName(this CacheSetting cacheSetting)
        {
            return _cacheFactoryTypes[cacheSetting];
        }

    }
}
