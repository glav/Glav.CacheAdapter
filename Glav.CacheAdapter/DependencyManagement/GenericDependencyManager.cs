using Glav.CacheAdapter.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Glav.CacheAdapter.Core.Diagnostics;

namespace Glav.CacheAdapter.DependencyManagement
{
    /// <summary>
    /// A generic cache dependency mechanism that utilises no specific features
    /// of any cache engine and acts as overall support of rudimentary cache dependencies
    /// in light of the fact cache engines may not support advanced queries, dependencies
    /// and events
    /// </summary>
    public class GenericDependencyManager : ICacheDependencyManager
    {
        private ICache _cache;
        private ILogging _logger;
        private const string CacheKeyPrefix = "__DepMgr_"; // The root cache key prefix we use
        private const string CacheDependencyEntryPrefix = "DepEntry_"; // The additional prefix for master/child cache key dependency entries
        private const string CachePrefixKey = "PrefixEntry_";  // the additional prefix for registering cache key prefixes to monitor

        public GenericDependencyManager(ICache cache, ILogging logger)
        {
            _cache = cache;
            _logger = logger;
        }
        public void AssociateCacheKeyToDependentKey(string masterCacheKey, string dependentCacheKey, CacheDependencyAction actionToPerform = CacheDependencyAction.ClearDependentItems)
        {
            var cacheKeyForDependency = string.Format("{0}{1}{2}", CacheKeyPrefix, CacheDependencyEntryPrefix, masterCacheKey);
            var currentEntry = _cache.Get<string[]>(cacheKeyForDependency);
            if (currentEntry == null || currentEntry.Length == 0)
            {
                currentEntry = new string[] {dependentCacheKey};
            } else
            {
                var tempList = new List<string>();
                tempList.AddRange(currentEntry);
                if (!tempList.Contains(dependentCacheKey))
                {
                    tempList.Add(dependentCacheKey);
                    currentEntry = tempList.ToArray();
                }
            }
            _cache.Add(cacheKeyForDependency, GetMaxAge(), currentEntry);
        }

        public void AssociateCacheKeyToDependentKey(string masterCacheKey, IEnumerable<string> dependentCacheKeys, CacheDependencyAction actionToPerform = CacheDependencyAction.ClearDependentItems)
        {
            var cacheKeyForDependency = string.Format("{0}{1}{2}", CacheKeyPrefix, CacheDependencyEntryPrefix, masterCacheKey);
            var currentEntry = _cache.Get<string[]>(cacheKeyForDependency);
            var tempList = new List<string>();
            if (currentEntry != null && currentEntry.Length > 0)
            {
                tempList.AddRange(currentEntry);
            }
            ((List<string>) dependentCacheKeys).ForEach(d =>
                                                            {
                                                                if (!tempList.Contains(d))
                                                                {
                                                                    tempList.Add(d);
                                                                }
                                                            });
            _cache.Add(cacheKeyForDependency, GetMaxAge(), tempList.ToArray());
        }

        public IEnumerable<string> GetDependentCacheKeysForMasterCacheKey(string masterCacheKey)
        {
            var cacheKeyForDependency = string.Format("{0}{1}{2}", CacheKeyPrefix, CacheDependencyEntryPrefix, masterCacheKey);
            return _cache.Get<string[]>(cacheKeyForDependency);
        }

        public void ClearDependencies(string cacheKey)
        {
            throw new NotImplementedException();
        }

        public void RegisterDependencyPrefix(string prefix)
        {
            var cacheKeyForPrefix = string.Format("{0}{1}{2}", CacheKeyPrefix, CachePrefixKey, prefix);
            var maxAge = GetMaxAge();
            _cache.Add(cacheKeyForPrefix, maxAge, DateTime.Now);
        }


        public string Name
        {
            get { return "Generic/Default"; }
        }

        private DateTime GetMaxAge()
        {
            //Note: memcached has a 'bug' where if you store something >= 30 days, it doesn't work
            //      so we use 29 days as the max.
            return DateTime.Now.AddDays(29);
        }
    }
}
