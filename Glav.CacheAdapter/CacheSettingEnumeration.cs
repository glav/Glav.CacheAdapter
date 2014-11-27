using Glav.CacheAdapter.Bootstrap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Glav.CacheAdapter.Core
{
    public enum CacheSetting
    {
        Memory,
        Web,
        AppFabric,
		memcached,
        Redis
    }

    public static class CacheSettingExtensions
    {
        public static string ToStringType(this CacheSetting cacheSetting)
        {
            switch (cacheSetting)
            {
                case CacheSetting.AppFabric:
                    return CacheTypes.AppFabricCache;
                    break;
                case CacheSetting.memcached:
                    return CacheTypes.memcached;
                    break;
                case CacheSetting.Memory:
                    return CacheTypes.MemoryCache;
                    break;
                case CacheSetting.Redis:
                    return CacheTypes.redis;
                    break;
                case CacheSetting.Web:
                    return CacheTypes.WebCache;
                    break;
                default:
                    return CacheTypes.MemoryCache;
                    break;
            }
        }
    }
}
