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
        private PerRequestCacheHelper _requestCacheHelper = new PerRequestCacheHelper();
        private string _regionName = null;

        public AppFabricCacheAdapter(ILogging logger)
        {
        	_logger = logger;
        	var factory = new AppFabricCacheFactory(_logger);
            _cache = factory.ConstructCache();
            setRegionName();
        }

        private void setRegionName()
        {
            _regionName = Assembly.GetEntryAssembly().FullName;
        }

        public void Add(string cacheKey, DateTime expiry, object dataToAdd)
        {
            if (expiry > DateTime.Now && dataToAdd != null)
            {
				TimeSpan timeout = expiry - DateTime.Now;
                _cache.Put(cacheKey, dataToAdd, timeout,_regionName);
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

            T data = _cache.Get(cacheKey,_regionName) as T;
            return data;
        }

        public void InvalidateCacheItem(string cacheKey)
        {
            _cache.Remove(cacheKey,_regionName);
        }


		public void Add(string cacheKey, TimeSpan slidingExpiryWindow, object dataToAdd)
		{
			if (dataToAdd != null)
			{
				_logger.WriteInfoMessage(string.Format("Adding data to cache with cache key: {0}, sliding window expiry in seconds {1}", cacheKey, slidingExpiryWindow.TotalSeconds));
				_cache.Put(cacheKey, dataToAdd, slidingExpiryWindow,_regionName);
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
            _cache.ClearRegion(_regionName);
        }
    }
}
