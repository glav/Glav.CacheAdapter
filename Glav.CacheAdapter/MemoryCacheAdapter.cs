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

        public void Add<T>(string cacheKey, DateTime expiry, T dataToAdd) where T : class
        {
            var policy = new CacheItemPolicy();
            policy.AbsoluteExpiration = new DateTimeOffset(expiry);

            if (dataToAdd != null)
                _cache.Add(cacheKey, dataToAdd, policy);
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
            if (_cache.Contains(cacheKey))
                _cache.Remove(cacheKey);
        }


		public void Add<T>(string cacheKey, TimeSpan slidingExpiryWindow, T dataToAdd) where T : class
		{
			if (dataToAdd != null)
			{
				var item = new CacheItem(cacheKey, dataToAdd);
				var policy = new CacheItemPolicy() {SlidingExpiration = slidingExpiryWindow};
				_cache.Add(item, policy);
			}
		}

		public void Add(string cacheKey, TimeSpan slidingExpiryWindow, object dataToAdd)
		{
			Add<object>(cacheKey,slidingExpiryWindow,dataToAdd);
		}

		public void AddToPerRequestCache(string cacheKey, object dataToAdd)
		{
			// There is no per request cache do we dont do anything.
		}
	}
}
