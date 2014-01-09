using Glav.CacheAdapter.Bootstrap;
using Glav.CacheAdapter.Core;
using Glav.CacheAdapter.Core.Diagnostics;
using Glav.CacheAdapter.DependencyManagement;
using Glav.CacheAdapter.Distributed.AppFabric;
using Glav.CacheAdapter.Distributed.memcached;
using Glav.CacheAdapter.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Glav.CacheAdapter.DependencyInjection
{
    public class CacheAdapterResolver :ICacheAdapterResolver
    {
        private ILogging _logger;

        public CacheAdapterResolver(ILogging logger)
        {
            _logger = logger;
        }
        public Core.ICacheProvider ResolveCacheFromConfig(CacheConfig config)
        {
            ICacheProvider provider = null;
            var cache = GetCache(config);
            if (config.IsCacheDependencyManagementEnabled)
            {
                var dependencyManager = GetCacheDependencyManager(config, cache);

                provider = new CacheProvider(cache, _logger, dependencyManager);
            }
            else
            {
                provider = new CacheProvider(cache, _logger);
            }
            _logger.WriteInfoMessage(string.Format("CacheProvider initialised with {0} cache engine", config.CacheToUse));
            return provider;
        }

        private ICache GetCache(CacheConfig config)
        {
            ICache cache = null;
            var normalisedCacheToUse = !string.IsNullOrWhiteSpace(config.CacheToUse) ? config.CacheToUse.ToLowerInvariant() : string.Empty;
            switch (normalisedCacheToUse)
            {
                case CacheTypes.MemoryCache:
                    cache = new MemoryCacheAdapter(_logger);
                    break;
                case CacheTypes.WebCache:
                    cache = new WebCacheAdapter(_logger);
                    break;
                case CacheTypes.AppFabricCache:
                    cache = new AppFabricCacheAdapter(_logger);
                    break;
                case CacheTypes.memcached:
                    cache = new memcachedAdapter(_logger,config);
                    break;
                default:
                    cache = new MemoryCacheAdapter(_logger);
                    break;
            }
            return cache;
        }

        private ICacheDependencyManager GetCacheDependencyManager(CacheConfig config,ICache cache)
        {
            ICacheDependencyManager dependencyMgr = null;
            var normalisedDependencyManagerConfig = !string.IsNullOrWhiteSpace(config.DependencyManagerToUse) ? config.DependencyManagerToUse.ToLowerInvariant() : string.Empty;
            switch (normalisedDependencyManagerConfig)
            {
                case CacheDependencyManagerTypes.Default:
                    dependencyMgr = new GenericDependencyManager(cache, _logger);
                    break;
                default:
                    dependencyMgr = new GenericDependencyManager(cache, _logger);
                    break;
            }
            return dependencyMgr;
        }

    }
}
