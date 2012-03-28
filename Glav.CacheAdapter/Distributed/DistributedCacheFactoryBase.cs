using System;
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

		public DistributedCacheFactoryBase()
		{
			_logger = new Logger();	
		}

		public DistributedCacheFactoryBase(ILogging logger)
		{
			_logger = logger;
		}
		protected ILogging Logger { get { return _logger; } }

		public CacheConfig ParseConfig(string defaultServerIp, int defaultPort)
		{
			CacheConfig config = new CacheConfig();

			if (String.IsNullOrWhiteSpace(MainConfig.Default.DistributedCacheServers))
				return config;

			ExtractServerNodesFromConfig(config);
			if (config.ServerNodes.Count == 0)
			{
				config.ServerNodes.Add(new ServerNode(defaultServerIp,defaultPort));
			}
			ExtractCacheSpecificConfiguration(config);

			return config;
		}

		private void ExtractCacheSpecificConfiguration(CacheConfig config)
		{
			if (string.IsNullOrWhiteSpace(MainConfig.Default.CacheSpecificData))
				return;

			try
			{
				var configKeyPairs = MainConfig.Default.CacheSpecificData.Split(new char[] {CacheConstants.ConfigItemPairSeparator});
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
			if (MainConfig.Default.DistributedCacheServers.Contains(CacheConstants.ConfigDistributedServerSeparatorObsolete))
			{
				separator = CacheConstants.ConfigDistributedServerSeparatorObsolete;
			}
			var endPointList = MainConfig.Default.DistributedCacheServers.Split(separator);
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
