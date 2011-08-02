using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Glav.CacheAdapter.Core.Diagnostics;

namespace Glav.CacheAdapter.Distributed.memcached
{
	public class memcachedCacheFactory : DistributedCacheFactoryBase
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

			var config = ParseConfig(endPointConfig);

			try
			{
				var serverFarm = new CacheServerFarm();
				serverFarm.Initialise(config.ServerNodes);
				return serverFarm;
			}
			catch (Exception ex)
			{
				_logger.WriteException(ex);
				throw;
			}
		}
	}
}
