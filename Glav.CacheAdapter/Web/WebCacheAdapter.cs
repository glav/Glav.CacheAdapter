using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Caching;
using Glav.CacheAdapter.Core;
using Glav.CacheAdapter.Core.Diagnostics;

namespace Glav.CacheAdapter.Web
{
    public class WebCacheAdapter : ICache
    {
        private System.Web.Caching.Cache _cache;
        private ILogging _logger;
        private PerRequestCacheHelper _requestCacheHelper = new PerRequestCacheHelper();

        public WebCacheAdapter(ILogging logger)
        {
            _logger = logger;

            if (System.Web.HttpContext.Current != null)
                _cache = System.Web.HttpContext.Current.Cache;
            else
                _cache = System.Web.HttpRuntime.Cache;
        }

        public void Add(string cacheKey, DateTime expiry, object dataToAdd)
        {
            if (dataToAdd != null)
            {
                _cache.Add(cacheKey, dataToAdd, null, expiry, Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
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
            if (_cache.Get(cacheKey) != null)
                _cache.Remove(cacheKey);
        }

        public void Add(string cacheKey, TimeSpan slidingExpiryWindow, object dataToAdd)
        {
            if (dataToAdd != null)
            {
                _logger.WriteInfoMessage(string.Format("Adding data to cache with cache key: {0}, sliding window expiry in seconds {1}", cacheKey, slidingExpiryWindow.TotalSeconds));
                _cache.Add(cacheKey, dataToAdd, null, Cache.NoAbsoluteExpiration, slidingExpiryWindow, CacheItemPriority.BelowNormal,
                           null);
            }
        }

        public void AddToPerRequestCache(string cacheKey, object dataToAdd)
        {
            _requestCacheHelper.AddToPerRequestCache(cacheKey, dataToAdd);
        }

        public CacheSetting CacheType
        {
            get { return CacheSetting.Web; }
        }


        public void ClearAll()
        {
            if (_cache.Count == 0)
            {
                return;
            }

            _logger.WriteInfoMessage("Clearing the cache");

            foreach (var item in _cache)
            {
                // Granular exception around clearing cache items so it can continue
                //clearing if an error
                try
                {
                    var entry = (System.Collections.DictionaryEntry)item;
                    _cache.Remove(entry.Key.ToString());
                }
                catch (Exception ex)
                {
                    _logger.WriteErrorMessage("Error removing item from cache during ClearAll");
                }
            }
        }
    }
}
