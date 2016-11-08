using Glav.CacheAdapter.Core;
using Glav.CacheAdapter.Core.Diagnostics;
using Glav.CacheAdapter.DependencyManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Glav.CacheAdapter.Web
{
    public class WebCacheFactory : ICacheConstructionFactory
    {
        private readonly ILogging _logger;
        private readonly CacheConfig _config;

        public WebCacheFactory(ILogging logger, CacheConfig config = null)
        {
            _logger = logger;
            _config = config; //not used but ctor kept consistent with other engines.
        }

        public CacheFactoryComponentResult CreateCacheComponents()
        {
            var cacheEngine = CreateCacheEngine();
            var dependencyMgr = new GenericDependencyManager(cacheEngine, _logger, _config);
            var featureSupport = new WebFeatureSupport();
            var result = CacheFactoryComponentResult.Create(cacheEngine, dependencyMgr, featureSupport, _config);
            return result;
        }

        private ICache CreateCacheEngine()
        {
            var cache = System.Web.HttpContext.Current != null
                ? System.Web.HttpContext.Current.Cache
                : System.Web.HttpRuntime.Cache;

            return new WebCacheAdapter(_logger, cache);
        }
    }
}
