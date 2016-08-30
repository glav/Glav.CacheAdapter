using Glav.CacheAdapter.Bootstrap;
using Glav.CacheAdapter.Core;
using Glav.CacheAdapter.DependencyManagement;
using Glav.CacheAdapter.Distributed.AppFabric;
using Glav.CacheAdapter.Distributed.memcached;
using Glav.CacheAdapter.Web;
using Glav.CacheAdapter.Helpers;

namespace Glav.CacheAdapter.Tests
{
    public static class TestHelper
    {
        private static readonly CacheConfig _config = new CacheConfig();
        private static ICache _cache;

        public static ICache GetCacheFromConfig()
        {
            if (_cache != null)
            {
                return _cache;
            }

            _cache = CacheConfig.Create()
                .BuildCacheProvider(new MockLogger())
                .InnerCache;

        //    switch (_config.CacheToUse)
        //    {
        //        case CacheTypes.MemoryCache:
        //            _cache = new MemoryCacheFactory(new MockLogger(),_config).CreateCacheComponents().Cache;
        //            break;
        //        case CacheTypes.memcached:
        //            _cache = new memcachedCacheFactory(new MockLogger(), _config).CreateCacheComponents().Cache;
        //            break;
        //        case CacheTypes.WebCache:
        //            _cache = new WebCacheFactory(new MockLogger(),_config).CreateCacheComponents().Cache;
        //            break;
        //        case CacheTypes.AppFabricCache:
        //            _cache = new AppFabricCacheFactory(new MockLogger(),_config).CreateCacheComponents().Cache;
        //            break;
        //        default:
        //            _cache = new MemoryCacheFactory(new MockLogger(),_config).CreateCacheComponents().Cache;
        //            break;
        //    }

            return _cache;
        }

        public static ICacheDependencyManager GetDependencyManager()
        {
            return new GenericDependencyManager(GetCacheFromConfig(), new MockLogger());
        }

        public static ICacheProvider GetCacheProvider()
        {
            var provider = CacheConfig.Create()
                .BuildCacheProvider(new MockLogger());

            //ICacheProvider provider = new CacheProvider(GetCacheFromConfig(), new MockLogger(), GetDependencyManager());
            return provider;
        }
    }

}
