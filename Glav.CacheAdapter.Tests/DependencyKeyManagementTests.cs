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
            mgr.ClearAssociatedDependencies("Test");

            mgr.AssociateCacheKeyToDependentKey("Test", "Child");

            var dependencies = mgr.GetDependentCacheKeysForMasterCacheKey("Test");
            Assert.IsNotNull(dependencies, "Did not get any dependencies");
            Assert.AreEqual<int>(1, dependencies.Count());
            Assert.AreEqual<string>("Child", dependencies.FirstOrDefault());
        }

        [TestMethod]
        public void ShouldAddMultipleDependencyItems()
        {
            var mgr = new GenericDependencyManager(new MemoryCacheAdapter(), new MockLogger());
            // Make sure we start out with nothing
            mgr.ClearAssociatedDependencies("Test");

            var dependenciesToAdd = new List<string>();
            dependenciesToAdd.Add("Child1");
            dependenciesToAdd.Add("Child2");
            dependenciesToAdd.Add("Child3");
            mgr.AssociateCacheKeyToDependentKey("Test",dependenciesToAdd );

            var dependencies = mgr.GetDependentCacheKeysForMasterCacheKey("Test");
            Assert.IsNotNull(dependencies, "Did not get any dependencies");
            var dependenciesAsArray = dependencies.ToArray();
            Assert.AreEqual<int>(3, dependenciesAsArray.Length);
            Assert.AreEqual<string>("Child1", dependenciesAsArray[0]);
            Assert.AreEqual<string>("Child2", dependenciesAsArray[1]);
            Assert.AreEqual<string>("Child3", dependenciesAsArray[2]);
        }

        [TestMethod]
        public void ShouldAddMultipleDependencyItemsWithNoConflict()
        {
            var mgr = new GenericDependencyManager(new MemoryCacheAdapter(), new MockLogger());
            // Make sure we start out with nothing
            mgr.ClearAssociatedDependencies("Test");

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
            Assert.AreEqual<string>("Child1", dependenciesAsArray[0]);
            Assert.AreEqual<string>("Child2", dependenciesAsArray[1]);
            Assert.AreEqual<string>("Child3", dependenciesAsArray[2]);
            Assert.AreEqual<string>("Child10", dependenciesAsArray[3]);
            Assert.AreEqual<string>("Child11", dependenciesAsArray[4]);
        }
    }
}
