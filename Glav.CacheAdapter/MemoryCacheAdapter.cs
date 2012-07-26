using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Caching;
using Glav.CacheAdapter.Core.Diagnostics;

namespace Glav.CacheAdapter.Core
{
    /// <summary>
    /// In memory cache with no dependencies on the web cache, only runtime dependencies.
    /// ie. Can be used in any type of application, desktop, web, service or otherwise.
    /// </summary>
    public class MemoryCacheAdapter : ICache
    {
        private MemoryCache _cache = MemoryCache.Default;
        private ILogging _logger;

        public MemoryCacheAdapter()
        {
            _logger = new Logger();
        }
        public MemoryCacheAdapter(ILogging logger)
        {
            _logger = logger;
        }

		public void Add(string cacheKey, DateTime expiry, object dataToAdd)
		{
			var policy = new CacheItemPolicy();
			policy.AbsoluteExpiration = new DateTimeOffset(expiry);

			if (dataToAdd != null)
			{
				_cache.Add(cacheKey, dataToAdd, policy);
				_logger.WriteInfoMessage(string.Format("Adding data to cache with cache key: {0}, expiry date {1}", cacheKey, expiry.ToString("yyyy/MM/dd hh:mm:ss")));

			}
		}

    	public T Get<T>(string cacheKey) where T : class
        {
            T data = _cache.Get(cacheKey) as T;
            return data;
        }

        public void InvalidateCacheItem(string cacheKey)
        {
            if (_cache.Contains(cacheKey))
                _cache.Remove(cacheKey);
        }

		public void Add(string cacheKey, TimeSpan slidingExpiryWindow, object dataToAdd)
		{
			if (dataToAdd != null)
			{
				var item = new CacheItem(cacheKey, dataToAdd);
				var policy = new CacheItemPolicy() {SlidingExpiration = slidingExpiryWindow};
				_cache.Add(item, policy);
				_logger.WriteInfoMessage(string.Format("Adding data to cache with cache key: {0}, sliding expiry window in seconds {1}", cacheKey, slidingExpiryWindow.TotalSeconds));

			}
		}

		public void AddToPerRequestCache(string cacheKey, object dataToAdd)
		{
			// memory cache does not have a per request concept nor does it need to since all cache nodes should be in sync
			// You could simulate this in code with a dependency on the ASP.NET framework and its inbuilt request
			// objects but we wont do that here. We simply add it into the cache for 1 second.
			// Its hacky but this behaviour will be specific to the scenario at hand.
			Add(cacheKey, TimeSpan.FromSeconds(1), dataToAdd);
		}


		public CacheSetting CacheType
		{
			get { return CacheSetting.Memory; }
		}
	}
}
