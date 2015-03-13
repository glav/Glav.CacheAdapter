using Glav.CacheAdapter.Core;
using Glav.CacheAdapter.Core.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Glav.CacheAdapter.DependencyManagement
{
    public abstract class BaseCacheDependencyManager : ICacheDependencyManager
    {
        private ICache _cache;
        private ILogging _logger;
        private CacheConfig _config;

        public BaseCacheDependencyManager(ICache cache, ILogging logger, CacheConfig config = null)
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

        public CacheConfig Config { get { return _config;  } }
        public ICache Cache { get { return _cache; } }
        public ILogging Logger { get { return _logger; } }

        public abstract void RegisterParentDependencyDefinition(string parentKey, CacheDependencyAction actionToPerform = CacheDependencyAction.ClearDependentItems);

        public abstract void RemoveParentDependencyDefinition(string parentKey);

        public abstract void AssociateDependentKeysToParent(string parentKey, IEnumerable<string> dependentCacheKeys, CacheDependencyAction actionToPerform = CacheDependencyAction.ClearDependentItems);

        public abstract IEnumerable<DependencyItem> GetDependentCacheKeysForParent(string parentKey, bool includeParentNode = false);

        public abstract string Name { get; }

        public virtual void PerformActionForDependenciesAssociatedWithParent(string parentKey)
        {
            Logger.WriteInfoMessage(string.Format("Performing required actions on associated dependency cache keys for parent key:[{0}]", parentKey));

            ExecuteDefaultOrSuppliedActionForParentKeyDependencies(parentKey);
        }


        public virtual void ForceActionForDependenciesAssociatedWithParent(string parentKey, CacheDependencyAction forcedAction)
        {
            Logger.WriteInfoMessage(string.Format("Forcing action:[{0}] on dependency cache keys for parent key:[{1}]", forcedAction.ToString(), parentKey));
            ExecuteDefaultOrSuppliedActionForParentKeyDependencies(parentKey, forcedAction);
        }


        protected virtual void ExecuteDefaultOrSuppliedActionForParentKeyDependencies(string parentKey, CacheDependencyAction? forcedAction = null)
        {
            if (!IsOkToActOnDependencyKeysForParent(parentKey))
            {
                return;
            }
            Logger.WriteInfoMessage(string.Format("executing action on dependency cache keys for parent key:[{0}]", parentKey));

            var alreadyProcessedKeys = new List<string>();

            var itemsToAction = GetCacheKeysToActionForParentKeyDependencies(parentKey, alreadyProcessedKeys);
            var itemsToClear = new List<string>();
            itemsToAction.ForEach(item =>
            {
                var cacheItemAction = item.Action;
                if (forcedAction.HasValue)
                {
                    cacheItemAction = forcedAction.Value;
                }
                switch (cacheItemAction)
                {
                    case CacheDependencyAction.ClearDependentItems:
                        itemsToClear.Add(item.CacheKey);
                        break;
                    default:
                        throw new NotSupportedException(string.Format("Action [{0}] not supported at this time", cacheItemAction));
                }
            });
            if (itemsToClear.Count > 0)
            {
                _cache.InvalidateCacheItems(itemsToClear);
            }
        }

        protected virtual List<DependencyItem> GetCacheKeysToActionForParentKeyDependencies(string parentKey, List<string> alreadyProcessedKeys = null)
        {
            var cacheKeysToAction = new List<DependencyItem>();

            if (!IsOkToActOnDependencyKeysForParent(parentKey))
            {
                return cacheKeysToAction;
            }

            if (alreadyProcessedKeys == null)
            {
                alreadyProcessedKeys = new List<string>();
            }



            var items = GetDependentCacheKeysForParent(parentKey);
            var numItems = items != null ? items.Count() : 0;
            _logger.WriteInfoMessage(string.Format("Number of dependencies found for master cache key [{0}] is: {1}", parentKey, numItems));
            if (numItems > 0)
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
                    cacheKeysToAction.Add(item);
                    alreadyProcessedKeys.Add(item.CacheKey);
                    cacheKeysToAction.AddRange(GetCacheKeysToActionForParentKeyDependencies(item.CacheKey,alreadyProcessedKeys));
                }
            }
            return cacheKeysToAction;
        }


        public bool IsOkToActOnDependencyKeysForParent(string parentKey)
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
    }
}
