using Glav.CacheAdapter.Bootstrap;
using Glav.CacheAdapter.Core;
using Glav.CacheAdapter.Core.Diagnostics;
using Glav.CacheAdapter.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Glav.CacheAdapter.Helpers
{
    public static class CacheConfigExtensions
    {
        public static CacheConfig UseMemoryCache(this CacheConfig config)
        {
            config.CacheToUse = CacheTypes.MemoryCache;
            return config;
        }
        public static CacheConfig UseWebCache(this CacheConfig config)
        {
            config.CacheToUse = CacheTypes.WebCache;
            return config;
        }
        public static CacheConfig UseMemcachedCache(this CacheConfig config)
        {
            config.CacheToUse = CacheTypes.memcached;
            return config;
        }
        public static CacheConfig UseAppFabricCache(this CacheConfig config)
        {
            config.CacheToUse = CacheTypes.AppFabricCache;
            return config;
        }
        public static CacheConfig UseRedisCache(this CacheConfig config)
        {
            config.CacheToUse = CacheTypes.redis;
            return config;
        }
        public static CacheConfig UseHybridCache(this CacheConfig config)
        {
            config.CacheToUse = CacheTypes.hybrid;
            return config;
        }
        public static CacheConfig WithCacheSpecificSettings(this CacheConfig config, string settings)
        {
            config.CacheSpecificData = settings;
            return config;
        }
        public static CacheConfig UsingDistributedServerNode(this CacheConfig config, string address)
        {
            AddServerNodeIfNotPresent(config, address);
            return config;
        }
        public static CacheConfig UsingDistributedServerNode(this CacheConfig config, string ipAddress, int port)
        {
            AddServerNodeIfNotPresent(config, string.Format("{0}:{1}",ipAddress, port));
            return config;
        }
        public static CacheConfig UsingDistributedServerNodes(this CacheConfig config, IEnumerable<string> nodeAddresses)
        {
            nodeAddresses.ToList().ForEach(n =>
            {
                AddServerNodeIfNotPresent(config, n);
            });
            return config;
        }

        public static CacheFactoryComponentResult BuildComponentsWithTraceLogging(this CacheConfig config)
        {
            var resolver = new CacheAdapterResolver(new Logger(config));
            var factory = resolver.GetCacheConstructionFactoryUsingConfig(config);
            return factory.CreateCacheComponents();
        }

        public static CacheFactoryComponentResult BuildComponents(this CacheConfig config, ILogging logger)
        {
            var resolver = new CacheAdapterResolver(logger);
            var factory = resolver.GetCacheConstructionFactoryUsingConfig(config);
            return factory.CreateCacheComponents();
        }

        public static ICacheProvider BuildCacheProviderWithTraceLogging(this CacheFactoryComponentResult cacheComponents)
        {
            return new CacheProvider(cacheComponents.Cache, new Logger(cacheComponents.ConfigUsed), cacheComponents.ConfigUsed, cacheComponents.DependencyManager, cacheComponents.FeatureSupport);
        }

        public static ICacheProvider BuildCacheProvider(this CacheFactoryComponentResult cacheComponents, ILogging logger)
        {
            return new CacheProvider(cacheComponents.Cache, logger, cacheComponents.ConfigUsed, cacheComponents.DependencyManager, cacheComponents.FeatureSupport);
        }

        public static ICacheProvider BuildCacheProviderWithTraceLogging(this CacheConfig config)
        {
            var logger = new Logger(config);
            var components = BuildComponents(config, logger);

            return new CacheProvider(components.Cache, logger, config, components.DependencyManager, components.FeatureSupport);
        }

        public static ICacheProvider BuildCacheProvider(this CacheConfig config, ILogging logger)
        {
            var components = BuildComponents(config, logger);

            return new CacheProvider(components.Cache, logger, config,components.DependencyManager, components.FeatureSupport);
        }

        private static void AddServerNodeIfNotPresent(CacheConfig config, string address)
        {
            if (string.IsNullOrWhiteSpace(config.DistributedCacheServers))
            {
                config.DistributedCacheServers = address;
                return;
            }
            if (!config.DistributedCacheServers.Contains(address))
            {
                config.DistributedCacheServers  += ";" + address;
                return;
            }
        }



    }
}
