using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Glav.CacheAdapter;

namespace Glav.CacheAdapter.Core
{
    public interface ICache
    {
        void Add<T>(string cacheKey, DateTime absoluteExpiry, T dataToAdd) where T : class;
		void Add<T>(string cacheKey, TimeSpan slidingExpiryWindow, T dataToAdd) where T : class;
        T Get<T>(string cacheKey) where T : class;
        void Add(string cacheKey, DateTime absoluteExpiry, object dataToAdd);
		void Add(string cacheKey, TimeSpan slidingExpiryWindow, object dataToAdd);
        object Get(string cacheKey);
        void InvalidateCacheItem(string cacheKey);
    	void AddToPerRequestCache(string cacheKey, object dataToAdd);
    }
}
