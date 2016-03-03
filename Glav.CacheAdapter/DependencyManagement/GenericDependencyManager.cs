using Glav.CacheAdapter.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using Glav.CacheAdapter.Core.Diagnostics;

namespace Glav.CacheAdapter.DependencyManagement
{
    /// <summary>
    /// A generic cache dependency mechanism that utilises no specific features
    /// of any cache engine and acts as overall support of rudimentary cache dependencies
    /// in light of the fact cache engines may not support advanced queries, dependencies
    /// and events
    /// </summary>
    public class GenericDependencyManager : BaseCacheDependencyManager
    {
        public const string CacheKeyPrefix = "__DepMgr_"; // The root cache key prefix we use
        public const string CacheDependencyEntryPrefix = "DepEntry_"; // The additional prefix for master/child cache key dependency entries

        public GenericDependencyManager(ICache cache, ILogging logger, CacheConfig config = null)
            : base(cache, logger, config)
        {
        }

        /// <summary>
        /// Associate the dependent cache keys to their parent or masterkey so that when the parent is cleared, a list of dependent keys can also be cleared.
        /// IMPORTANT NOTE!!: This method is not thread safe, especially across a distributed system. If this is called concurrently by 2 different threads or processes
        /// and executes at the same time, there is a chance that the parent gets registered at the time meaning the last registration will work and one child/dependent cache
        /// key may not get associated with the parent key.
        /// </summary>
        /// <param name="parentKey"></param>
        /// <param name="dependentCacheKeys"></param>
        /// <param name="actionToPerform"></param>
        public override void AssociateDependentKeysToParent(string parentKey, IEnumerable<string> dependentCacheKeys, CacheDependencyAction actionToPerform = CacheDependencyAction.ClearDependentItems)
        {
            Logger.WriteInfoMessage(string.Format("Associating list of cache keys to parent key:[{0}]", parentKey));

            var cacheKeyForDependency = GetParentItemCacheKey(parentKey);
            var currentDependencyItems = Cache.Get<DependencyItem[]>(cacheKeyForDependency);
            var tempList = new List<DependencyItem>();

            if (currentDependencyItems != null && currentDependencyItems.Length > 0)
            {
                Logger.WriteInfoMessage(string.Format("Found cache key dependency list for parent key:[{0}]", parentKey));

                tempList.AddRange(currentDependencyItems);
            }
            else
            {
                Logger.WriteInfoMessage(string.Format("No dependency items were found for parent key [{0}].",parentKey));
                RegisterParentDependencyDefinition(parentKey, actionToPerform);
                var items = Cache.Get<DependencyItem[]>(cacheKeyForDependency);
                if (items != null)
                {
                    tempList.AddRange(items);
                }
            }

            var keysList = new List<string>(dependentCacheKeys);
            keysList.ForEach(d =>
                                                            {
                                                                if (!tempList.Any(c => c.CacheKey == d))
                                                                {
                                                                    tempList.Add(new DependencyItem { CacheKey = d, Action = actionToPerform });
                                                                    Logger.WriteInfoMessage(string.Format("Associating cache key [{0}] to its dependent parent key:[{1}]",d, parentKey));

                                                                }
                                                            });
            Cache.InvalidateCacheItem(cacheKeyForDependency);
            Cache.Add(cacheKeyForDependency, GetMaxAge(), tempList.ToArray());
        }

        public override IEnumerable<DependencyItem> GetDependentCacheKeysForParent(string parentKey, bool includeParentNode = false)
        {
            Logger.WriteInfoMessage(string.Format("Retrieving associated cache key dependency list parent key:[{0}]", parentKey));

            var cacheKeyForDependency = GetParentItemCacheKey(parentKey);
            var keyList = Cache.Get<DependencyItem[]>(cacheKeyForDependency);
            if (keyList == null)
            {
                RegisterParentDependencyDefinition(parentKey);
                return FilterDependencyListForParentNode(Cache.Get<DependencyItem[]>(cacheKeyForDependency), includeParentNode);
            }

            return FilterDependencyListForParentNode(keyList, includeParentNode);
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

        public override void RegisterParentDependencyDefinition(string parentKey, CacheDependencyAction actionToPerform = CacheDependencyAction.ClearDependentItems)
        {
            Logger.WriteInfoMessage(string.Format("Registering parent item:[{0}]", parentKey));

            var cacheKeyForParent = GetParentItemCacheKey(parentKey);
            var item = new DependencyItem { CacheKey = parentKey, Action = actionToPerform, IsParentNode = true };
            var depList = new[] { item };
            Cache.InvalidateCacheItem(cacheKeyForParent);
            Cache.Add(cacheKeyForParent, GetMaxAge(), depList);
        }


        public override void RemoveParentDependencyDefinition(string parentKey)
        {
            Logger.WriteInfoMessage(string.Format("Removing parent key:[{0}]", parentKey));

            var cacheKeyForParent = GetParentItemCacheKey(parentKey);
            Cache.InvalidateCacheItem(cacheKeyForParent);
        }

        public override string Name
        {
            get { return "Generic/Default"; }
        }

        private DateTime GetMaxAge()
        {
            // Note: Anything above 25 causes memcached to NOT store the item with an error.
            return DateTime.Now.AddYears(10);
        }

        private string GetParentItemCacheKey(string parentKey)
        {
            var cacheKeyForParent = string.Format("{0}{1}{2}", CacheKeyPrefix, CacheDependencyEntryPrefix, parentKey);
            return cacheKeyForParent;

        }
    }
}
