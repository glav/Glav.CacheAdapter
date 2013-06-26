using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Glav.CacheAdapter.DependencyManagement;
using Glav.CacheAdapter.Web;
using Glav.CacheAdapter.Core.Diagnostics;
using Glav.CacheAdapter.Distributed.AppFabric;
using Glav.CacheAdapter.Bootstrap;
using Glav.CacheAdapter.Distributed.memcached;

namespace Glav.CacheAdapter.Core.DependencyInjection
{
	public static class CacheBinder
	{
	    private static CacheConfig _config = new CacheConfig();

        public static ICacheProvider ResolveCacheFromConfig(ILogging logger, string cacheConfigEntry=null, string dependencyManagerConfigEntry=null)
		{
			if (logger == null)
			{
				logger = new Logger();
			}

            if (string.IsNullOrWhiteSpace(cacheConfigEntry))
            {
                cacheConfigEntry = _config.CacheToUse;
            }
            if (string.IsNullOrWhiteSpace(dependencyManagerConfigEntry))
            {
                dependencyManagerConfigEntry = _config.DependencyManagerToUse;
            }

            ICacheProvider provider = null;
            var cache = GetCache(cacheConfigEntry, logger);
            if (_config.IsCacheKeysDependenciesEnabled || _config.IsCacheGroupDependenciesEnabled)
            {
                var dependencyManager = GetCacheDependencyManager(dependencyManagerConfigEntry, cache, logger);

                provider = new CacheProvider(cache, logger, dependencyManager);
            } else
            {
                provider = new CacheProvider(cache, logger);
            }
            logger.WriteInfoMessage(string.Format("CacheProvider initialised with {0} cache engine",cacheConfigEntry));

			return provider;
		}

        private static ICache GetCache(string cacheConfigEntry, ILogging logger)
        {
            ICache cache = null;
            switch (cacheConfigEntry)
            {
                case CacheTypes.MemoryCache:
                    cache = new MemoryCacheAdapter(logger);
                    break;
                case CacheTypes.WebCache:
                    cache = new WebCacheAdapter(logger);
                    break;
                case CacheTypes.AppFabricCache:
                    cache = new AppFabricCacheAdapter(logger);
                    break;
                case CacheTypes.memcached:
                    cache = new memcachedAdapter(logger);
                    break;
                default:
                    cache = new MemoryCacheAdapter(logger);
                    break;
            }
            return cache;
        }
        private static ICacheDependencyManager GetCacheDependencyManager(string dependencyManagerConfigEntry, ICache cache, ILogging logger)
        {
            ICacheDependencyManager dependencyMgr = null;
            switch (dependencyManagerConfigEntry)
            {
                case CacheDependencyManagerTypes.Default:
                    dependencyMgr = new GenericDependencyManager(cache,logger);
                    break;
                default:
                    dependencyMgr = new GenericDependencyManager(cache, logger);
                    break;
            }
            return dependencyMgr;
        }
    }
}
