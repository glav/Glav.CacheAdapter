using System;
using System.Collections.Generic;
using Glav.CacheAdapter.Core;
using Glav.CacheAdapter.DependencyManagement;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Glav.CacheAdapter.Tests
{
    [TestClass]
    public class DependencyKeyManagementTests
    {
        [TestMethod]
        public void ShouldAddSingleDependencyItem()
        {
            var mgr = new GenericDependencyManager(new MemoryCacheAdapter(), new MockLogger());
            // Make sure we start out with nothing
            mgr.ClearAssociatedDependencyList("Test");

            mgr.AssociateCacheKeyToDependentKey("Test", "Child");

            var dependencies = mgr.GetDependentCacheKeysForMasterCacheKey("Test");
            Assert.IsNotNull(dependencies, "Did not get any dependencies");
            Assert.AreEqual<int>(1, dependencies.Count());
            Assert.AreEqual<string>("Child", dependencies.FirstOrDefault().CacheKeyOrCacheGroup);
        }

        [TestMethod]
        public void ShouldAddMultipleDependencyItems()
        {
            var mgr = new GenericDependencyManager(new MemoryCacheAdapter(), new MockLogger());
            // Make sure we start out with nothing
            mgr.ClearAssociatedDependencyList("Test");

            var dependenciesToAdd = new List<string>();
            dependenciesToAdd.Add("Child1");
            dependenciesToAdd.Add("Child2");
            dependenciesToAdd.Add("Child3");
            mgr.AssociateCacheKeyToDependentKey("Test",dependenciesToAdd );

            var dependencies = mgr.GetDependentCacheKeysForMasterCacheKey("Test");
            Assert.IsNotNull(dependencies, "Did not get any dependencies");
            var dependenciesAsArray = dependencies.ToArray();
            Assert.AreEqual<int>(3, dependenciesAsArray.Length);
            Assert.AreEqual<string>("Child1", dependenciesAsArray[0].CacheKeyOrCacheGroup);
            Assert.AreEqual<string>("Child2", dependenciesAsArray[1].CacheKeyOrCacheGroup);
            Assert.AreEqual<string>("Child3", dependenciesAsArray[2].CacheKeyOrCacheGroup);
        }

        [TestMethod]
        public void ShouldAddMultipleDependencyItemsWithNoConflict()
        {
            var mgr = new GenericDependencyManager(new MemoryCacheAdapter(), new MockLogger());
            // Make sure we start out with nothing
            mgr.ClearAssociatedDependencyList("Test");

            var dependenciesToAdd = new List<string>();
            dependenciesToAdd.Add("Child1");
            dependenciesToAdd.Add("Child2");
            dependenciesToAdd.Add("Child3");
            mgr.AssociateCacheKeyToDependentKey("Test", dependenciesToAdd);

            // Now add some more items, some that are already added,some not.
            dependenciesToAdd.Clear();
            dependenciesToAdd.Add("Child10");  // should add
            dependenciesToAdd.Add("Child2");  // should not add
            dependenciesToAdd.Add("Child11");  // should add
            mgr.AssociateCacheKeyToDependentKey("Test", dependenciesToAdd);

            var dependencies = mgr.GetDependentCacheKeysForMasterCacheKey("Test");
            Assert.IsNotNull(dependencies, "Did not get any dependencies");
            var dependenciesAsArray = dependencies.ToArray();
            Assert.AreEqual<int>(5, dependenciesAsArray.Length);
            Assert.AreEqual<string>("Child1", dependenciesAsArray[0].CacheKeyOrCacheGroup);
            Assert.AreEqual<string>("Child2", dependenciesAsArray[1].CacheKeyOrCacheGroup);
            Assert.AreEqual<string>("Child3", dependenciesAsArray[2].CacheKeyOrCacheGroup);
            Assert.AreEqual<string>("Child10", dependenciesAsArray[3].CacheKeyOrCacheGroup);
            Assert.AreEqual<string>("Child11", dependenciesAsArray[4].CacheKeyOrCacheGroup);
        }

        [TestMethod]
        public void ShouldClearAssociatedDependency()
        {
            var cacheAdapter = new MemoryCacheAdapter();
            var mgr = new GenericDependencyManager(cacheAdapter, new MockLogger());
            // Make sure we start out with nothing
            mgr.ClearAssociatedDependencyList("Test");

            // Associate a dependent cachekey
            mgr.AssociateCacheKeyToDependentKey("Test", "Child");

            // Addin the master item
            cacheAdapter.Add("Test", DateTime.Now.AddDays(1), "DataBlob");
            // Add in the dependent item
            cacheAdapter.Add("Child", DateTime.Now.AddDays(1), "DataBlob2");

            // Now clear the dependencies for the master
            mgr.CheckAssociatedDependenciesAndPerformAction("Test");

            // And finally check its existence
            Assert.IsNull(cacheAdapter.Get<string>("Child"));
        }
    }
}
