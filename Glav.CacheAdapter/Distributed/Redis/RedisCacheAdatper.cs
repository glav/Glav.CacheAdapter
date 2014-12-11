using Glav.CacheAdapter.Core;
using Glav.CacheAdapter.Core.Diagnostics;
using Glav.CacheAdapter.Web;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Glav.CacheAdapter.Helpers;

namespace Glav.CacheAdapter.Distributed.Redis
{
    public class RedisCacheAdatper : ICache
    {
		private ILogging _logger;
        private RedisCacheFactory _factory;
        private PerRequestCacheHelper _requestCacheHelper = new PerRequestCacheHelper();
        private static IDatabase _db = null;
        private CacheConfig _config = null;

        public RedisCacheAdatper(ILogging logger, CacheConfig config = null)
        {
            _logger = logger;
            _config = config;
            _factory = new RedisCacheFactory(logger, _config);

            _db = _factory.ConstructCacheInstance();
        }
        public T Get<T>(string cacheKey) where T : class
        {
            try
            {
                var requestCacheData = _requestCacheHelper.TryGetItemFromPerRequestCache<T>(cacheKey);
                if (requestCacheData != null)
                {
                    return requestCacheData;
                }

                var data = new RedisValue();
                if (_config.IsCacheDependencyManagementEnabled && _db.KeyType(cacheKey) == RedisType.List)
                {
                    var cacheValue = _db.ListGetByIndex(cacheKey, 0);
                    if (cacheValue.HasValue && cacheValue != string.Empty)
                    {
                        data = cacheValue;
                    }
                }
                else
                {
                    data = _db.StringGet(cacheKey);
                }
                if (!data.IsNull && data.HasValue)
                {
                    var blobBytes = (byte[])data;
                    var deserialisedObject = blobBytes.Deserialize<T>();
                    return deserialisedObject;
                }
            }
            catch (Exception ex)
            {
                _logger.WriteException(ex);
            }
            return null;
        }

        public void Add(string cacheKey, DateTime absoluteExpiry, object dataToAdd)
        {
            try
            {
                var expiry = absoluteExpiry - DateTime.Now;
                var success = _db.StringSet(cacheKey, dataToAdd.Serialize(), expiry);
                if (!success)
                {
                    _logger.WriteErrorMessage(string.Format("Unable to store item in cache. CacheKey:{0}", cacheKey));
                }
            }
            catch (Exception ex)
            {
                _logger.WriteException(ex);
            }
        }

        public void Add(string cacheKey, TimeSpan slidingExpiryWindow, object dataToAdd)
        {
            try
            {
                var success = _db.StringSet(cacheKey, dataToAdd.Serialize(),slidingExpiryWindow);
                if (!success)
                {
                    _logger.WriteErrorMessage(string.Format("Unable to store item in cache. CacheKey:{0}", cacheKey));
                }
            }
            catch (Exception ex)
            {
                _logger.WriteException(ex);
            }
        }

        public void InvalidateCacheItem(string cacheKey)
        {
            _db.KeyDelete(cacheKey);
        }

        public void AddToPerRequestCache(string cacheKey, object dataToAdd)
        {
            _requestCacheHelper.AddToPerRequestCache(cacheKey, dataToAdd);
        }

        public CacheSetting CacheType
        {
            get { return CacheSetting.Redis; }
        }

        public void ClearAll()
        {
            //TODO: Figure out good way of clearing a redis instance of data
            _logger.WriteErrorMessage("Redis does not support clearing the entire contents of the cache.");
        }

        public IDatabase RedisDatabase
        {
            get { return _db; }
        }
    }
}
