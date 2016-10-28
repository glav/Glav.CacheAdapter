using System;
using System.Collections.Generic;
using Glav.CacheAdapter.Core.Diagnostics;
using Microsoft.ApplicationServer.Caching;
using Glav.CacheAdapter.Core;
using Glav.CacheAdapter.Web;

namespace Glav.CacheAdapter.Distributed.AppFabric
{
    public class AppFabricCacheAdapter : ICache
    {
        private DataCache _cache;
        private readonly ILogging _logger;

        private readonly PerRequestCacheHelper _requestCacheHelper = new PerRequestCacheHelper();

        public AppFabricCacheAdapter(ILogging logger, DataCache cache)
        {
            _logger = logger;
            _cache = cache;
        }

        public void Add(string cacheKey, DateTime expiry, object dataToAdd)
        {
            if (expiry > DateTime.Now && dataToAdd != null)
            {
                TimeSpan timeout = expiry - DateTime.Now;
                _cache.Put(cacheKey, dataToAdd, timeout);
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
                _logger.WriteInfoMessage(string.Format("Adding data to cache with cache key: {0}, sliding window expiry in seconds {1}", cacheKey, slidingExpiryWindow.TotalSeconds));
                _cache.Put(cacheKey, dataToAdd, slidingExpiryWindow);
            }
        }

        public void AddToPerRequestCache(string cacheKey, object dataToAdd)
        {
            _requestCacheHelper.AddToPerRequestCache(cacheKey, dataToAdd);
        }


        public CacheSetting CacheType
        {
            get { return CacheSetting.AppFabric; }
        }


        public void ClearAll()
        {
            try
            {
                // if in Azure, currently this throws an exception and therefore clear the cache is
                // not available currently in Azure.
                var regions = _cache.GetSystemRegions();
                foreach (string regionName in regions)
                {
                    _cache.ClearRegion(regionName);
                }

            }
            catch (Exception ex)
            {
                _logger.WriteException(ex);
                _logger.WriteInfoMessage("Clearing the cache cannot be performed, not currently support by Windows Azure");
            }
        }
    }
}
