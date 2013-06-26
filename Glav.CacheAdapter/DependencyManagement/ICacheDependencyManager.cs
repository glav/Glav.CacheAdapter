using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Glav.CacheAdapter.DependencyManagement
{
    public interface ICacheDependencyManager
    {
        void AssociateDependentKeyToMasterCacheKey(string masterCacheKey, string dependentCacheKey, CacheDependencyAction actionToPerform = CacheDependencyAction.ClearDependentItems);
        void AssociateDependentKeysToMasterCacheKey(string masterCacheKey, IEnumerable<string> dependentCacheKeys, CacheDependencyAction actionToPerform = CacheDependencyAction.ClearDependentItems);
        IEnumerable<DependencyItem> GetDependentCacheKeysForMasterCacheKey(string cacheKey);
        void ClearAssociatedDependencyList(string masterCacheKey);
        
        /// <summary>
        /// Register a cache key group name such that items can be associated with
        /// a group. When the group is signalled to be removed everything the cache 
        /// knows about associated with the group is also removed
        /// </summary>
        void RegisterDependencyGroup(string groupName, CacheDependencyAction actionToPerform = CacheDependencyAction.ClearDependentItems);
        void RemoveDependencyGroup(string groupName);
        IEnumerable<DependencyItem> GetDependencyGroup(string groupName);
        void AddCacheKeyToDependencyGroup(string groupName, string cacheKey, CacheDependencyAction actionToPerform = CacheDependencyAction.ClearDependentItems);
        string Name { get;  }
        void CheckGroupDependenciesAndPerformAction(string groupName);
        void CheckAssociatedDependenciesAndPerformAction(string masterCacheKey);
    }

    public enum CacheDependencyAction
    {
        ClearDependentItems=0,
        RaiseEvent=1,
        ClearDependentItemsAndRaiseEvent=2
    }
}
