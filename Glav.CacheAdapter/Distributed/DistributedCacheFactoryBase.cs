using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Glav.CacheAdapter.Distributed
{
	public class DistributedCacheFactoryBase
	{
		public CacheConfig ParseConfig(string configValue)
		{
			CacheConfig config = new CacheConfig();

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
					config.ServerNodes.Add(cacheEndpoint);
				}
			}

			return config;
		}

	}
}
