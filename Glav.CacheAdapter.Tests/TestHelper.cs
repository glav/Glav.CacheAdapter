using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Glav.CacheAdapter.Bootstrap;
using Glav.CacheAdapter.Core;
using Glav.CacheAdapter.DependencyManagement;
using Glav.CacheAdapter.Distributed.AppFabric;
using Glav.CacheAdapter.Distributed.memcached;
using Glav.CacheAdapter.Web;

namespace Glav.CacheAdapter.Tests
{
    public static class TestHelper
    {
        private static CacheConfig _config = new CacheConfig();

        public static ICache GetCacheFromConfig()
        {
            switch (_config.CacheToUse)
            {
                case CacheTypes.MemoryCache:
                    return new MemoryCacheAdapter(new MockLogger());
                    break;
                case CacheTypes.memcached:
                    return new memcachedAdapter(new MockLogger());
                    break;
                case CacheTypes.WebCache:
                    return new WebCacheAdapter(new MockLogger());
                    break;
                case CacheTypes.AppFabricCache:
                    return new AppFabricCacheAdapter(new MockLogger());
                    break;
                default:
                    return new MemoryCacheAdapter(new MockLogger());
            }
        }

        public static ICacheDependencyManager GetDependencyManager()
        {
            return new GenericDependencyManager(GetCacheFromConfig(), new MockLogger());
        }

        public static ICacheProvider GetCacheProvider()
        {
            ICacheProvider provider = new CacheProvider(GetCacheFromConfig(), new MockLogger(), GetDependencyManager());
            return provider;
        }
    }

}
