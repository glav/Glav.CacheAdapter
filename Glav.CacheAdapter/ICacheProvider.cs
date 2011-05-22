using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Glav.CacheAdapter.Core
{
    public delegate T GetDataToCacheDelegate<T>();

    public interface ICacheProvider
    {
        T Get<T>(string cacheKey, DateTime absoluteExpiryDate, GetDataToCacheDelegate<T> getData) where T : class;
		T Get<T>(string cacheKey, TimeSpan slidingExpiryWindow, GetDataToCacheDelegate<T> getData) where T : class;
        void InvalidateCacheItem(string cacheKey);
    }
}
