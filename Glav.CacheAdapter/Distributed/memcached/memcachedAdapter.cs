using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Enyim.Caching;
using Enyim.Caching.Memcached;
using Glav.CacheAdapter.Core;
using Glav.CacheAdapter.Core.Diagnostics;
using Glav.CacheAdapter.Web;

namespace Glav.CacheAdapter.Distributed.memcached
{
	public class memcachedAdapter : ICache
	{
		private CacheServerFarm _serverFarm;
		private ILogging _logger;
        private PerRequestCacheHelper _requestCacheHelper = new PerRequestCacheHelper();

		private static Enyim.Caching.IMemcachedClient _client;
		private static object _lockRef = new object();
		private static bool _isInitialised = false;


        public memcachedAdapter(ILogging logger, CacheConfig config = null)
		{
			_logger = logger;

			var factory = new memcachedCacheFactory(_logger,config);
			_serverFarm = factory.ConstructCacheFarm();

			if (_serverFarm == null || _serverFarm.NodeList == null || _serverFarm.NodeList.Count == 0)
				throw new ArgumentException("Must specify at least 1 server node to use for memcached");

			Initialise(factory);
			LogManager.AssignFactory(new LogFactoryAdapter(_logger));
		}

		private void Initialise(memcachedCacheFactory factory)
		{
			if (!_isInitialised)
			{
				lock (_lockRef)
				{
					if (!_isInitialised)
					{
						_isInitialised = true;
						// If the consumer of this class has passed in a IMemcachedClient
						//instance, then we use that instead
						if (_client != null)
						{
							return;
						}
						var config = new Enyim.Caching.Configuration.MemcachedClientConfiguration();
						_serverFarm.NodeList.ForEach(n => config.AddServer(n.IPAddressOrHostName,n.Port));
						config.SocketPool.ConnectionTimeout = factory.ConnectTimeout;
						config.SocketPool.DeadTimeout = factory.DeadNodeTimeout;

                        config.SocketPool.MaxPoolSize = factory.MaximumPoolSize;
						config.SocketPool.MinPoolSize = factory.MinimumPoolSize;
						
						config.Protocol = MemcachedProtocol.Text;
						config.Transcoder = new DataContractTranscoder();
						_client = new MemcachedClient(config);
						_logger.WriteInfoMessage("memcachedAdapter initialised.");
					}
				}
			}
		}


		public T Get<T>(string cacheKey) where T : class
		{
			try
			{
				var sanitisedKey = SanitiseCacheKey(cacheKey);
                // try per request cache first, but only if in a web context
                var requestCacheData = _requestCacheHelper.TryGetItemFromPerRequestCache<T>(cacheKey);
                if (requestCacheData != null)
                {
                    return requestCacheData;
                }

                return _client.Get<T>(sanitisedKey);
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
				var sanitisedKey = SanitiseCacheKey(cacheKey);
				var success = _client.Store(StoreMode.Set, sanitisedKey, dataToAdd, absoluteExpiry);
				if (!success)
				{
					_logger.WriteErrorMessage(string.Format("Unable to store item in cache. CacheKey:{0}",sanitisedKey));
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
				var sanitisedKey = SanitiseCacheKey(cacheKey);
				var success = _client.Store(StoreMode.Set, sanitisedKey, dataToAdd, slidingExpiryWindow);
				if (!success)
				{
					_logger.WriteErrorMessage(string.Format("Unable to store item in cache. CacheKey:{0}", sanitisedKey));
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
				var sanitisedKey = SanitiseCacheKey(cacheKey);
				var success = _client.Remove(sanitisedKey);
				if (!success)
				{
					_logger.WriteErrorMessage(string.Format("Unable to remove item from cache. CacheKey:{0}",sanitisedKey));
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
            foreach (var cacheKey in cacheKeys)
            {
                InvalidateCacheItem(cacheKey);
            }
        }


		public void AddToPerRequestCache(string cacheKey, object dataToAdd)
		{
            _requestCacheHelper.AddToPerRequestCache(cacheKey, dataToAdd);
		}

		public CacheSetting CacheType
		{
			get { return CacheSetting.memcached; }
		}

		private string SanitiseCacheKey(string cacheKey)
		{
			if (string.IsNullOrWhiteSpace(cacheKey))
			{
				throw new ArgumentException("Cannot have an empty or NULL cache key");
			}
			return cacheKey.Replace(" ", string.Empty).Replace("#","-");
		}


        public void ClearAll()
        {
            _logger.WriteInfoMessage("Clearing the cache");
            try
            {
                _client.FlushAll();
            } catch (Exception ex)
            {
                _logger.WriteErrorMessage("Error flushing the cache:" + ex.Message);
            }
        }
    }
}
