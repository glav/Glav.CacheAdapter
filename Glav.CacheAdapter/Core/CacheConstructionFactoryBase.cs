using System;
using System.Linq;
using Glav.CacheAdapter.Core.Diagnostics;
using Glav.CacheAdapter.Core;
using Glav.CacheAdapter.DependencyManagement;
using Glav.CacheAdapter.Features;

namespace Glav.CacheAdapter.Core
{
    public abstract class CacheConstructionFactoryBase : ICacheConstructionFactory
    {
        private readonly ILogging _logger;
        private readonly CacheConfig _config;

        public CacheConstructionFactoryBase(ILogging logger, CacheConfig config)
        {
            _logger = logger;
            _config = config ?? new CacheConfig();
        }
        protected ILogging Logger { get { return _logger; } }

        protected CacheConfig CacheConfiguration { get { return _config; } }

        public void ParseConfig(string defaultServerIp, int defaultPort)
        {
            _config.ServerNodes.Clear();

            if (String.IsNullOrWhiteSpace(_config.DistributedCacheServers))
                return;

            ExtractServerNodesFromConfig(_config);
            if (_config.ServerNodes.Count == 0)
            {
                if (string.IsNullOrWhiteSpace(defaultServerIp) || defaultPort <= 0)
                {
                    throw new ArgumentException("No configuration specified and the default server IP and port are not valid");
                }
                _config.ServerNodes.Add(new ServerNode(defaultServerIp, defaultPort));
            }
            ExtractCacheSpecificConfiguration(_config);
        }

        private void ExtractCacheSpecificConfiguration(CacheConfig config)
        {
            if (string.IsNullOrWhiteSpace(config.CacheSpecificData))
                return;

            try
            {
                var configKeyPairs = config.CacheSpecificData.Split(new char[] { CacheConstants.ConfigItemPairSeparator });
                if (configKeyPairs.Length == 0)
                    return;
                foreach (var keyPair in configKeyPairs)
                {
                    if (!string.IsNullOrWhiteSpace(keyPair))
                    {
                        int posOfEquals = keyPair.IndexOf(CacheConstants.ConfigItemKeyValuePairSeparator);
                        if (posOfEquals >= 0)
                        {
                            var keyItem = keyPair.Substring(0, posOfEquals);
                            var keyValue = keyPair.Substring(posOfEquals + 1, keyPair.Length - (posOfEquals + 1));
                            if (!config.ProviderSpecificValues.ContainsKey(keyItem))
                            {
                                config.ProviderSpecificValues.Add(keyItem, keyValue);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteException(ex);
                throw new ArgumentException("Cache Specific configuration could not be parsed.", ex);
            }
        }

        private static void ExtractServerNodesFromConfig(CacheConfig config)
        {
            // Here we test to see if the old separator char is used.If not, we use the
            // preferred one, otherwise we revert to the obsolete one (for backwards compatibility)
            char separator = CacheConstants.ConfigDistributedServerSeparator;
            if (config.DistributedCacheServers.Contains(CacheConstants.ConfigDistributedServerSeparatorObsolete))
            {
                separator = CacheConstants.ConfigDistributedServerSeparatorObsolete;
            }
            var endPointList = config.DistributedCacheServers.Split(separator);
            if (endPointList.Length == 0)
                return;

            foreach (var endpoint in endPointList)
            {
                var endPointComponents = endpoint.Split(CacheConstants.ConfigDistributedServerPortSeparator);
                if (endPointComponents.Length < 2)
                    continue;

                int port;
                if (int.TryParse(endPointComponents[1], out port))
                {
                    var cacheEndpoint = new ServerNode(endPointComponents[0], port);
                    config.ServerNodes.Add(cacheEndpoint);
                }
            }
        }

        public abstract CacheFactoryComponentResult CreateCacheComponents();

    }
}
