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
        private const string DEFAULT_EndpointConfig = "localhost:22233";

        private ILogging _logger;

        public AppFabricCacheFactory()
        {
            _logger = new Logger();
        }
        public AppFabricCacheFactory(ILogging logger)
        {
            _logger = logger;
        }

        public DataCache ConstructCache(string endPointConfig)
        {
            if (string.IsNullOrWhiteSpace(endPointConfig))
                endPointConfig = DEFAULT_EndpointConfig;

            var config = ParseConfig(endPointConfig);
			var dataCacheEndpoints = new List<DataCacheServerEndpoint>();
			config.ServerNodes.ForEach(e => dataCacheEndpoints.Add(new DataCacheServerEndpoint(e.IPAddressOrHostName,e.Port)));

            var factoryConfig = new DataCacheFactoryConfiguration();
			factoryConfig.Servers = dataCacheEndpoints;

            try
            {
				var factory = new DataCacheFactory(factoryConfig);
                DataCacheClientLogManager.ChangeLogLevel(System.Diagnostics.TraceLevel.Error);

				// Note: When setting up AppFabric. The configured cache needs to be created by the admin using the New-Cache powershell command
            	var cache = factory.GetCache(MainConfig.Default.DistributedCacheName);
                return cache;
            }
            catch (Exception ex)
            {
                _logger.WriteException(ex);
                throw;
            }
            
        }

    }
}
