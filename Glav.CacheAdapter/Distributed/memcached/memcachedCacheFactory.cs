using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Glav.CacheAdapter.Core.Diagnostics;

namespace Glav.CacheAdapter.Distributed.memcached
{
	public class memcachedCacheFactory
	{
		private ILogging _logger;
		private const string DEFAULT_EndpointConfig = "127.0.0.1:11211";

		public memcachedCacheFactory()
		{
			_logger = new Logger();
		}
		public memcachedCacheFactory(ILogging logger)
		{
			_logger = logger;
		}

		public CacheServerFarm ConstructCacheFarm(string endPointConfig)
		{
			if (string.IsNullOrWhiteSpace(endPointConfig))
				endPointConfig = DEFAULT_EndpointConfig;

			var serverNodes = ParseConfig(endPointConfig);

			try
			{
				var serverFarm = new CacheServerFarm();
				serverFarm.Initialise(serverNodes);
				return serverFarm;
			}
			catch (Exception ex)
			{
				_logger.WriteException(ex);
				throw;
			}
		}

		public List<ServerNode> ParseConfig(string configValue)
		{
			List<ServerNode> config = new List<ServerNode>();

			if (String.IsNullOrWhiteSpace(configValue))
				return config;

			var endPointList = configValue.Split(',');
			if (endPointList.Length == 0)
				return config;

			foreach (var endpoint in endPointList)
			{
				var endPointComponents = endpoint.Split(':');
				if (endPointComponents.Length < 2)
					continue;

				int port;
				if (int.TryParse(endPointComponents[1], out port))
				{
					var cacheEndpoint = new ServerNode(endPointComponents[0], port);
					config.Add(cacheEndpoint);
				}
			}

			return config;
		}

	}
}
