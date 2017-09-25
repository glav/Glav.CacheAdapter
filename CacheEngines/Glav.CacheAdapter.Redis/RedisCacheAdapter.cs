﻿using Glav.CacheAdapter.Core;
using Glav.CacheAdapter.Core.Diagnostics;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using Glav.CacheAdapter.Helpers;
using Glav.CacheAdapter.Serialisation;

namespace Glav.CacheAdapter.Redis
{
    public class RedisCacheAdapter : ICache
    {
        private readonly ILogging _logger;
        private static IDatabase _db;
        public static ConnectionMultiplexer _connection;
        private readonly bool _cacheDependencyManagementEnabled;
        private readonly IObjectSerialiser _serialiser;

        public RedisCacheAdapter(ILogging logger, ConnectionMultiplexer redisConnection, 
            IObjectSerialiser serialiser, bool cacheDependencyManagementEnabled = false)
        {
            _logger = logger;
            _connection = redisConnection;
            _db = _connection.GetDatabase();
            _serialiser = serialiser;  // Stack Exchange does not provide serialisation, so we must do so.
            _cacheDependencyManagementEnabled = cacheDependencyManagementEnabled;

        }
        public T Get<T>(string cacheKey) where T : class
        {
            try
            {
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
                    var deserialisedObject = _serialiser.Deserialize<T>(blobBytes);
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
                var success = _db.StringSet(cacheKey, _serialiser.Serialize(dataToAdd), expiry);
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
                var success = _db.StringSet(cacheKey, _serialiser.Serialize(dataToAdd), slidingExpiryWindow);
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