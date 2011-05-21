using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Glav.CacheAdapter.Core
{
    public delegate T GetDataToCacheDelegate<T>();

    public interface ICacheProvider
    {
        T Get<T>(string cacheKey, DateTime expiryDate, GetDataToCacheDelegate<T> getData) where T : class;
        object GetObject(string cacheKey, DateTime expiryDate, GetDataToCacheDelegate<object> getData);
        void InvalidateCacheItem(string cacheKey);
    }
}
