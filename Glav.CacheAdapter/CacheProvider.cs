﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Glav.CacheAdapter.Core.Diagnostics;
using Glav.CacheAdapter.DependencyManagement;
using Glav.CacheAdapter.Features;

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
        private ICacheFeatureSupport _featureSupport;

        public CacheProvider(ICache cache, ILogging logger)
            : this(cache, logger, null, null)
        {
        }
        public CacheProvider(ICache cache, ILogging logger, ICacheDependencyManager cacheDependencyManager)
            : this(cache, logger, cacheDependencyManager, null)
        {
        }

        public CacheProvider(ICache cache, ILogging logger, ICacheDependencyManager cacheDependencyManager, ICacheFeatureSupport featureSupport)
        {
            _cache = cache;
            _logger = logger;
            _featureSupport = featureSupport;
            if (_featureSupport == null)
            {
                _featureSupport = new CacheFeatureSupport(cache);
            }
            if (_config.IsCacheDependencyManagementEnabled)
            {
                _cacheDependencyManager = cacheDependencyManager;
                if (_cacheDependencyManager == null)
                {
                    _cacheDependencyManager = new GenericDependencyManager(_cache, _logger);
                }
                _logger.WriteInfoMessage(string.Format("CacheKey dependency management enabled, using {0}.", _cacheDependencyManager.Name));
            }
            else
            {
                _cacheDependencyManager = null;  // Dependency Management is disabled
                _logger.WriteInfoMessage("CacheKey dependency management not enabled.");
            }

            _featureSupport.Cache = _cache;
        }


        public ICache InnerCache { get { return _cache; } }

        public CacheConfig CacheConfiguration
        {
            get { return _config; }
        }

        public T Get<T>(string cacheKey, DateTime expiryDate, Func<T> getData, string parentKey = null, CacheDependencyAction actionForDependency = CacheDependencyAction.ClearDependentItems) where T : class
        {
            return GetAndAddIfNecessary<T>(cacheKey,
                data =>
                {
                    _cache.Add(cacheKey, expiryDate, data);
                    _logger.WriteInfoMessage(string.Format("Adding item [{0}] to cache with expiry date/time of [{1}].", cacheKey,
                                                           expiryDate.ToString("dd/MM/yyyy hh:mm:ss")));
                },
                getData,
                parentKey,
                actionForDependency
                );
        }

        public T Get<T>(string cacheKey, TimeSpan slidingExpiryWindow, Func<T> getData, string parentKey = null, CacheDependencyAction actionForDependency = CacheDependencyAction.ClearDependentItems) where T : class
        {
            return GetAndAddIfNecessary<T>(cacheKey,
                data =>
                {
                    _cache.Add(cacheKey, slidingExpiryWindow, data);
                    _logger.WriteInfoMessage(
                        string.Format("Adding item [{0}] to cache with sliding sliding expiry window in seconds [{1}].", cacheKey,
                                      slidingExpiryWindow.TotalSeconds));
                },
                getData,
                parentKey,
                actionForDependency
                );
        }

        private T GetAndAddIfNecessary<T>(string cacheKey, Action<T> addData, Func<T> getData, string parentKey = null, CacheDependencyAction actionForDependency = CacheDependencyAction.ClearDependentItems) where T : class
        {
            if (!_config.IsCacheEnabled)
                return getData();

            //Get data from cache
            T data = _cache.Get<T>(cacheKey);

            // check to see if we need to get data from the source
            if (data == null)
            {
                //get data from source
                data = getData();

                //only add non null data to the cache.
                if (data != null)
                {
                    addData(data);
                    ManageCacheDependenciesForCacheItem(data, cacheKey, parentKey, actionForDependency);
                }
            }
            else
            {
                _logger.WriteInfoMessage(string.Format("Retrieving item [{0}] from cache.", cacheKey));
            }

            return data;
        }

        public void InvalidateCacheItems(IEnumerable<string> cacheKeys)
        {
            if (cacheKeys == null)
            {
                return;
            }

            if (!_config.IsCacheEnabled)
            {
                return;
            }

            var distinctKeys = cacheKeys.Distinct();

            if (_cacheDependencyManager == null)
            {
                _cache.InvalidateCacheItems(distinctKeys);
                return;
            }

            foreach (var cacheKey in distinctKeys)
            {
                if (_cacheDependencyManager.IsOkToActOnDependencyKeysForParent(cacheKey))
                {
                    try
                    {
                        _cacheDependencyManager.PerformActionForDependenciesAssociatedWithParent(cacheKey);
                    }
                    catch (Exception ex)
                    {
                        _logger.WriteErrorMessage(string.Format("Error when trying to invalidate dependencies for [{0}]", cacheKey));
                        _logger.WriteException(ex);
                    }
                }
            }

            try
            {
                _cache.InvalidateCacheItems(distinctKeys);
            } catch (Exception ex)
            {
                _logger.WriteErrorMessage("Error when trying to invalidate a series of cache keys");
                _logger.WriteException(ex);
            }
        }

        public void InvalidateCacheItem(string cacheKey)
        {
            if (!_config.IsCacheEnabled)
            {
                return;
            }

            if (_cacheDependencyManager == null)
            {
                _cache.InvalidateCacheItem(cacheKey);
                return;
            }

            if (_cacheDependencyManager.IsOkToActOnDependencyKeysForParent(cacheKey))
            {
                try
                {
                    _cacheDependencyManager.PerformActionForDependenciesAssociatedWithParent(cacheKey);
                } catch (Exception ex)
                {
                    _logger.WriteErrorMessage(string.Format("Error when trying to invalidate dependencies for [{0}]", cacheKey));
                    _logger.WriteException(ex);
                }
            }
            _cache.InvalidateCacheItem(cacheKey);
        }

        public void Add(string cacheKey, DateTime absoluteExpiryDate, object dataToAdd, string parentKey = null, CacheDependencyAction actionForDependency = CacheDependencyAction.ClearDependentItems)
        {
            if (!_config.IsCacheEnabled)
            {
                return;
            }
            _cache.Add(cacheKey, absoluteExpiryDate, dataToAdd);

            ManageCacheDependenciesForCacheItem(dataToAdd, cacheKey, parentKey, actionForDependency);
        }

        public void Add(string cacheKey, TimeSpan slidingExpiryWindow, object dataToAdd, string parentKey = null, CacheDependencyAction actionForDependency = CacheDependencyAction.ClearDependentItems)
        {
            if (!_config.IsCacheEnabled)
            {
                return;
            }
            _cache.Add(cacheKey, slidingExpiryWindow, dataToAdd);

            ManageCacheDependenciesForCacheItem(dataToAdd, cacheKey, parentKey, actionForDependency);
        }

        public void AddToPerRequestCache(string cacheKey, object dataToAdd)
        {
            if (!_config.IsCacheEnabled)
            {
                return;
            }
            _cache.AddToPerRequestCache(cacheKey, dataToAdd);
        }


        public T Get<T>(DateTime absoluteExpiryDate, Func<T> getData, string parentKey = null, CacheDependencyAction actionForDependency = CacheDependencyAction.ClearDependentItems) where T : class
        {
            return Get<T>(GetCacheKeyFromFuncDelegate(getData), absoluteExpiryDate, getData, parentKey, actionForDependency);
        }

        public T Get<T>(TimeSpan slidingExpiryWindow, Func<T> getData, string parentKey = null, CacheDependencyAction actionForDependency = CacheDependencyAction.ClearDependentItems) where T : class
        {
            return Get<T>(GetCacheKeyFromFuncDelegate(getData), slidingExpiryWindow, getData, parentKey, actionForDependency);
        }

        private string GetCacheKeyFromFuncDelegate<T>(Func<T> getData) where T : class
        {
            return getData.Method.DeclaringType.FullName + "-" + getData.Method.Name;
        }


        public ICacheDependencyManager InnerDependencyManager
        {
            get { return _cacheDependencyManager; }
        }

        private void ManageCacheDependenciesForCacheItem(object dataToAdd, string cacheKey, string parentKey, CacheDependencyAction action)
        {
            if (_cacheDependencyManager == null)
            {
                return;
            }
            if (_cacheDependencyManager.IsOkToActOnDependencyKeysForParent(parentKey) && dataToAdd != null)
            {
                _cacheDependencyManager.AssociateDependentKeysToParent(parentKey, new string[1] { cacheKey }, action);
            }

        }


        public void InvalidateDependenciesForParent(string parentKey)
        {
            if (_cacheDependencyManager == null)
            {
                return;
            }
            _cacheDependencyManager.ForceActionForDependenciesAssociatedWithParent(parentKey, CacheDependencyAction.ClearDependentItems);
        }


        public void ClearAll()
        {
            _cache.ClearAll();
        }


        public Features.ICacheFeatureSupport FeatureSupport
        {
            get { return _featureSupport; }
        }
    }
}
