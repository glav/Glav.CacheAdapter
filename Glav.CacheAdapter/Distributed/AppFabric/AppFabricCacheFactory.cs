using System;
using System.Collections.Generic;
using Microsoft.ApplicationServer.Caching;
using Glav.CacheAdapter.Core.Diagnostics;

namespace Glav.CacheAdapter.Distributed.AppFabric
{
    public class AppFabricCacheFactory : DistributedCacheFactoryBase
    {
        public AppFabricCacheFactory(ILogging logger, CacheConfig config = null)
            : base(logger, config)
        {
        }

        public bool IsLocalCacheEnabled { get; set; }

        public DataCache ConstructCache()
        {
            ParseConfig(AppFabricConstants.DEFAULT_ServerAddress, AppFabricConstants.DEFAULT_Port);
            var dataCacheEndpoints = new List<DataCacheServerEndpoint>();
            CacheConfiguration.ServerNodes.ForEach(e => dataCacheEndpoints.Add(new DataCacheServerEndpoint(e.IPAddressOrHostName, e.Port)));

            var factoryConfig = new DataCacheFactoryConfiguration
            {
                Servers = dataCacheEndpoints
            };

            var configMapper = new FactoryConfigConverter(Logger);
            configMapper.MapSettingsFromConfigToAppFabricSettings(CacheConfiguration, factoryConfig);
            IsLocalCacheEnabled = configMapper.IsLocalCacheEnabled;
            //SetSecuritySettings(config, factoryConfig);

            try
            {
                Logger.WriteInfoMessage("Constructing AppFabric Cache");

                var factory = new DataCacheFactory(factoryConfig);
                DataCacheClientLogManager.ChangeLogLevel(System.Diagnostics.TraceLevel.Error);

                // Note: When setting up AppFabric. The configured cache needs to be created by the admin using the New-Cache powershell command
                // Prefer the new config mechanism over the explicit entry but still support it. So we
                // try and extract config from the ProviderSpecificValues first.
                var cacheName = CacheConfiguration.ProviderSpecificValues.ContainsKey(AppFabricConstants.CONFIG_CacheNameKey)
                    ? CacheConfiguration.ProviderSpecificValues[AppFabricConstants.CONFIG_CacheNameKey]
                    : MainConfig.Default.DistributedCacheName;

                Logger.WriteInfoMessage(string.Format("Appfabric Cache Name: [{0}]", cacheName));

                var cache = string.IsNullOrWhiteSpace(cacheName) ? factory.GetDefaultCache() : factory.GetCache(cacheName);

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
