﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Glav.CacheAdapter;

namespace Glav.CacheAdapter.Core
{
    public interface ICache
    {
        T Get<T>(string cacheKey) where T : class;
        void Add(string cacheKey, DateTime absoluteExpiry, object dataToAdd);
		void Add(string cacheKey, TimeSpan slidingExpiryWindow, object dataToAdd);
        void InvalidateCacheItem(string cacheKey);
    	void AddToPerRequestCache(string cacheKey, object dataToAdd);
    	CacheSetting CacheType { get; }
    }
}
