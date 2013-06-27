using Glav.CacheAdapter.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Glav.CacheAdapter.Core.Diagnostics;
using Glav.CacheAdapter.Distributed.memcached;

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
        public const string CacheKeyPrefix = "__DepMgr_"; // The root cache key prefix we use
        public const string CacheDependencyEntryPrefix = "DepEntry_"; // The additional prefix for master/child cache key dependency entries
        public const string CacheGroupKey = "GroupEntry_";  // the additional prefix for registering cache key prefixes to monitor
        private CacheConfig _config;

        public GenericDependencyManager(ICache cache, ILogging logger, CacheConfig config = null)
        {
            _cache = cache;
            _logger = logger;
            if (config == null)
            {
                _config = new CacheConfig();
            }
            else
            {
                _config = config;
            }
        }
        public void AssociateDependentKeyToMasterCacheKey(string masterCacheKey, string dependentCacheKey, CacheDependencyAction actionToPerform = CacheDependencyAction.ClearDependentItems)
        {
            _logger.WriteInfoMessage(string.Format("Associating cache key:[{0}] to master cache key:[{0}]", dependentCacheKey, masterCacheKey));

            var cacheKeyForDependency = string.Format("{0}{1}{2}", CacheKeyPrefix, CacheDependencyEntryPrefix, masterCacheKey);
            var currentEntry = _cache.Get<DependencyItem[]>(cacheKeyForDependency);
            List<DependencyItem> tempList = new List<DependencyItem>();
            if (currentEntry == null || currentEntry.Length == 0)
            {
                _logger.WriteInfoMessage(string.Format("Creating new associated dependency list for master cache key:[{0}]", masterCacheKey));

                tempList.Add(new DependencyItem { CacheKeyOrCacheGroup = dependentCacheKey, Action = actionToPerform });
                //currentEntry = new string[] {dependentCacheKey};
            }
            else
            {
                tempList.AddRange(currentEntry);
                if (!tempList.Any(d => d.CacheKeyOrCacheGroup == dependentCacheKey))
                {
                    tempList.Add(new DependencyItem { CacheKeyOrCacheGroup = dependentCacheKey, Action = actionToPerform });
                }
            }
            _cache.InvalidateCacheItem(cacheKeyForDependency);
            _cache.Add(cacheKeyForDependency, GetMaxAge(), tempList.ToArray());
        }

        public void AssociateDependentKeysToMasterCacheKey(string masterCacheKey, IEnumerable<string> dependentCacheKeys, CacheDependencyAction actionToPerform = CacheDependencyAction.ClearDependentItems)
        {
            _logger.WriteInfoMessage(string.Format("Associating list of cache keys to master cache key:[{0}]", masterCacheKey));

            var cacheKeyForDependency = string.Format("{0}{1}{2}", CacheKeyPrefix, CacheDependencyEntryPrefix, masterCacheKey);
            var currentEntry = _cache.Get<DependencyItem[]>(cacheKeyForDependency);
            var tempList = new List<DependencyItem>();
            if (currentEntry != null && currentEntry.Length > 0)
            {
                _logger.WriteInfoMessage(string.Format("Creating new associated dependency list for master cache key:[{0}]", masterCacheKey));

                tempList.AddRange(currentEntry);
            }

            var keysList = new List<string>(dependentCacheKeys);
            keysList.ForEach(d =>
                                                            {
                                                                if (!tempList.Any(c => c.CacheKeyOrCacheGroup == d))
                                                                {
                                                                    tempList.Add(new DependencyItem { CacheKeyOrCacheGroup = d, Action = actionToPerform });
                                                                }
                                                            });
            _cache.InvalidateCacheItem(cacheKeyForDependency);
            _cache.Add(cacheKeyForDependency, GetMaxAge(), tempList.ToArray());
        }

        public IEnumerable<DependencyItem> GetDependentCacheKeysForMasterCacheKey(string masterCacheKey)
        {
            _logger.WriteInfoMessage(string.Format("Retrieving associated cache key dependency list master cache key:[{0}]", masterCacheKey));

            var cacheKeyForDependency = string.Format("{0}{1}{2}", CacheKeyPrefix, CacheDependencyEntryPrefix, masterCacheKey);
            return _cache.Get<DependencyItem[]>(cacheKeyForDependency);
        }

        public void ClearDependencyListForMasterCacheKey(string masterCacheKey)
        {
            _logger.WriteInfoMessage(string.Format("Clearing associated dependency list for master cache key:[{0}]", masterCacheKey));

            var cacheKeyForDependency = string.Format("{0}{1}{2}", CacheKeyPrefix, CacheDependencyEntryPrefix, masterCacheKey);
            _cache.InvalidateCacheItem(cacheKeyForDependency);
        }

        public void RegisterDependencyGroup(string groupName, CacheDependencyAction actionToPerform = CacheDependencyAction.ClearDependentItems)
        {
            _logger.WriteInfoMessage(string.Format("Registering dependency group:[{0}]", groupName));

            var cacheKeyForPrefix = string.Format("{0}{1}{2}", CacheKeyPrefix, CacheGroupKey, groupName);
            if (_cache.Get<DependencyItem[]>(cacheKeyForPrefix) == null)
            {
                var item = new DependencyItem { CacheKeyOrCacheGroup = groupName, Action = actionToPerform };
                var depList = new DependencyItem[] { item };
                _cache.InvalidateCacheItem(cacheKeyForPrefix);
                _cache.Add(cacheKeyForPrefix, GetMaxAge(), depList);
            }
        }

        public IEnumerable<DependencyItem> GetDependencyGroup(string groupName)
        {
            _logger.WriteInfoMessage(string.Format("Retrieving dependency group:[{0}]", groupName));

            var cacheKeyForGroup = string.Format("{0}{1}{2}", CacheKeyPrefix, CacheGroupKey, groupName);
            var cacheDependencyGroup = _cache.Get<DependencyItem[]>(cacheKeyForGroup);
            if (cacheDependencyGroup == null)
            {
                RegisterDependencyGroup(groupName);
                cacheDependencyGroup = _cache.Get<DependencyItem[]>(cacheKeyForGroup);
            }
            return cacheDependencyGroup;
        }

        public void AddCacheKeyToDependencyGroup(string groupName, string cacheKey, CacheDependencyAction actionToPerform = CacheDependencyAction.ClearDependentItems)
        {
            _logger.WriteInfoMessage(string.Format("Adding cache key:[{0}] to dependency group:[{1}]", cacheKey, groupName));

            var cacheGroup = GetDependencyGroup(groupName);
            if (cacheGroup.Any(d => d.CacheKeyOrCacheGroup == cacheKey))
            {
                //Already exists in group so do nothing
                return;
            }

            var cacheKeyForGroup = string.Format("{0}{1}{2}", CacheKeyPrefix, CacheGroupKey, groupName);
            var tempGroup = new List<DependencyItem>(cacheGroup);
            tempGroup.Add(new DependencyItem { CacheKeyOrCacheGroup = cacheKey, Action = actionToPerform });
            _cache.InvalidateCacheItem(cacheKeyForGroup);
            _cache.Add(cacheKeyForGroup, GetMaxAge(), tempGroup.ToArray());
        }

        public void RemoveDependencyGroup(string groupName)
        {
            _logger.WriteInfoMessage(string.Format("Removing dependency group:[{0}]", groupName));

            var cacheKeyForGroup = string.Format("{0}{1}{2}", CacheKeyPrefix, CacheGroupKey, groupName);
            _cache.InvalidateCacheItem(cacheKeyForGroup);
        }

        public string Name
        {
            get { return "Generic/Default"; }
        }

        public void PerformActionForGroupDependencies(string groupName)
        {
            _logger.WriteInfoMessage(string.Format("Performing required action for dependency group:[{0}]", groupName));

            ExecuteDefaultOrSuppliedActionForGroupDependencies(groupName);
        }

        private void ExecuteDefaultOrSuppliedActionForGroupDependencies(string groupName, CacheDependencyAction? forcedAction = null)
        {
            if (!IsOkToActOnGroupDependency(groupName))
            {
                return;
            }

            var cacheGroup = GetDependencyGroup(groupName);
            if (cacheGroup == null)
            {
                return;
            }

            var tempList = new List<DependencyItem>(cacheGroup);
            tempList.ForEach(item =>
            {
                if (item.CacheKeyOrCacheGroup == groupName)
                {
                    return;
                }
                var cacheItemAction = item.Action;
                // if a forced action was supplied, use that instead of the 
                // stored cache items action
                if (forcedAction.HasValue)
                {
                    cacheItemAction = forcedAction.Value;
                }
                switch (cacheItemAction)
                {
                    case CacheDependencyAction.ClearDependentItems:
                        _cache.InvalidateCacheItem(item.CacheKeyOrCacheGroup);
                        break;
                    case CacheDependencyAction.ClearDependentItemsAndRaiseEvent:
                        //do nothing-not supported yet
                        break;
                    case CacheDependencyAction.RaiseEvent:
                        //do nothing-not supported yet
                        break;
                }
            });
            
        }

        private bool IsOkToActOnGroupDependency(string groupName)
        {
            if (string.IsNullOrWhiteSpace(groupName))
            {
                return false;
            }

            if (!_config.IsCacheEnabled)
            {
                return false;
            }

            if (!_config.IsCacheGroupDependenciesEnabled)
            {
                return false;
            }
            return true;   
        }

        public void PerformActionForAssociatedDependencyKeys(string masterCacheKey)
        {
            _logger.WriteInfoMessage(string.Format("Performing required actions on associated dependency cache keys for master cache key:[{0}]", masterCacheKey));

            ExecuteDefaultOrSuppliedActionForMasterCacheKeyAssociatedDependencies(masterCacheKey);
        }

        private void ExecuteDefaultOrSuppliedActionForMasterCacheKeyAssociatedDependencies(string masterCacheKey, CacheDependencyAction? forcedAction=null)
        {
            if (!IsOkToActOnAssociatedDependencyKeysForMasterCacheKey(masterCacheKey))
            {
                return;
            }

            var items = GetDependentCacheKeysForMasterCacheKey(masterCacheKey);
            if (items != null && items.Count() > 0)
            {
                foreach (var item in items)
                {
                    var cacheItemAction = item.Action;
                    if (forcedAction.HasValue)
                    {
                        cacheItemAction = forcedAction.Value;
                    }
                    switch (cacheItemAction)
                    {
                        case CacheDependencyAction.ClearDependentItems:
                            _cache.InvalidateCacheItem(item.CacheKeyOrCacheGroup);
                            break;
                        default:
                            throw new NotSupportedException(string.Format("Action [{0}] not supported at this time"));
                    }
                }
            }
            
        }

        private bool IsOkToActOnAssociatedDependencyKeysForMasterCacheKey(string masterCacheKey)
        {
            if (!_config.IsCacheEnabled)
            {
                return false;
            }

            if (!_config.IsCacheKeysDependenciesEnabled)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(masterCacheKey))
            {
                return false;
            }
            return true;
        }

        private DateTime GetMaxAge()
        {
            var usingMemcached = _cache is memcachedAdapter;
            if (usingMemcached)
            {
                //Note: memcached has a 'bug' where if you store something >= 30 days, it doesn't work
                //      so we use 29 days as the max.
                return DateTime.Now.AddDays(29);
            }
            else
            {
                return DateTime.Now.AddYears(99);
            }
        }


        public void ForceActionForGroupDependencies(string groupName, CacheDependencyAction forcedAction)
        {
            _logger.WriteInfoMessage(string.Format("Forcing action:[{0}] on items in dependency group:[{1}]", forcedAction.ToString(), groupName));
            ExecuteDefaultOrSuppliedActionForGroupDependencies(groupName, forcedAction);
        }

        public void ForceActionForAssociatedDependencyKeys(string masterCacheKey, CacheDependencyAction forcedAction)
        {
            _logger.WriteInfoMessage(string.Format("Forcing action:[{0}] on associated dependency cache keys for master cache key:[{1}]", forcedAction.ToString(), masterCacheKey));
            ExecuteDefaultOrSuppliedActionForMasterCacheKeyAssociatedDependencies(masterCacheKey, forcedAction);
        }
    }
}
