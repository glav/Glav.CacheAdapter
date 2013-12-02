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
	    private static CacheConfig _config ;
        private static ILogging _logger;

        public static CacheConfig  Configuration
        {
            get { return _config; }
        }
        public static ILogging Logger
        {
            get { return _logger; }
        }
        public static ICacheProvider ResolveCacheFromConfig(ILogging logger, CacheConfig config)
        {
            if (config != null)
            {
                _config = config;
            }
            if (logger != null)
            {
                _logger = logger;
            }
            EnsureLoggingAndCacheAreValidObjects();
            return CreateCacheFromConfig();
        }

        [Obsolete("Use 'ResolveCacheFromConfig(ILogging logger, CacheConfig config) overload")]
        public static ICacheProvider ResolveCacheFromConfig(ILogging logger, string cacheConfigEntry = null, string dependencyManagerConfigEntry = null)
		{
            if (logger != null)
            {
                _logger = logger;
            }
            EnsureLoggingAndCacheAreValidObjects();

            if (!string.IsNullOrWhiteSpace(cacheConfigEntry))
            {
                _config.CacheToUse = cacheConfigEntry;
            }
            if (!string.IsNullOrWhiteSpace(dependencyManagerConfigEntry))
            {
                _config.DependencyManagerToUse = dependencyManagerConfigEntry;
            }

            return CreateCacheFromConfig();
		}

        private static ICacheProvider CreateCacheFromConfig()
        {
            ICacheProvider provider = null;
            var cache = GetCache(_config.CacheToUse, _logger);
            if (_config.IsCacheDependencyManagementEnabled)
            {
                var dependencyManager = GetCacheDependencyManager(_config.DependencyManagerToUse, cache, _logger);

                provider = new CacheProvider(cache, _logger, dependencyManager);
            }
            else
            {
                provider = new CacheProvider(cache, _logger);
            }
            _logger.WriteInfoMessage(string.Format("CacheProvider initialised with {0} cache engine", _config.CacheToUse));
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

        private static void EnsureLoggingAndCacheAreValidObjects()
        {
            if (_logger == null)
            {
                _logger = new Logger();
            }
            if (_config == null)
            {
                _config = new CacheConfig();
            }
        }
    }
}
