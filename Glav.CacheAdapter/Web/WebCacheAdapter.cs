using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Caching;
using Glav.CacheAdapter.Core;

namespace Glav.CacheAdapter.Web
{
    public class WebCacheAdapter : ICache
    {
        private System.Web.Caching.Cache _cache;

        public WebCacheAdapter()
        {
            if (System.Web.HttpContext.Current != null)
                _cache = System.Web.HttpContext.Current.Cache;
            else
                throw new ArgumentNullException("Not in a web context, unable to use the web cache.");
        }

        #region ICache Members

        public void Add(string cacheKey, DateTime expiry, object dataToAdd)
        {
            if (dataToAdd != null)
                _cache.Add(cacheKey, dataToAdd, null, expiry, Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
        }

        public T Get<T>(string cacheKey) where T : class
        {
            T data = _cache.Get(cacheKey) as T;
			if (data == null)
			{
				if (System.Web.HttpContext.Current.Items.Contains(cacheKey))
				{
					return System.Web.HttpContext.Current.Items[cacheKey] as T;
				}
			}
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
				_cache.Add(cacheKey, dataToAdd, null, Cache.NoAbsoluteExpiration, slidingExpiryWindow, CacheItemPriority.BelowNormal,
				           null);
			}
		}

		public void AddToPerRequestCache(string cacheKey, object dataToAdd)
		{
			if (dataToAdd != null && System.Web.HttpContext.Current != null)
			{
				if (!System.Web.HttpContext.Current.Items.Contains(cacheKey))
				{
					System.Web.HttpContext.Current.Items.Add(cacheKey,dataToAdd);
				}
				else
				{
					System.Web.HttpContext.Current.Items[cacheKey] = dataToAdd;
				}
				
			}
		}

		#endregion

	}
}
