using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Glav.CacheAdapter.DependencyManagement;

namespace Glav.CacheAdapter.Core
{
    /// <summary>
    /// The primary way of dealing with the underlying cache engines. This is a high level interface
    /// providing value added functions that make working with the cache easy with a lot less code.
    /// </summary>
    public interface ICacheProvider
    {
        T Get<T>(string cacheKey, DateTime absoluteExpiryDate, Func<T> getData, string parentKey=null, CacheDependencyAction actionForDependency= CacheDependencyAction.ClearDependentItems) where T : class;
		T Get<T>(string cacheKey, TimeSpan slidingExpiryWindow, Func<T> getData,string parentKey = null, CacheDependencyAction actionForDependency = CacheDependencyAction.ClearDependentItems) where T : class;
        T Get<T>(DateTime absoluteExpiryDate, Func<T> getData,string parentKey = null, CacheDependencyAction actionForDependency = CacheDependencyAction.ClearDependentItems) where T : class;
        T Get<T>(TimeSpan slidingExpiryWindow, Func<T> getData,string parentKey = null, CacheDependencyAction actionForDependency = CacheDependencyAction.ClearDependentItems) where T : class;
        void InvalidateCacheItem(string cacheKey);
    	void Add(string cacheKey, DateTime absoluteExpiryDate, object dataToAdd, string parentKey=null, CacheDependencyAction actionForDependency= CacheDependencyAction.ClearDependentItems);
		void Add(string cacheKey, TimeSpan slidingExpiryWindow, object dataToAdd, string parentKey=null, CacheDependencyAction actionForDependency= CacheDependencyAction.ClearDependentItems);
    	void AddToPerRequestCache(string cacheKey, object dataToAdd);
		ICache InnerCache { get; }

        // Dependency Management API
        ICacheDependencyManager InnerDependencyManager { get;  }
        // Convenience methods for dependency management
        void InvalidateDependenciesForParent(string parentKey);
    }
}
