using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using Glav.CacheAdapter.Core.Diagnostics;
using Glav.CacheAdapter.Web;

namespace Glav.CacheAdapter.Core
{
    /// <summary>
    /// In memory cache with no dependencies on the web cache, only runtime dependencies.
    /// ie. Can be used in any type of application, desktop, web, service or otherwise.
    /// </summary>
    public class MemoryCacheAdapter : ICache
    {
        private readonly MemoryCache _cache;
        private readonly ILogging _logger;
        private readonly PerRequestCacheHelper _requestCacheHelper = new PerRequestCacheHelper();


        public MemoryCacheAdapter(ILogging logger, MemoryCache cache)
        {
            _logger = logger;
            _cache = cache;
        }

        public void Add(string cacheKey, DateTime expiry, object dataToAdd)
        {
            var policy = new CacheItemPolicy
            {
                AbsoluteExpiration = new DateTimeOffset(expiry)
            };

            if (dataToAdd != null)
            {
                _cache.Add(cacheKey, dataToAdd, policy);
                _logger.WriteInfoMessage(string.Format("Adding data to cache with cache key: {0}, expiry date {1}", cacheKey, expiry.ToString("yyyy/MM/dd hh:mm:ss")));

            }
        }

        public T Get<T>(string cacheKey) where T : class
        {
            // try per request cache first, but only if in a web context
            var requestCacheData = _requestCacheHelper.TryGetItemFromPerRequestCache<T>(cacheKey);
            if (requestCacheData != null)
            {
                return requestCacheData;
            }

            T data = _cache.Get(cacheKey) as T;
            return data;
        }

        public void InvalidateCacheItem(string cacheKey)
        {
            if (_cache.Contains(cacheKey))
                _cache.Remove(cacheKey);
        }

        public void InvalidateCacheItems(IEnumerable<string> cacheKeys)
        {
            if (cacheKeys == null)
            {
                return;
            }
            _logger.WriteInfoMessage("Invalidating a series of cache keys");
            foreach (var cacheKey in cacheKeys)
            {
                _cache.Remove(cacheKey);
            }
        }

        public void Add(string cacheKey, TimeSpan slidingExpiryWindow, object dataToAdd)
        {
            if (dataToAdd != null)
            {
                var item = new CacheItem(cacheKey, dataToAdd);
                var policy = new CacheItemPolicy() { SlidingExpiration = slidingExpiryWindow };
                _cache.Add(item, policy);
                _logger.WriteInfoMessage(string.Format("Adding data to cache with cache key: {0}, sliding expiry window in seconds {1}", cacheKey, slidingExpiryWindow.TotalSeconds));

            }
        }

        public void AddToPerRequestCache(string cacheKey, object dataToAdd)
        {
            _requestCacheHelper.AddToPerRequestCache(cacheKey, dataToAdd);
        }


        public CacheSetting CacheType
        {
            get { return CacheSetting.Memory; }
        }


        public void ClearAll()
        {
            _logger.WriteInfoMessage("Clearing the cache");
            _cache.ToList().ForEach(i =>
                                        {
                                            _cache.Remove(i.Key);
                                        });
        }
    }
}
