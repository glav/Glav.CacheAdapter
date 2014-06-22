﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using Glav.CacheAdapter.Core.Diagnostics;

namespace Glav.CacheAdapter.Distributed
{
	public class DistributedCacheFactoryBase
	{
		private ILogging _logger;
        private CacheConfig _config;

		public DistributedCacheFactoryBase()
		{
			_logger = new Logger();
            _config = new CacheConfig();
		}

		public DistributedCacheFactoryBase(ILogging logger)
		{
			_logger = logger;
            _config = new CacheConfig();
		}
        public DistributedCacheFactoryBase(ILogging logger, CacheConfig config)
        {
            _logger = logger;
            _config = config;
        }
        protected ILogging Logger { get { return _logger; } }

        protected CacheConfig CacheConfiguration { get { return _config; } }

		public void ParseConfig(string defaultServerIp, int defaultPort)
		{
            if (String.IsNullOrWhiteSpace(_config.DistributedCacheServers))
				return;

			ExtractServerNodesFromConfig(_config);
			if (_config.ServerNodes.Count == 0)
			{
				_config.ServerNodes.Add(new ServerNode(defaultServerIp,defaultPort));
			}
			ExtractCacheSpecificConfiguration(_config);

			return;
		}

		private void ExtractCacheSpecificConfiguration(CacheConfig config)
		{
			if (string.IsNullOrWhiteSpace(config.CacheSpecificData))
				return;

			try
			{
				var configKeyPairs = config.CacheSpecificData.Split(new char[] {CacheConstants.ConfigItemPairSeparator});
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
			} catch (Exception ex)
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
	}
}
