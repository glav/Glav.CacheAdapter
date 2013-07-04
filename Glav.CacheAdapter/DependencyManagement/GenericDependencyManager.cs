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
        public virtual void AssociateDependentKeysToParent(string parentKey, IEnumerable<string> dependentCacheKeys, CacheDependencyAction actionToPerform = CacheDependencyAction.ClearDependentItems)
        {
            _logger.WriteInfoMessage(string.Format("Associating list of cache keys to parent key:[{0}]", parentKey));

            var cacheKeyForDependency = GetParentItemCacheKey(parentKey);
            var currentEntry = _cache.Get<DependencyItem[]>(cacheKeyForDependency);
            var tempList = new List<DependencyItem>();
            if (currentEntry != null && currentEntry.Length > 0)
            {
                _logger.WriteInfoMessage(string.Format("Creating new associated dependency list for parent key:[{0}]", parentKey));

                tempList.AddRange(currentEntry);
            } else
            {
                RegisterParentDependencyDefinition(parentKey, actionToPerform);
                tempList.AddRange(_cache.Get<DependencyItem[]>(cacheKeyForDependency));
            }

            var keysList = new List<string>(dependentCacheKeys);
            keysList.ForEach(d =>
                                                            {
                                                                if (!tempList.Any(c => c.CacheKey == d))
                                                                {
                                                                    tempList.Add(new DependencyItem { CacheKey = d, Action = actionToPerform });
                                                                }
                                                            });
            _cache.InvalidateCacheItem(cacheKeyForDependency);
            _cache.Add(cacheKeyForDependency, GetMaxAge(), tempList.ToArray());
        }

        public virtual IEnumerable<DependencyItem> GetDependentCacheKeysForParent(string parentKey, bool includeParentNode = false)
        {
            _logger.WriteInfoMessage(string.Format("Retrieving associated cache key dependency list parent key:[{0}]", parentKey));

            var cacheKeyForDependency = GetParentItemCacheKey(parentKey);
            var keyList = _cache.Get<DependencyItem[]>(cacheKeyForDependency);
            if (keyList == null)
            {
                RegisterParentDependencyDefinition(parentKey);
                return FilterDependencyListForParentNode(_cache.Get<DependencyItem[]>(cacheKeyForDependency),includeParentNode);
            }

            return FilterDependencyListForParentNode(keyList,includeParentNode);
        }

        private DependencyItem[] FilterDependencyListForParentNode(DependencyItem[] dependencyList, bool includeParentNode)
        {
            var depList = new List<DependencyItem>();
            if (dependencyList != null)
            {
                depList.AddRange(dependencyList);
            }

            if (!includeParentNode)
            {
                var item = depList.FirstOrDefault(d => d.IsParentNode);
                if (item != null)
                {
                    depList.Remove(item);
                }
            }
            return depList.ToArray();
        }

        public void RegisterParentDependencyDefinition(string parentKey, CacheDependencyAction actionToPerform = CacheDependencyAction.ClearDependentItems)
        {
            _logger.WriteInfoMessage(string.Format("Registering parent item:[{0}]", parentKey));

            var cacheKeyForParent = GetParentItemCacheKey(parentKey);
            var item = new DependencyItem { CacheKey = parentKey, Action = actionToPerform, IsParentNode = true };
            var depList = new DependencyItem[] { item };
            _cache.InvalidateCacheItem(cacheKeyForParent);
            _cache.Add(cacheKeyForParent, GetMaxAge(), depList);
        }


        public virtual void RemoveParentDependencyDefinition(string parentKey)
        {
            _logger.WriteInfoMessage(string.Format("Removing parent key:[{0}]", parentKey));

            var cacheKeyForParent = GetParentItemCacheKey(parentKey);
            _cache.InvalidateCacheItem(cacheKeyForParent);
        }

        public virtual string Name
        {
            get { return "Generic/Default"; }
        }

        public virtual void PerformActionForDependenciesAssociatedWithParent(string parentKey)
        {
            _logger.WriteInfoMessage(string.Format("Performing required actions on associated dependency cache keys for parent key:[{0}]", parentKey));

            ExecuteDefaultOrSuppliedActionForParentKeyDependencies(parentKey);
        }

        private void ExecuteDefaultOrSuppliedActionForParentKeyDependencies(string parentKey, List<string> alreadyProcessedKeys = null, CacheDependencyAction? forcedAction=null)
        {
            if (!IsOkToActOnDependencyKeysForParent(parentKey))
            {
                return;
            }

            if (alreadyProcessedKeys == null)
            {
                alreadyProcessedKeys = new List<string>();
            }

            var items = GetDependentCacheKeysForParent(parentKey);
            if (items != null && items.Count() > 0)
            {
                foreach (var item in items)
                {
                    // Dont allow recursion
                    if (item.CacheKey == parentKey)
                    {
                        continue;
                    }
                    if (alreadyProcessedKeys.Contains(item.CacheKey))
                    {
                        continue;
                    }
                    var cacheItemAction = item.Action;
                    if (forcedAction.HasValue)
                    {
                        cacheItemAction = forcedAction.Value;
                    }
                    switch (cacheItemAction)
                    {
                        case CacheDependencyAction.ClearDependentItems:
                            _cache.InvalidateCacheItem(item.CacheKey);
                            // Recursively clear any dependencies as this key itself might be a parent
                            // to other items
                            break;
                        default:
                            throw new NotSupportedException(string.Format("Action [{0}] not supported at this time",cacheItemAction));
                    }
                    alreadyProcessedKeys.Add(item.CacheKey);
                    ExecuteDefaultOrSuppliedActionForParentKeyDependencies(item.CacheKey,alreadyProcessedKeys);

                }
            }
            
        }

        public virtual bool IsOkToActOnDependencyKeysForParent(string parentKey)
        {
            if (!_config.IsCacheEnabled)
            {
                return false;
            }

            if (!_config.IsCacheDependencyManagementEnabled)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(parentKey))
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


        public virtual void ForceActionForDependenciesAssociatedWithParent(string parentKey, CacheDependencyAction forcedAction)
        {
            _logger.WriteInfoMessage(string.Format("Forcing action:[{0}] on dependency cache keys for parent key:[{1}]", forcedAction.ToString(), parentKey));
            ExecuteDefaultOrSuppliedActionForParentKeyDependencies(parentKey, null,forcedAction);
        }

        private string GetParentItemCacheKey(string parentKey)
        {
            var cacheKeyForParent = string.Format("{0}{1}{2}", CacheKeyPrefix, CacheDependencyEntryPrefix, parentKey);
            return cacheKeyForParent;

        }
    }
}
