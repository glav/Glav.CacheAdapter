using System;
using System.Collections.Generic;

namespace Glav.CacheAdapter.Core
{
    public interface ICache
    {
        T Get<T>(string cacheKey) where T : class;
        void Add(string cacheKey, DateTime absoluteExpiry, object dataToAdd);
		void Add(string cacheKey, TimeSpan slidingExpiryWindow, object dataToAdd);
        void InvalidateCacheItem(string cacheKey);
        void InvalidateCacheItems(IEnumerable<string> cacheKeys);
    	CacheSetting CacheType { get; }
        void ClearAll();
    }
}
