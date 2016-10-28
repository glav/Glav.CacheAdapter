using System;
using System.Collections.Generic;
using Enyim.Caching;
using Enyim.Caching.Memcached;
using Glav.CacheAdapter.Core;
using Glav.CacheAdapter.Core.Diagnostics;
using Glav.CacheAdapter.Web;

namespace Glav.CacheAdapter.Distributed.memcached
{
    public class memcachedAdapter : ICache
    {
        private readonly ILogging _logger;
        private readonly PerRequestCacheHelper _requestCacheHelper = new PerRequestCacheHelper();

        private static IMemcachedClient _client;


        public memcachedAdapter(ILogging logger, IMemcachedClient client)
        {
            _logger = logger;
            _client = client;
        }

        public T Get<T>(string cacheKey) where T : class
        {
            try
            {
                var sanitisedKey = SanitiseCacheKey(cacheKey);
                // try per request cache first, but only if in a web context
                var requestCacheData = _requestCacheHelper.TryGetItemFromPerRequestCache<T>(cacheKey);
                if (requestCacheData != null)
                {
                    return requestCacheData;
                }

                return _client.Get<T>(sanitisedKey);
            }
            catch (Exception ex)
            {
                _logger.WriteException(ex);
            }
            return null;
        }

        public void Add(string cacheKey, DateTime absoluteExpiry, object dataToAdd)
        {
            try
            {
                var sanitisedKey = SanitiseCacheKey(cacheKey);
                var success = _client.Store(StoreMode.Set, sanitisedKey, dataToAdd, absoluteExpiry);
                if (!success)
                {
                    _logger.WriteErrorMessage(string.Format("Unable to store item in cache. CacheKey:{0}", sanitisedKey));
                }
            }
            catch (Exception ex)
            {
                _logger.WriteException(ex);
            }
        }

        public void Add(string cacheKey, TimeSpan slidingExpiryWindow, object dataToAdd)
        {
            try
            {
                var sanitisedKey = SanitiseCacheKey(cacheKey);
                var success = _client.Store(StoreMode.Set, sanitisedKey, dataToAdd, slidingExpiryWindow);
                if (!success)
                {
                    _logger.WriteErrorMessage(string.Format("Unable to store item in cache. CacheKey:{0}", sanitisedKey));
                }
            }
            catch (Exception ex)
            {
                _logger.WriteException(ex);
            }
        }

        public void InvalidateCacheItem(string cacheKey)
        {
            try
            {
                var sanitisedKey = SanitiseCacheKey(cacheKey);
                var success = _client.Remove(sanitisedKey);
                if (!success)
                {
                    _logger.WriteErrorMessage(string.Format("Unable to remove item from cache. CacheKey:{0}", sanitisedKey));
                }
            }
            catch (Exception ex)
            {
                _logger.WriteException(ex);
            }
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
                InvalidateCacheItem(cacheKey);
            }
        }


        public void AddToPerRequestCache(string cacheKey, object dataToAdd)
        {
            _requestCacheHelper.AddToPerRequestCache(cacheKey, dataToAdd);
        }

        public CacheSetting CacheType
        {
            get { return CacheSetting.memcached; }
        }

        private string SanitiseCacheKey(string cacheKey)
        {
            if (string.IsNullOrWhiteSpace(cacheKey))
            {
                throw new ArgumentException("Cannot have an empty or NULL cache key");
            }
            return cacheKey.Replace(" ", string.Empty).Replace("#", "-");
        }


        public void ClearAll()
        {
            _logger.WriteInfoMessage("Clearing the cache");
            try
            {
                _client.FlushAll();
            }
            catch (Exception ex)
            {
                _logger.WriteErrorMessage("Error flushing the cache:" + ex.Message);
            }
        }
    }
}
