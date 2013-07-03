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
        private const string PARENTKEY = "TestParentKey";
        [TestMethod]
        public void ShouldAddSingleDependencyItem()
        {
            var mgr = TestHelper.GetDependencyManager();
            // Make sure we start out with nothing
            mgr.ClearDependencyListForParent(PARENTKEY);

            mgr.AssociateDependentKeysToParent(PARENTKEY, new string[1] { "Child"});

            var dependencies = mgr.GetDependentCacheKeysForParent(PARENTKEY);
            Assert.IsNotNull(dependencies, "Did not get any dependencies");
            Assert.AreEqual<int>(1, dependencies.Count());
            Assert.AreEqual<string>("Child", dependencies.FirstOrDefault().CacheKey);
        }

        [TestMethod]
        public void ShouldAddMultipleDependencyItems()
        {
            var mgr = TestHelper.GetDependencyManager();
            // Make sure we start out with nothing
            mgr.ClearDependencyListForParent(PARENTKEY);

            var dependenciesToAdd = new List<string>();
            dependenciesToAdd.Add("Child1");
            dependenciesToAdd.Add("Child2");
            dependenciesToAdd.Add("Child3");
            mgr.AssociateDependentKeysToParent(PARENTKEY, dependenciesToAdd);

            var dependencies = mgr.GetDependentCacheKeysForParent(PARENTKEY);
            Assert.IsNotNull(dependencies, "Did not get any dependencies");
            var dependenciesAsArray = dependencies.ToArray();
            Assert.AreEqual<int>(3, dependenciesAsArray.Length);
            Assert.AreEqual<string>("Child1", dependenciesAsArray[0].CacheKey);
            Assert.AreEqual<string>("Child2", dependenciesAsArray[1].CacheKey);
            Assert.AreEqual<string>("Child3", dependenciesAsArray[2].CacheKey);
        }

        [TestMethod]
        public void ShouldAddMultipleDependencyItemsWithNoConflict()
        {
            var mgr = TestHelper.GetDependencyManager();
            // Make sure we start out with nothing
            mgr.ClearDependencyListForParent(PARENTKEY);

            var dependenciesToAdd = new List<string>();
            dependenciesToAdd.Add("Child1");
            dependenciesToAdd.Add("Child2");
            dependenciesToAdd.Add("Child3");
            mgr.AssociateDependentKeysToParent(PARENTKEY, dependenciesToAdd);

            // Now add some more items, some that are already added,some not.
            dependenciesToAdd.Clear();
            dependenciesToAdd.Add("Child10");  // should add
            dependenciesToAdd.Add("Child2");  // should not add
            dependenciesToAdd.Add("Child11");  // should add
            mgr.AssociateDependentKeysToParent(PARENTKEY, dependenciesToAdd);

            var dependencies = mgr.GetDependentCacheKeysForParent(PARENTKEY);
            Assert.IsNotNull(dependencies, "Did not get any dependencies");
            var dependenciesAsArray = dependencies.ToArray();
            Assert.AreEqual<int>(5, dependenciesAsArray.Length);
            Assert.AreEqual<string>("Child1", dependenciesAsArray[0].CacheKey);
            Assert.AreEqual<string>("Child2", dependenciesAsArray[1].CacheKey);
            Assert.AreEqual<string>("Child3", dependenciesAsArray[2].CacheKey);
            Assert.AreEqual<string>("Child10", dependenciesAsArray[3].CacheKey);
            Assert.AreEqual<string>("Child11", dependenciesAsArray[4].CacheKey);
        }

        [TestMethod]
        public void ShouldClearAssociatedCacheItemDependencyFromCache()
        {
            var cache = TestHelper.GetCacheFromConfig();
            var mgr = TestHelper.GetDependencyManager();
            // Make sure we start out with nothing
            mgr.ClearDependencyListForParent(PARENTKEY);

            // Associate a dependent cachekey
            mgr.AssociateDependentKeysToParent(PARENTKEY, new string[1] { "Child"});

            // Addin the master item
            cache.Add(PARENTKEY, DateTime.Now.AddDays(1), "DataBlob");
            // Add in the dependent item
            cache.Add("Child", DateTime.Now.AddDays(1), "DataBlob2");

            // Now clear the dependencies for the master
            mgr.PerformActionForDependenciesAssociatedWithParent(PARENTKEY);

            // And finally check its existence
            Assert.IsNull(cache.Get<string>("Child"));
        }

        [TestMethod]
        public void ShouldClearMultipleAssociatedCacheItemDependenciesFromCache()
        {
            var cache = TestHelper.GetCacheFromConfig();
            var mgr = TestHelper.GetDependencyManager();
            // Make sure we start out with nothing
            mgr.ClearDependencyListForParent(PARENTKEY);

            // Associate a dependent cachekey
            mgr.AssociateDependentKeysToParent(PARENTKEY,new string[3] { "Child1","Child2","Child3"});

            // Addin the master item
            cache.Add(PARENTKEY, DateTime.Now.AddDays(1), "DataBlob");
            // Add in the dependent items
            cache.Add("Child1", DateTime.Now.AddDays(1), "DataBlob2");
            cache.Add("Child2", DateTime.Now.AddDays(1), "DataBlob3");
            cache.Add("Child3", DateTime.Now.AddDays(1), "DataBlob4");

            // Now clear the dependencies for the master
            mgr.PerformActionForDependenciesAssociatedWithParent(PARENTKEY);

            // And finally check its existence
            Assert.IsNull(cache.Get<string>("Child1"));
            Assert.IsNull(cache.Get<string>("Child2"));
            Assert.IsNull(cache.Get<string>("Child3"));
        }
        [TestMethod]
        public void ShouldClearMultipleAssociatedCacheItemDependenciesFromCacheUsingBatchAssociation()
        {
            var cache = TestHelper.GetCacheFromConfig();
            var mgr = TestHelper.GetDependencyManager();
            // Make sure we start out with nothing
            mgr.ClearDependencyListForParent(PARENTKEY);

            // Associate a dependent cachekey
            var dependencyList = new string[3] {"Child1", "Child2", "Child3"};
            mgr.AssociateDependentKeysToParent(PARENTKEY, dependencyList);

            // Addin the master item
            cache.Add(PARENTKEY, DateTime.Now.AddDays(1), "DataBlob");
            // Add in the dependent items
            cache.Add("Child1", DateTime.Now.AddDays(1), "DataBlob2");
            cache.Add("Child2", DateTime.Now.AddDays(1), "DataBlob3");
            cache.Add("Child3", DateTime.Now.AddDays(1), "DataBlob4");

            // Now clear the dependencies for the master
            mgr.PerformActionForDependenciesAssociatedWithParent(PARENTKEY);

            // And finally check its existence
            Assert.IsNull(cache.Get<string>("Child1"));
            Assert.IsNull(cache.Get<string>("Child2"));
            Assert.IsNull(cache.Get<string>("Child3"));
        }
    }
}
