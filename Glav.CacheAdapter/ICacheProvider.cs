using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Glav.CacheAdapter.Core
{
    public delegate T GetDataToCacheDelegate<T>();

    public interface ICacheProvider
    {
        T Get<T>(string cacheKey, DateTime absoluteExpiryDate, GetDataToCacheDelegate<T> getData, bool addToPerRequestCache = false) where T : class;
		T Get<T>(string cacheKey, TimeSpan slidingExpiryWindow, GetDataToCacheDelegate<T> getData, bool addToPerRequestCache = false) where T : class;
        void InvalidateCacheItem(string cacheKey);
    	void Add(string cacheKey, DateTime absoluteExpiryDate, object dataToAdd);
		void Add(string cacheKey, TimeSpan slidingExpiryWindow, object dataToAdd);
    	void AddToPerRequestCache(string cacheKey, object dataToAdd);
    }
}
