using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Glav.CacheAdapter.Core
{
    public interface ICacheProvider
    {
        T Get<T>(string cacheKey, DateTime absoluteExpiryDate, Func<T> getData) where T : class;
		T Get<T>(string cacheKey, TimeSpan slidingExpiryWindow, Func<T> getData) where T : class;
		T Get<T>(DateTime absoluteExpiryDate, Func<T> getData) where T : class;
		T Get<T>(TimeSpan slidingExpiryWindow, Func<T> getData) where T : class;
		void InvalidateCacheItem(string cacheKey);
    	void Add(string cacheKey, DateTime absoluteExpiryDate, object dataToAdd);
		void Add(string cacheKey, TimeSpan slidingExpiryWindow, object dataToAdd);
    	void AddToPerRequestCache(string cacheKey, object dataToAdd);
    }
}
