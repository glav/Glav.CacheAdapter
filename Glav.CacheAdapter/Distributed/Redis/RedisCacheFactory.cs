using Glav.CacheAdapter.Bootstrap;
using Glav.CacheAdapter.Core;
using Glav.CacheAdapter.Core.Diagnostics;
using Glav.CacheAdapter.DependencyManagement;
using StackExchange.Redis;
using System;

namespace Glav.CacheAdapter.Distributed.Redis
{
    public class RedisCacheFactory : CacheConstructionFactoryBase
    {
        private const string DEFAULT_IpAddress = "127.0.0.1";
        private const int DEFAULT_Port = 6379;

        private ConnectionMultiplexer _redisConnection;

        public RedisCacheFactory(ILogging logger, CacheConfig config = null)
            : base(logger, config)
        {
            ParseConfig(DEFAULT_IpAddress, DEFAULT_Port);
        }

        public override CacheFactoryComponentResult CreateCacheComponents()
        {
            var cacheEngine = CreateCacheEngine();

            ICacheDependencyManager dependencyMgr = null;
            if (CacheConfiguration.DependencyManagerToUse == CacheDependencyManagerTypes.Generic)
            {
                dependencyMgr = new GenericDependencyManager(cacheEngine, Logger, CacheConfiguration);
            }
            else
            {
                dependencyMgr = new RedisDependencyManager(cacheEngine, Logger, _redisConnection.GetDatabase(), CacheConfiguration);
            }
            var featureSupport = new RedisFeatureSupport();
            var result = CacheFactoryComponentResult.Create(cacheEngine, dependencyMgr, featureSupport, CacheConfiguration);
            return result;
        }

        private ICache CreateCacheEngine()
        {
            var connectionOptions = ConstructConnectionOptions();

            try
            {
                _redisConnection = ConnectionMultiplexer.Connect(connectionOptions);
            }
            catch (Exception ex)
            {
                Logger.WriteException(ex);
                throw;
            }

            return new RedisCacheAdapter(Logger,_redisConnection,CacheConfiguration.IsCacheDependencyManagementEnabled);
        }

        private ConfigurationOptions ConstructConnectionOptions()
        {
            var redisOptions = new ConfigurationOptions();
            if (!string.IsNullOrWhiteSpace(CacheConfiguration.CacheSpecificData))
            {
                // Note: The redis config parser requires values be separated by a comma, not a semi-colon which is what CacheAdapter likes
                var redisSpecificOptions = CacheConfiguration.CacheSpecificData.Replace(';', ',');
                redisOptions = ConfigurationOptions.Parse(redisSpecificOptions, true);
            }

            // Clear the endpoints if any specified here as we use the ones defined in DistributedCacheServers setting to keep
            // config consistent and it means that users can switch to different cache providers without issues
            redisOptions.EndPoints.Clear();
            CacheConfiguration.ServerNodes.ForEach(n =>
            {
                redisOptions.EndPoints.Add(n.IPAddressOrHostName, n.Port);
            });
            return redisOptions;
        }
    }
}
