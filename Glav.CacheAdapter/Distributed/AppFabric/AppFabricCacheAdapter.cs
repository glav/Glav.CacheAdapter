using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Glav.CacheAdapter.Core.Diagnostics;
using Microsoft.ApplicationServer.Caching;
using Glav.CacheAdapter.Core;
using Glav.CacheAdapter.Core.DependencyInjection;
using Glav.CacheAdapter.Web;
using System.Reflection;

namespace Glav.CacheAdapter.Distributed.AppFabric
{
    public class AppFabricCacheAdapter : ICache
    {
        private DataCache _cache;
        private ILogging _logger;
        private bool _isLocalCacheEnabled = false;

        private PerRequestCacheHelper _requestCacheHelper = new PerRequestCacheHelper();

        public AppFabricCacheAdapter(ILogging logger, CacheConfig config = null)
        {
            _logger = logger;
            var factory = new AppFabricCacheFactory(_logger,config);

            _cache = factory.ConstructCache();
            _isLocalCacheEnabled = factory.IsLocalCacheEnabled;
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
                
            } catch (Exception ex)
            {
                _logger.WriteException(ex);
                _logger.WriteInfoMessage("Clearing the cache cannot be performed, not currently support by Windows Azure");
            }
        }
    }
}
