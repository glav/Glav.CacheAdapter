using System;
using Glav.CacheAdapter.DependencyManagement;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Glav.CacheAdapter.Tests
{
    [TestClass]
    public class DependencyManagementTests
    {
        private const string PARENTKEYNAME = "TestCacheKeyGroup";

        [TestMethod]
        public void ShouldRegisterParentKey()
        {
            var mgr = TestHelper.GetDependencyManager();

            // Make sure we start out with nothing
            mgr.RemoveParentDependencyDefinition(PARENTKEYNAME);

            mgr.RegisterParentDependencyDefinition(PARENTKEYNAME);

            var groupEntry = mgr.GetDependentCacheKeysForParent(PARENTKEYNAME,true);
            Assert.IsNotNull(groupEntry, "Did not get a parent entry");
            Assert.AreEqual<int>(1,groupEntry.Count());
            Assert.AreEqual<string>(PARENTKEYNAME, groupEntry.First().CacheKey);
        }

        [TestMethod]
        public void ShouldRemoveParentKeyButReturnOneWhenRequested()
        {
            var mgr = TestHelper.GetDependencyManager();

            // Make sure we start out with nothing
            mgr.RemoveParentDependencyDefinition(PARENTKEYNAME);

            mgr.RegisterParentDependencyDefinition(PARENTKEYNAME);

            var groupEntry = mgr.GetDependentCacheKeysForParent(PARENTKEYNAME,true);
            Assert.IsNotNull(groupEntry, "Did not get a parent entry");
            Assert.AreEqual<int>(1, groupEntry.Count());
            Assert.AreEqual<string>(PARENTKEYNAME, groupEntry.First().CacheKey);

            // Remove it
            mgr.RemoveParentDependencyDefinition(PARENTKEYNAME);
            // Ensure we can get the entry (as it is implicitly created)
            // but has not items
            groupEntry = mgr.GetDependentCacheKeysForParent(PARENTKEYNAME);
            Assert.IsNotNull(groupEntry);
            Assert.AreEqual<int>(0, groupEntry.Count());
            // Ensure we can also return the parent node definition when requested
            groupEntry = mgr.GetDependentCacheKeysForParent(PARENTKEYNAME,true);
            Assert.IsNotNull(groupEntry);
            Assert.AreEqual<int>(1, groupEntry.Count());
        }

        [TestMethod]
        public void ShouldClearDependentItemsInParentListFromListButNotRemoveParent()
        {
            var mgr = TestHelper.GetDependencyManager();

            // Make sure we start out with nothing
            mgr.RemoveParentDependencyDefinition(PARENTKEYNAME);

            mgr.RegisterParentDependencyDefinition(PARENTKEYNAME);

            mgr.AssociateDependentKeysToParent(PARENTKEYNAME, new string[3] {"one", "two", "three"});

            // Not including parentNode
            var groupEntry = mgr.GetDependentCacheKeysForParent(PARENTKEYNAME);
            Assert.IsNotNull(groupEntry, "Did not get a parent entry");
            Assert.AreEqual<int>(3, groupEntry.Count());

            // Including the parentNode
            groupEntry = mgr.GetDependentCacheKeysForParent(PARENTKEYNAME,true);
            Assert.AreEqual<int>(4, groupEntry.Count());
            Assert.IsNotNull(groupEntry.FirstOrDefault(n => n.IsParentNode));

            // Clear the list
            mgr.RemoveParentDependencyDefinition(PARENTKEYNAME);
            // Get just the dependencies again, not including parent node
            groupEntry = mgr.GetDependentCacheKeysForParent(PARENTKEYNAME);
            Assert.IsNotNull(groupEntry);
            Assert.AreEqual<int>(0, groupEntry.Count());
            // And get all dependencies including parent node
            groupEntry = mgr.GetDependentCacheKeysForParent(PARENTKEYNAME,true);
            Assert.AreEqual<int>(1, groupEntry.Count());

        }

        [TestMethod]
        public void ShouldNotClearParentItemDefinitionWhenInvalidatingDependentItems()
        {
            var mgr = TestHelper.GetDependencyManager();
            var cacheProvider = TestHelper.GetCacheProvider();
            var cache = TestHelper.BuildTestCache();

            // Make sure we start out with nothing
            mgr.RemoveParentDependencyDefinition(PARENTKEYNAME);

            var data1 = cacheProvider.Get<string>("Key1", DateTime.Now.AddDays(1), () => "Key1Data", PARENTKEYNAME);
            var data2 = cacheProvider.Get<string>("Key2", DateTime.Now.AddDays(1), () => "Key2Data", PARENTKEYNAME);
            var data3 = cacheProvider.Get<string>("Key3", DateTime.Now.AddDays(1), () => "Key3Data", PARENTKEYNAME);
            Assert.IsNotNull(cache.Get<string>("Key1"));
            Assert.IsNotNull(cache.Get<string>("Key2"));
            Assert.IsNotNull(cache.Get<string>("Key3"));

            cacheProvider.InvalidateCacheItem(PARENTKEYNAME);
            Assert.IsNull(cache.Get<string>("Key1"));
            Assert.IsNull(cache.Get<string>("Key2"));
            Assert.IsNull(cache.Get<string>("Key3"));

            var cacheKeyForParent = string.Format("{0}{1}{2}", GenericDependencyManager.CacheKeyPrefix, GenericDependencyManager.CacheDependencyEntryPrefix, PARENTKEYNAME);
            Assert.IsNotNull(cache.Get<DependencyItem[]>(cacheKeyForParent));
        }

        [TestMethod]
        public void ShouldInvalidateNestedDependenciesAndParent()
        {
            var mgr = TestHelper.GetDependencyManager();
            var cacheProvider = TestHelper.GetCacheProvider();
            var cache = TestHelper.BuildTestCache();

            // Make sure we start out with nothing
            mgr.RemoveParentDependencyDefinition(PARENTKEYNAME);

            // Setup parent cache Item
            var parentData = cacheProvider.Get<string>(PARENTKEYNAME, DateTime.Now.AddDays(1), () => "ParentBlob");
            // Setup 1st level dependent children cache items
            var data1 = cacheProvider.Get<string>("Key1", DateTime.Now.AddDays(1), () => "Key1Data", PARENTKEYNAME);
            var data2 = cacheProvider.Get<string>("Key2", DateTime.Now.AddDays(1), () => "Key2Data", PARENTKEYNAME);
            var data3 = cacheProvider.Get<string>("Key3", DateTime.Now.AddDays(1), () => "Key3Data", PARENTKEYNAME);
            Assert.IsNotNull(cache.Get<string>(PARENTKEYNAME));
            Assert.IsNotNull(cache.Get<string>("Key1"));
            Assert.IsNotNull(cache.Get<string>("Key2"));
            Assert.IsNotNull(cache.Get<string>("Key3"));

            // Setup 2nd level dependent children cache items
            var level2_data1 = cacheProvider.Get<string>("Level2Key1", DateTime.Now.AddDays(1), () => "Key1Data", "Key1");
            var level2_data2 = cacheProvider.Get<string>("Level2Key2", DateTime.Now.AddDays(1), () => "Key2Data", "Key1");
            var level2_data3 = cacheProvider.Get<string>("Level2Key3", DateTime.Now.AddDays(1), () => "Key3Data", "Key2");
            Assert.IsNotNull(cache.Get<string>("Level2Key1"));
            Assert.IsNotNull(cache.Get<string>("Level2Key2"));
            Assert.IsNotNull(cache.Get<string>("Level2Key3"));

            // Setup 3rd level dependent children cache items
            var level3_data1 = cacheProvider.Get<string>("Level3Key1", DateTime.Now.AddDays(1), () => "Key1Data", "Level2Key1");
            var level3_data2 = cacheProvider.Get<string>("Level3Key2", DateTime.Now.AddDays(1), () => "Key2Data", "Level2Key2");
            var level3_data3 = cacheProvider.Get<string>("Level3Key3", DateTime.Now.AddDays(1), () => "Key3Data", "Level2Key3");
            Assert.IsNotNull(cache.Get<string>("Level3Key1"));
            Assert.IsNotNull(cache.Get<string>("Level3Key2"));
            Assert.IsNotNull(cache.Get<string>("Level3Key3"));

            // Setup 4th level dependent children cache items
            var level4_data1 = cacheProvider.Get<string>("Level4Key1", DateTime.Now.AddDays(1), () => "Key1Data", "Level3Key3");
            Assert.IsNotNull(cache.Get<string>("Level4Key1"));

            // Now invalidate the top level parent item
            cacheProvider.InvalidateCacheItem(PARENTKEYNAME);

            // Ensure all dependencies and children of children(nested) dependencies
            // have also cleared
            Assert.IsNull(cache.Get<string>(PARENTKEYNAME));
            // 1st level
            Assert.IsNull(cache.Get<string>("Key1"));
            Assert.IsNull(cache.Get<string>("Key2"));
            Assert.IsNull(cache.Get<string>("Key3"));
            // 2nd level
            Assert.IsNull(cache.Get<string>("Level2Key1"));
            Assert.IsNull(cache.Get<string>("Level2Key2"));
            Assert.IsNull(cache.Get<string>("Level2Key3"));
            // 3rd level
            Assert.IsNull(cache.Get<string>("Level3Key1"));
            Assert.IsNull(cache.Get<string>("Level3Key2"));
            Assert.IsNull(cache.Get<string>("Level3Key3"));
            // 4th level
            Assert.IsNull(cache.Get<string>("Level4Key1"));
        }
    }
}
