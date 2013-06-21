using Glav.CacheAdapter.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Glav.CacheAdapter.Core.Diagnostics;

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

        public GenericDependencyManager(ICache cache, ILogging logger)
        {
            _cache = cache;
            _logger = logger;
        }
        public void AssociateCacheKeyToDependentKey(string masterCacheKey, string dependentCacheKey, CacheDependencyAction actionToPerform = CacheDependencyAction.ClearDependentItems)
        {
            throw new NotImplementedException();
        }

        public void AssociateCacheKeyToDependentKey(string masterCacheKey, IEnumerable<string> dependentCacheKeys, CacheDependencyAction actionToPerform = CacheDependencyAction.ClearDependentItems)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetDependentCacheKeysForMasterCacheKey(string cacheKey)
        {
            throw new NotImplementedException();
        }

        public void ClearDependencies(string cacheKey)
        {
            throw new NotImplementedException();
        }

        public void RegisterDependencyPrefix(string prefix)
        {
            throw new NotImplementedException();
        }
    }
}
