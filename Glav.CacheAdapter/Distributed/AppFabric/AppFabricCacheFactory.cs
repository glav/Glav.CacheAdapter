using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.ApplicationServer.Caching;
using System.Diagnostics;
using Glav.CacheAdapter.Core.Diagnostics;
using System.Security;

namespace Glav.CacheAdapter.Distributed.AppFabric
{
    public class AppFabricCacheFactory: DistributedCacheFactoryBase
    {
        public AppFabricCacheFactory(ILogging logger) : base(logger)
        {
        }

        public DataCache ConstructCache()
        {
			var config = ParseConfig(AppFabricConstants.DEFAULT_ServerAddress, AppFabricConstants.DEFAULT_Port);
			var dataCacheEndpoints = new List<DataCacheServerEndpoint>();
			config.ServerNodes.ForEach(e => dataCacheEndpoints.Add(new DataCacheServerEndpoint(e.IPAddressOrHostName,e.Port)));

            var factoryConfig = new DataCacheFactoryConfiguration();
			factoryConfig.Servers = dataCacheEndpoints;

            var configMapper = new FactoryConfigConverter(Logger);
            configMapper.MapSettingsFromConfigToAppFabricSettings(config, factoryConfig);
			//SetSecuritySettings(config, factoryConfig);

            try
            {
				Logger.WriteInfoMessage("Constructing AppFabric Cache");

				var factory = new DataCacheFactory(factoryConfig);
                DataCacheClientLogManager.ChangeLogLevel(System.Diagnostics.TraceLevel.Error);

				// Note: When setting up AppFabric. The configured cache needs to be created by the admin using the New-Cache powershell command
            	string cacheName;
				// Prefer the new config mechanism over the explicit entry but still support it. So we
				// try and extract config from the ProviderSpecificValues first.
				if (config.ProviderSpecificValues.ContainsKey(AppFabricConstants.CONFIG_CacheNameKey))
				{
					cacheName = config.ProviderSpecificValues[AppFabricConstants.CONFIG_CacheNameKey];
				} else
				{
					cacheName = MainConfig.Default.DistributedCacheName;
				}

				Logger.WriteInfoMessage(string.Format("Appfabric Cache Name: [{0}]", cacheName));

            	DataCache cache = null;
				if (string.IsNullOrWhiteSpace(cacheName))
				{
					cache = factory.GetDefaultCache();
				}
				else
				{
					cache = factory.GetCache(cacheName);
				}

				Logger.WriteInfoMessage("AppFabric cache constructed.");

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
