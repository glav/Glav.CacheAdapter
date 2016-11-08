using Glav.CacheAdapter.Core.Diagnostics;
using Glav.CacheAdapter.DependencyManagement;
using System.Runtime.Caching;

namespace Glav.CacheAdapter.Core
{
    public class MemoryCacheFactory : ICacheConstructionFactory
    {
        private readonly ILogging _logger;
        private readonly CacheConfig _config;
        public MemoryCacheFactory(ILogging logger, CacheConfig config)
        {
            _logger = logger;
            _config = config;
        }
        private ICache CreateCacheEngine()
        {
            return new MemoryCacheAdapter(_logger, MemoryCache.Default);
        }

        public CacheFactoryComponentResult CreateCacheComponents()
        {
            var cacheEngine = CreateCacheEngine();
            var dependencyMgr = new GenericDependencyManager(cacheEngine, _logger, _config);
            var featureSupport = new MemoryFeatureSupport();
            var result = CacheFactoryComponentResult.Create(cacheEngine, dependencyMgr, featureSupport,_config);
            return result;
        }

    }
}
