using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Unity;
using Microsoft.ApplicationServer.Caching;
using Glav.CacheAdapter.Core;
using Glav.CacheAdapter.Core.DependencyInjection;

namespace Glav.CacheAdapter.Distributed
{
    public class AppFabricCacheAdapter : ICache
    {
        private DataCache _cache;

        public AppFabricCacheAdapter()
        {
            var factory = AppServices.Container.Resolve<AppFabricCacheFactory>();
            _cache = factory.ConstructCache(MainConfig.Default.DistributedCacheServers);
        }

        public void Add<T>(string cacheKey, DateTime expiry, T dataToAdd) where T : class
        {
            if (expiry > DateTime.Now && dataToAdd != null)
            {
                TimeSpan timeout = expiry - DateTime.Now;
                _cache.Add(cacheKey, dataToAdd, timeout);
            }
        }

        public T Get<T>(string cacheKey) where T : class
        {
            T data = _cache.Get(cacheKey) as T;
            return data;
        }

        public void Add(string cacheKey, DateTime expiry, object dataToAdd)
        {
            Add<object>(cacheKey, expiry, dataToAdd);
        }

        public object Get(string cacheKey)
        {
            return _cache.Get(cacheKey);
        }

        public void InvalidateCacheItem(string cacheKey)
        {
            _cache.Remove(cacheKey);
        }


		public void Add<T>(string cacheKey, TimeSpan slidingExpiryWindow, T dataToAdd) where T : class
		{
			if (dataToAdd != null)
			{
				_cache.Add(cacheKey, dataToAdd, slidingExpiryWindow);
			}
		}

		public void Add(string cacheKey, TimeSpan slidingExpiryWindow, object dataToAdd)
		{
			Add<object>(cacheKey,slidingExpiryWindow,dataToAdd);
		}

		public void AddToPerRequestCache(string cacheKey, object dataToAdd)
		{
			// AppFabric does not have a per request concept nor does it need to since all cache nodes should be in sync
			// You could simulate this in code with a dependency on the ASP.NET framework and its inbuilt request
			// objects but we wont do that here. We simply add it into the cache for 1 second.
			// Its hacky but this behaviour will be specific to the scenario at hand.
			Add<object>(cacheKey,TimeSpan.FromSeconds(1),dataToAdd);
		}
	}
}
