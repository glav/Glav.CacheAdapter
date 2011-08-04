using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.ApplicationServer.Caching;
using System.Diagnostics;
using Glav.CacheAdapter.Core.Diagnostics;

namespace Glav.CacheAdapter.Distributed.AppFabric
{
    public class AppFabricCacheFactory: DistributedCacheFactoryBase
    {
        private const string DEFAULT_ServerAddress = "localhost";
    	public const string CONFIG_CacheNameKey = "DistributedCacheName";

		private const int DEFAULT_Port = 22233;

        public AppFabricCacheFactory(ILogging logger) : base(logger)
        {
        }

        public DataCache ConstructCache()
        {
            var config = ParseConfig(DEFAULT_ServerAddress,DEFAULT_Port);
			var dataCacheEndpoints = new List<DataCacheServerEndpoint>();
			config.ServerNodes.ForEach(e => dataCacheEndpoints.Add(new DataCacheServerEndpoint(e.IPAddressOrHostName,e.Port)));

            var factoryConfig = new DataCacheFactoryConfiguration();

			factoryConfig.Servers = dataCacheEndpoints;

            try
            {
				var factory = new DataCacheFactory(factoryConfig);
                DataCacheClientLogManager.ChangeLogLevel(System.Diagnostics.TraceLevel.Error);

				// Note: When setting up AppFabric. The configured cache needs to be created by the admin using the New-Cache powershell command
            	string cacheName;
				// Prefer the new config mechanismover the explicit entry but still support it
				if (config.ProviderSpecificValues.ContainsKey(CONFIG_CacheNameKey))
				{
					cacheName = config.ProviderSpecificValues[CONFIG_CacheNameKey];
				} else
				{
					cacheName = MainConfig.Default.DistributedCacheName;
				}
				var cache = factory.GetCache(cacheName);
                return cache;
            }
            catch (Exception ex)
            {
                Logger.WriteException(ex);
                throw;
            }
            
        }

    }
}
