using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Glav.CacheAdapter.Core.Diagnostics;
using Glav.CacheAdapter.DependencyManagement;

namespace Glav.CacheAdapter.Core
{
	/// <summary>
	/// This class acts as a cache provider that will attempt to retrieve items from a cache, and if they do not exist,
	/// execute the passed in delegate to perform a data retrieval, then place the item into the cache before returning it.
	/// Subsequent accesses will get the data from the cache until it expires.
	/// </summary>
	public class CacheProvider : ICacheProvider
	{
		private readonly ICache _cache;
		private readonly ILogging _logger;
		private CacheConfig _config = new CacheConfig();
	    private readonly ICacheDependencyManager _cacheDependencyManager;

		public CacheProvider(ICache cache, ILogging logger)
		{
			_cache = cache;
			_logger = logger;
            if (_config.IsCacheKeysDependeniesEnabled || _config.IsCachePrefixDependenciesEnabled)
            {
                // Dependencies are enabled but the default constructor was used (without
                // specifying a dependency manager) so we instantiate the default.
                _cacheDependencyManager = new GenericDependencyManager(_cache, _logger);
                _logger.WriteInfoMessage(string.Format("CacheKey dependency management enabled but no dependency manager specified so using {0}.",_cacheDependencyManager.Name));
            } else
            {
                _cacheDependencyManager = null;  // Dependency Management is disabled
                _logger.WriteInfoMessage("CacheKey dependency management not enabled.");
            }
		}
        public CacheProvider(ICache cache, ILogging logger, ICacheDependencyManager cacheDependencyManager)
        {
            _cache = cache;
            _logger = logger;
            _cacheDependencyManager = cacheDependencyManager;
            _logger.WriteInfoMessage(string.Format("CacheKey dependency management enabled, using {0}.", _cacheDependencyManager.Name));
        }
        
        #region ICacheProvider Members

		public ICache InnerCache { get { return _cache; }}

		public T Get<T>(string cacheKey, DateTime expiryDate, Func<T> getData) where T : class
		{
			//Get data from cache
			T data = GetData(cacheKey, getData);
			//only add non null data to the cache.
			if (data != null)
			{
				_cache.Add(cacheKey, expiryDate, data);
				_logger.WriteInfoMessage(string.Format("Adding item [{0}] to cache with expiry date/time of [{1}].", cacheKey,
													   expiryDate.ToString("dd/MM/yyyy hh:mm:ss")));
			}
			return data;
		}

		public T Get<T>(string cacheKey, TimeSpan slidingExpiryWindow, Func<T> getData) where T : class
		{
			//Get data from cacheif it is enabled
			T data = GetData(cacheKey, getData);
			//only add non null data to the cache.
			if (data != null && _config.IsCacheEnabled)
			{
				_cache.Add(cacheKey, slidingExpiryWindow, data);
				_logger.WriteInfoMessage(
					string.Format("Adding item [{0}] to cache with sliding sliding expiry window in seconds [{1}].", cacheKey,
								  slidingExpiryWindow.TotalSeconds));
			}
			return data;
		}

		private T GetData<T>(string cacheKey, Func<T> getData) where T : class
		{
			T data = _config.IsCacheEnabled ? _cache.Get<T>(cacheKey) : null;
			if (data == null)
			{
				//get data from source
				data = getData();
			}
			else
			{
				_logger.WriteInfoMessage(string.Format("Retrieving item [{0}] from cache.", cacheKey));
			}

			return data;
		}

		public void InvalidateCacheItem(string cacheKey)
		{
			if (!_config.IsCacheEnabled)
			{
				return;
			}
			_cache.InvalidateCacheItem(cacheKey);
		}

		#endregion


		public void Add(string cacheKey, DateTime absoluteExpiryDate, object dataToAdd)
		{
			if (!_config.IsCacheEnabled)
			{
				return;
			}
			_cache.Add(cacheKey, absoluteExpiryDate, dataToAdd);
		}

		public void Add(string cacheKey, TimeSpan slidingExpiryWindow, object dataToAdd)
		{
			if (!_config.IsCacheEnabled)
			{
				return;
			}
			_cache.Add(cacheKey, slidingExpiryWindow, dataToAdd);
		}

		public void AddToPerRequestCache(string cacheKey, object dataToAdd)
		{
			if (!_config.IsCacheEnabled)
			{
				return;
			}
			_cache.AddToPerRequestCache(cacheKey, dataToAdd);
		}


		public T Get<T>(DateTime absoluteExpiryDate, Func<T> getData) where T : class
		{
			return Get<T>(GetCacheKeyFromFuncDelegate(getData), absoluteExpiryDate, getData);
		}

		public T Get<T>(TimeSpan slidingExpiryWindow, Func<T> getData) where T : class
		{
			return Get<T>(GetCacheKeyFromFuncDelegate(getData), slidingExpiryWindow, getData);
		}

		private string GetCacheKeyFromFuncDelegate<T>(Func<T> getData) where T : class
		{
			return getData.Method.DeclaringType.FullName + "-" + getData.Method.Name;
		}
	}
}
