using Glav.CacheAdapter.Core;
using Glav.CacheAdapter.Core.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Glav.CacheAdapter.Distributed.Redis
{
    public class RedisCacheAdatper : ICache
    {
		private ILogging _logger;
        private RedisCacheFactory _factory;
        

        public RedisCacheAdatper(ILogging logger, CacheConfig config = null)
        {
            _logger = logger;
            _factory = new RedisCacheFactory(logger, config);

        }
        public T Get<T>(string cacheKey) where T : class
        {
            throw new NotImplementedException();
        }

        public void Add(string cacheKey, DateTime absoluteExpiry, object dataToAdd)
        {
            throw new NotImplementedException();
        }

        public void Add(string cacheKey, TimeSpan slidingExpiryWindow, object dataToAdd)
        {
            throw new NotImplementedException();
        }

        public void InvalidateCacheItem(string cacheKey)
        {
            throw new NotImplementedException();
        }

        public void AddToPerRequestCache(string cacheKey, object dataToAdd)
        {
            throw new NotImplementedException();
        }

        public CacheSetting CacheType
        {
            get { throw new NotImplementedException(); }
        }

        public void ClearAll()
        {
            throw new NotImplementedException();
        }
    }
}
