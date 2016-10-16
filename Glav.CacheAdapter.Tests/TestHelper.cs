using Glav.CacheAdapter.Core;
using Glav.CacheAdapter.DependencyManagement;
using Glav.CacheAdapter.Helpers;

namespace Glav.CacheAdapter.Tests
{
    public static class TestHelper
    {
        private static readonly CacheConfig _config = new CacheConfig();
        private static ICache _cache;

        public static ICache BuildTestCache()
        {
            if (_cache != null)
            {
                return _cache;
            }

            _cache = CacheConfig.Create()
                .BuildCacheProviderWithTraceLogging()
                .InnerCache;

            return _cache;
        }

        public static ICacheDependencyManager GetDependencyManager()
        {
            var config = CacheConfig.Create();
            return new GenericDependencyManager(config.BuildCacheProviderWithTraceLogging().InnerCache, new Core.Diagnostics.Logger(config));
        }

        public static ICacheProvider GetCacheProvider()
        {
            var provider = CacheConfig.Create()
                .BuildCacheProviderWithTraceLogging();

            return provider;
        }
    }

}
