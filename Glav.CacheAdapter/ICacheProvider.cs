using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Glav.CacheAdapter.DependencyManagement;
using Glav.CacheAdapter.Features;

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
        Task<T> GetAsync<T>(string cacheKey, DateTime absoluteExpiryDate, Func<Task<T>> getData, string parentKey = null, CacheDependencyAction actionForDependency = CacheDependencyAction.ClearDependentItems) where T : class;
        Task<T> GetAsync<T>(string cacheKey, TimeSpan slidingExpiryWindow, Func<Task<T>> getData, string parentKey = null, CacheDependencyAction actionForDependency = CacheDependencyAction.ClearDependentItems) where T : class;
        Task<T> GetAsync<T>(DateTime absoluteExpiryDate, Func<Task<T>> getData, string parentKey = null, CacheDependencyAction actionForDependency = CacheDependencyAction.ClearDependentItems) where T : class;
        Task<T> GetAsync<T>(TimeSpan slidingExpiryWindow, Func<Task<T>> getData, string parentKey = null, CacheDependencyAction actionForDependency = CacheDependencyAction.ClearDependentItems) where T : class;

        void InvalidateCacheItem(string cacheKey);
        void InvalidateCacheItems(IEnumerable<string> cacheKeys);

        void Add(string cacheKey, DateTime absoluteExpiryDate, object dataToAdd, string parentKey=null, CacheDependencyAction actionForDependency= CacheDependencyAction.ClearDependentItems);
		void Add(string cacheKey, TimeSpan slidingExpiryWindow, object dataToAdd, string parentKey=null, CacheDependencyAction actionForDependency= CacheDependencyAction.ClearDependentItems);
    	void AddToPerRequestCache(string cacheKey, object dataToAdd);
        void ClearAll();  // pass through
		ICache InnerCache { get; }

        // Dependency Management API
        ICacheDependencyManager InnerDependencyManager { get;  }
        // Convenience methods for dependency management
        void InvalidateDependenciesForParent(string parentKey);

        ICacheFeatureSupport FeatureSupport { get;  }

        CacheConfig CacheConfiguration { get; }
    }
}
