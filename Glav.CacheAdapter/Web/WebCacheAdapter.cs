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

        public void Add<T>(string cacheKey, DateTime expiry, T dataToAdd) where T : class
        {
            if (dataToAdd != null)
                _cache.Add(cacheKey, dataToAdd, null, expiry, Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
        }

        public void Add(string cacheKey, DateTime expiry, object dataToAdd)
        {
            Add<object>(cacheKey, expiry, dataToAdd);
        }

        public T Get<T>(string cacheKey) where T : class
        {
            T data = _cache.Get(cacheKey) as T;
            return data;
        }

        public object Get(string cacheKey)
        {
            return _cache.Get(cacheKey);
        }

        public void InvalidateCacheItem(string cacheKey)
        {
            if (_cache.Get(cacheKey) != null)
                _cache.Remove(cacheKey);
        }

        #endregion
    }
}
