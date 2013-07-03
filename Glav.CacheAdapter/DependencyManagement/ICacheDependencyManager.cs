using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Glav.CacheAdapter.DependencyManagement
{
    public interface ICacheDependencyManager
    {
        /// <summary>
        /// Register a parent Key. This is normally done implicitly when adding
        /// and cache item and an associated parent key.
        /// </summary>
        /// <param name="parentKey"></param>
        /// <param name="actionToPerform"></param>
        void RegisterParentItem(string parentKey, CacheDependencyAction actionToPerform = CacheDependencyAction.ClearDependentItems);
        /// <summary>
        /// Remove a parent key definition and means no dependency actions are
        /// fired when the parent key is invalidated
        /// </summary>
        /// <param name="parentKey"></param>
        void RemoveParentItem(string parentKey);
        /// <summary>
        /// Associates a series of child or dependent keys to a parent key
        /// </summary>
        /// <param name="parentKey"></param>
        /// <param name="dependentCacheKeys"></param>
        /// <param name="actionToPerform"></param>
        void AssociateDependentKeysToParent(string parentKey, IEnumerable<string> dependentCacheKeys, CacheDependencyAction actionToPerform = CacheDependencyAction.ClearDependentItems);
        /// <summary>
        /// Retrieves the list of dependent keys for a parent key. If no parent
        /// key exists, oneis created an a new list is returned
        /// </summary>
        /// <param name="parentKey"></param>
        /// <returns></returns>
        IEnumerable<DependencyItem> GetDependentCacheKeysForParent(string parentKey);
        /// <summary>
        /// Clears the list of child or dependent items in the list for a parent
        /// key but does not remove the parent key itself
        /// </summary>
        /// <param name="parentKey"></param>
        void ClearDependencyListForParent(string parentKey);
        
        string Name { get;  }
        /// <summary>
        /// Performs the associated action for every child or dependency associated
        /// with the specified parent key
        /// </summary>
        /// <param name="parentKey"></param>
        void PerformActionForDependenciesAssociatedWithParent(string parentKey);
        /// <summary>
        /// Forces a particular action to occur for every child or dependency associated
        /// with the specified parent key
        /// </summary>
        /// <param name="parentKey"></param>
        /// <param name="forcedAction"></param>
        void ForceActionForDependenciesAssociatedWithParent(string parentKey, CacheDependencyAction forcedAction);
        /// <summary>
        /// Returns true if caching is enabled, dependencies are enabled and a 
        /// non empty parent key is provided
        /// </summary>
        /// <param name="parentKey"></param>
        /// <returns></returns>
        bool IsOkToActOnDependencyKeysForParent(string parentKey);
    }

    public enum CacheDependencyAction
    {
        ClearDependentItems=0,
        RaiseEvent=1,
        ClearDependentItemsAndRaiseEvent=2
    }
}
