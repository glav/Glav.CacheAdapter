using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Unity;
using Microsoft.ApplicationServer.Caching;
using Glav.CacheAdapter.Core;
using Glav.CacheAdapter.Core.DependencyInjection;

namespace Glav.CacheAdapter.Distributed
{
    public class AppFabricCacheAdapter : ICache
    {
        private DataCache _cache;

        public AppFabricCacheAdapter()
        {
            var factory = AppServices.Container.Resolve<AppFabricCacheFactory>();
            _cache = factory.ConstructCache(MainConfig.Default.DistributedCacheServers);
        }

        public void Add<T>(string cacheKey, DateTime expiry, T dataToAdd) where T : class
        {
            if (expiry > DateTime.Now && dataToAdd != null)
            {
                TimeSpan timeout = expiry - DateTime.Now;
                _cache.Add(cacheKey, dataToAdd, timeout);
            }
        }

        public T Get<T>(string cacheKey) where T : class
        {
            T data = _cache.Get(cacheKey) as T;
            return data;
        }

        public void Add(string cacheKey, DateTime expiry, object dataToAdd)
        {
            Add<object>(cacheKey, expiry, dataToAdd);
        }

        public object Get(string cacheKey)
        {
            return _cache.Get(cacheKey);
        }

        public void InvalidateCacheItem(string cacheKey)
        {
            _cache.Remove(cacheKey);
        }
    }
}
