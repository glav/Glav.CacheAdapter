using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Glav.CacheAdapter.Web;
using Glav.CacheAdapter.Core.Diagnostics;
using Glav.CacheAdapter.Distributed.AppFabric;
using Glav.CacheAdapter.Bootstrap;
using Glav.CacheAdapter.Distributed.memcached;

namespace Glav.CacheAdapter.Core.DependencyInjection
{
	public static class CacheBinder
	{
	    private static CacheConfig _config = new CacheConfig();

        public static ICacheProvider ResolveCacheFromConfig(ILogging logger, string cacheConfigEntry=null)
		{
			ICache cache = null;
			if (logger == null)
			{
				logger = new Logger();
			}

            if (string.IsNullOrWhiteSpace(cacheConfigEntry))
            {
                cacheConfigEntry = _config.CacheToUse;
            }
            switch (cacheConfigEntry)
			{
				case CacheTypes.MemoryCache:
					cache = new MemoryCacheAdapter(logger);
					break;
				case CacheTypes.WebCache:
					cache = new WebCacheAdapter(logger);
					break;
				case CacheTypes.AppFabricCache:
					cache = new AppFabricCacheAdapter(logger);
					break;
				case CacheTypes.memcached:
					cache = new memcachedAdapter(logger);
					break;
				default:
					cache = new MemoryCacheAdapter(logger);
					break;
			}

			var provider = new CacheProvider(cache, logger);
			logger.WriteInfoMessage(string.Format("CacheProvider initialised with {0} cache engine",cacheConfigEntry));

			return provider;
		}
	}
}
