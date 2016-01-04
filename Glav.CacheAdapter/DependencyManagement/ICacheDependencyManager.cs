using System.Collections.Generic;

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
        void RegisterParentDependencyDefinition(string parentKey, CacheDependencyAction actionToPerform = CacheDependencyAction.ClearDependentItems);
        /// <summary>
        /// Remove a parent key definition and means no dependency actions are
        /// fired when the parent key is invalidated
        /// </summary>
        /// <param name="parentKey"></param>
        void RemoveParentDependencyDefinition(string parentKey);
        /// <summary>
        /// Associates a series of child or dependent keys to a parent key
        /// </summary>
        /// <param name="parentKey"></param>
        /// <param name="dependentCacheKeys"></param>
        /// <param name="actionToPerform"></param>
        void AssociateDependentKeysToParent(string parentKey, IEnumerable<string> dependentCacheKeys, CacheDependencyAction actionToPerform = CacheDependencyAction.ClearDependentItems);

        /// <summary>
        /// Retrieves the list of dependent keys for a parent key. If no parent
        /// key exists, oneis created an a new list is returned.Can optionally also return
        /// the parent node definition as part of the list
        /// </summary>
        /// <param name="parentKey"></param>
        /// <param name="includeParentNode"></param>
        /// <returns></returns>
        IEnumerable<DependencyItem> GetDependentCacheKeysForParent(string parentKey, bool includeParentNode = false);

        string Name { get; }
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
        ClearDependentItems = 0,
        RaiseEvent = 1,
        ClearDependentItemsAndRaiseEvent = 2
    }
}
