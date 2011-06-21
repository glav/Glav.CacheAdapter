using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.ApplicationServer.Caching;
using System.Diagnostics;
using Glav.CacheAdapter.Core.Diagnostics;

namespace Glav.CacheAdapter.Distributed.AppFabric
{
    public class AppFabricCacheFactory
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

            var endPoints = ParseConfig(endPointConfig);

            DataCacheFactoryConfiguration config = new DataCacheFactoryConfiguration();
            config.Servers = endPoints;

            try
            {
                DataCacheFactory factory = new DataCacheFactory(config);
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

        public List<DataCacheServerEndpoint> ParseConfig(string configValue)
        {
            List<DataCacheServerEndpoint> config = new List<DataCacheServerEndpoint>();

            if (String.IsNullOrWhiteSpace(configValue))
                return config;

            var endPointList = configValue.Split(',');
            if (endPointList.Length == 0)
                return config;

            foreach(var endpoint in endPointList)
            {
                var endPointComponents = endpoint.Split(':');
                if (endPointComponents.Length < 2)
                    continue;

                int port;
                if (int.TryParse(endPointComponents[1],out port))
                {
                    var cacheEndpoint = new DataCacheServerEndpoint(endPointComponents[0],port);
                    config.Add(cacheEndpoint);
                }
            }

            return config;
        }


    }
}
