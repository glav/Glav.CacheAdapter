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
        private static ICache _cache = null;

        public static ICache GetCacheFromConfig()
        {
            if (_cache != null)
            {
                return _cache;
            }
            switch (_config.CacheToUse)
            {
                case CacheTypes.MemoryCache:
                    _cache = new MemoryCacheAdapter(new MockLogger());
                    break;
                case CacheTypes.memcached:
                    _cache = new memcachedAdapter(new MockLogger());
                    break;
                case CacheTypes.WebCache:
                    _cache = new WebCacheAdapter(new MockLogger());
                    break;
                case CacheTypes.AppFabricCache:
                    _cache = new AppFabricCacheAdapter(new MockLogger());
                    break;
                default:
                    _cache = new MemoryCacheAdapter(new MockLogger());
                    break;
            }

            return _cache;
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
