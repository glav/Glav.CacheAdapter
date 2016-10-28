using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Glav.CacheAdapter.Tests
{
    [TestClass]
    public class DependencyUsageTests
    {
        private const string PARENTKEY = "TestParentKey";
        [TestMethod]
        public void ShouldAddSingleDependencyItem()
        {
            var mgr = TestHelper.GetDependencyManager();
            // Make sure we start out with nothing
            mgr.RemoveParentDependencyDefinition(PARENTKEY);

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
            mgr.RemoveParentDependencyDefinition(PARENTKEY);

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
            mgr.RemoveParentDependencyDefinition(PARENTKEY);

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
            var cache = TestHelper.BuildTestCache();
            var mgr = TestHelper.GetDependencyManager();
            // Make sure we start out with nothing
            mgr.RemoveParentDependencyDefinition(PARENTKEY);

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
            var cache = TestHelper.BuildTestCache();
            var mgr = TestHelper.GetDependencyManager();
            // Make sure we start out with nothing
            mgr.RemoveParentDependencyDefinition(PARENTKEY);

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
            var cache = TestHelper.BuildTestCache();
            var mgr = TestHelper.GetDependencyManager();
            // Make sure we start out with nothing
            mgr.RemoveParentDependencyDefinition(PARENTKEY);

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

        [TestMethod]
        public void ShouldClearNestedDependenciesFromCacheAsWellAsImmediateDependencies()
        {
            var cache = TestHelper.BuildTestCache();
            var mgr = TestHelper.GetDependencyManager();
            // Make sure we start out with nothing
            mgr.RemoveParentDependencyDefinition(PARENTKEY);

            // Associate a dependent cachekey
            var dependencyList = new string[3] { "Child1", "Child2", "Child3" };
            mgr.AssociateDependentKeysToParent(PARENTKEY, dependencyList);

            // We have PARENTKEY as a parent, but also defined Child1 as the parent of these keys
            var nestedDependencyList1 = new string[3] { "Child1-Child1", "Child1-Child2", "Child1-Child3" };
            mgr.AssociateDependentKeysToParent("Child1", nestedDependencyList1);

            // We have PARENTKEY as a parent, but also defined Child2 as the parent of these keys
            var nestedDependencyList2 = new string[3] { "Child2-Child1", "Child2-Child2", "Child2-Child3" };
            mgr.AssociateDependentKeysToParent("Child2", nestedDependencyList2);

            // Addin the master item
            cache.Add(PARENTKEY, DateTime.Now.AddDays(1), "DataBlob");
            
            // Add in the dependent items
            cache.Add("Child1", DateTime.Now.AddDays(1), "DataBlob2");
            cache.Add("Child2", DateTime.Now.AddDays(1), "DataBlob3");
            cache.Add("Child3", DateTime.Now.AddDays(1), "DataBlob4");

            // Add in the dependent items of the children
            cache.Add("Child1-Child1", DateTime.Now.AddDays(1), "DataBlob1-1");
            cache.Add("Child1-Child2", DateTime.Now.AddDays(1), "DataBlob1-2");
            cache.Add("Child1-Child3", DateTime.Now.AddDays(1), "DataBlob1-3");
            cache.Add("Child2-Child1", DateTime.Now.AddDays(1), "DataBlob2-1");
            cache.Add("Child2-Child2", DateTime.Now.AddDays(1), "DataBlob2-2");
            cache.Add("Child2-Child3", DateTime.Now.AddDays(1), "DataBlob2-3");

            // Now clear the dependencies for the master
            mgr.PerformActionForDependenciesAssociatedWithParent(PARENTKEY);

            // And finally check for the dependencies existence
            Assert.IsNull(cache.Get<string>("Child1"));
            Assert.IsNull(cache.Get<string>("Child2"));
            Assert.IsNull(cache.Get<string>("Child3"));
            Assert.IsNull(cache.Get<string>("Child1-Child1"));
            Assert.IsNull(cache.Get<string>("Child1-Child2"));
            Assert.IsNull(cache.Get<string>("Child1-Child3"));
            Assert.IsNull(cache.Get<string>("Child2-Child1"));
            Assert.IsNull(cache.Get<string>("Child2-Child2"));
            Assert.IsNull(cache.Get<string>("Child2-Child3"));
        }

        [TestMethod]
        public void ShouldNotCrashWhenCircularDependenciesAreEncountered()
        {
            var cacheProvider = TestHelper.GetCacheProvider();
            var mgr = TestHelper.GetDependencyManager();
            // Make sure we start out with nothing
            mgr.RemoveParentDependencyDefinition(PARENTKEY);

            // Associate a series of child dependent cachekeys to a parent
            var dependencyList = new string[3] { "Child1", "Child2", "Child3" };
            mgr.AssociateDependentKeysToParent(PARENTKEY, dependencyList);

            // Associate some child cache keys to one of the previous child keys - Child1
            var dependencyList2 = new string[2] { "SubChild1","SubChild2" };
            mgr.AssociateDependentKeysToParent("Child1", dependencyList2);

            // Now associate one of the 2nd level child keys - SubChild1 
            // as a parent of the original top level parent key tocreate a circular
            //dependency loop. 
            var dependencyList3 = new string[1] { PARENTKEY };
            mgr.AssociateDependentKeysToParent("SubChild1", dependencyList3);

            // All cache keys should be cleared but the recursion should not create
            //an infinite loop and thus a stack overflow
            cacheProvider.InvalidateCacheItem(PARENTKEY);
        }

        [TestMethod]
        public void ShouldNotCreateInifniteLoopWhenCircularDependenciesAreEncounteredButStillClearCacheItems()
        {
            var cacheProvider = TestHelper.GetCacheProvider();
            var cache = TestHelper.BuildTestCache();
            var mgr = TestHelper.GetDependencyManager();
            // Make sure we start out with nothing
            mgr.RemoveParentDependencyDefinition(PARENTKEY);

            // Associate parent key to its parent of "SubChild1" 
            var parentData = cacheProvider.Get<String>(PARENTKEY, DateTime.Now.AddDays(1), () => "ParentBlob", "SubChild1");
            // Associate child1 to its parent of parentkey
            var child1Data = cacheProvider.Get<String>("Child1", DateTime.Now.AddDays(1), () => "Child1Blob", PARENTKEY);
           // Associate "SubChild1" as a parent of child1
            var subChildData = cacheProvider.Get<String>("SubChild1", DateTime.Now.AddDays(1), () => "SubChildBlob", "Child1");
            // At this point we have the following heirarchy:
            // SubChild1 -> PARENTKEY -> Child1 -> SubChild1 -> PARENTKEY -> Child1.... etc

            // Make sure data is all in the cache
            Assert.IsNotNull(cache.Get<string>(PARENTKEY));
            Assert.IsNotNull(cache.Get<string>("Child1"));
            Assert.IsNotNull(cache.Get<string>("SubChild1"));

            // All cache keys should be cleared but the recursion should not create
            //an infinite loop and thus a stack overflow
            cacheProvider.InvalidateCacheItem(PARENTKEY);

            // Ensure the data is removed from the cache
            Assert.IsNull(cache.Get<string>(PARENTKEY));
            Assert.IsNull(cache.Get<string>("Child1"));
            Assert.IsNull(cache.Get<string>("SUbChild1"));
        }
    }
}
