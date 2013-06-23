using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Glav.CacheAdapter.DependencyManagement
{
    public interface ICacheDependencyManager
    {
        void AssociateCacheKeyToDependentKey(string masterCacheKey, string dependentCacheKey, CacheDependencyAction actionToPerform = CacheDependencyAction.ClearDependentItems);
        void AssociateCacheKeyToDependentKey(string masterCacheKey, IEnumerable<string> dependentCacheKeys, CacheDependencyAction actionToPerform = CacheDependencyAction.ClearDependentItems);
        IEnumerable<string> GetDependentCacheKeysForMasterCacheKey(string cacheKey);
        void ClearAssociatedDependencies(string masterCacheKey);
        /// <summary>
        /// Register a cache key prefix such that when an item with a specific prefix
        /// is removed, everything the cache knows about with the prefix in the key is
        /// also removed
        /// </summary>
        /// <param name="prefix"></param>
        void RegisterDependencyPrefix(string prefix);
        string Name { get;  }
    }

    public enum CacheDependencyAction
    {
        ClearDependentItems=0,
        RaiseEvent=1,
        ClearDependentItemsAndRaiseEvent=2
    }
}
