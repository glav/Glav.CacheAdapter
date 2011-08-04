using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Glav.CacheAdapter.Core.Diagnostics;

namespace Glav.CacheAdapter.Distributed.memcached
{
	public class memcachedCacheFactory : DistributedCacheFactoryBase
	{
		private const string DEFAULT_IpAddress = "127.0.0.1";
		private const int DEFAULT_Port = 11211;

		public memcachedCacheFactory(ILogging logger) : base(logger)
		{
		}

		public CacheServerFarm ConstructCacheFarm()
		{
			var config = ParseConfig(DEFAULT_IpAddress,DEFAULT_Port);

			try
			{
				var serverFarm = new CacheServerFarm();
				serverFarm.Initialise(config.ServerNodes);
				return serverFarm;
			}
			catch (Exception ex)
			{
				Logger.WriteException(ex);
				throw;
			}
		}
	}
}
