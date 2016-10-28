using Glav.CacheAdapter.Core;
using Glav.CacheAdapter.Core.Diagnostics;
using Glav.CacheAdapter.Web;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using Glav.CacheAdapter.Helpers;

namespace Glav.CacheAdapter.Distributed.Redis
{
    public class RedisCacheAdapter : ICache
    {
        private readonly ILogging _logger;
        private readonly RedisCacheFactory _factory;
        private readonly PerRequestCacheHelper _requestCacheHelper = new PerRequestCacheHelper();
        private static IDatabase _db;
        public static ConnectionMultiplexer _connection;
        private readonly CacheConfig _config;
        private readonly bool _cacheDependencyManagementEnabled;

        public RedisCacheAdapter(ILogging logger, ConnectionMultiplexer redisConnection, bool cacheDependencyManagementEnabled = false)
        {
            _logger = logger;
            _connection = redisConnection;
            _db = _connection.GetDatabase();
            _cacheDependencyManagementEnabled = cacheDependencyManagementEnabled;

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
                if (_cacheDependencyManagementEnabled && _db.KeyType(cacheKey) == RedisType.List)
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
                var success = _db.StringSet(cacheKey, dataToAdd.Serialize(), slidingExpiryWindow);
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
            try
            {
                var success = _db.KeyDelete(cacheKey);
                if (!success)
                {
                    _logger.WriteErrorMessage(string.Format("Unable to remove item from cache. CacheKey:{0}", cacheKey));
                }
            }
            catch (Exception ex)
            {
                _logger.WriteException(ex);
            }
        }

        public void InvalidateCacheItems(IEnumerable<string> cacheKeys)
        {
            if (cacheKeys == null)
            {
                return;
            }
            _logger.WriteInfoMessage("Invalidating a series of cache keys");
            var distinctKeys = cacheKeys.Distinct();

            try
            {
                var redisKeyList = distinctKeys.Select(s => (RedisKey)s);

                _db.KeyDelete(redisKeyList.ToArray(), CommandFlags.FireAndForget);
            }
            catch (Exception ex)
            {
                _logger.WriteException(ex);
            }


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
            _logger.WriteInfoMessage("Clearing the cache");
            var allEndpoints = _connection.GetEndPoints();
            if (allEndpoints != null && allEndpoints.Length > 0)
            {
                foreach (var endpoint in allEndpoints)
                {
                    bool flushWorked;
                    var server = _connection.GetServer(endpoint);
                    try
                    {
                        server.FlushAllDatabases();
                        flushWorked = true;
                    }
                    catch (Exception ex)
                    {
                        _logger.WriteErrorMessage("Error flushing the cache (using FlushAllDatabases method):" + ex.Message);
                        flushWorked = false;
                    }

                    if (!flushWorked)
                    {
                        _logger.WriteErrorMessage("Flushing the database did not work (probably due to requiring admin privileges), attempting to delete all keys");

                        try
                        {
                            var allKeys = server.Keys();
                            _db.KeyDelete(allKeys.ToArray(), CommandFlags.FireAndForget);
                        }
                        catch (Exception ex)
                        {
                            _logger.WriteErrorMessage("Error flushing the cache (using delete keys method):" + ex.Message);
                        }
                    }
                }
            }
        }

        public IDatabase RedisDatabase
        {
            get { return _db; }
        }
    }
}
