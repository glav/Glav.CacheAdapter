using System;
using System.Collections.Generic;
using Glav.CacheAdapter.Core;
using Glav.CacheAdapter.Core.DependencyInjection;
using Glav.CacheAdapter.DependencyManagement;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Glav.CacheAdapter.Tests
{
    [TestClass]
    public class DependencyGroupManagementTests
    {
        private const string GROUPNAME = "TestCacheKeyGroup";

        [TestMethod]
        public void ShouldRegisterDependencyGroup()
        {
            var mgr = TestHelper.GetDependencyManager();

            // Make sure we start out with nothing
            mgr.RemoveDependencyGroup(GROUPNAME);

            mgr.RegisterDependencyGroup(GROUPNAME);

            var groupEntry = mgr.GetDependencyGroup(GROUPNAME);
            Assert.IsNotNull(groupEntry, "Did not get a group entry");
            Assert.AreEqual<int>(1,groupEntry.Count());
            Assert.AreEqual<string>(GROUPNAME, groupEntry.First().CacheKeyOrCacheGroup);
        }

        [TestMethod]
        public void ShouldAddSingleDependencyToDependencyGroup()
        {
            var mgr = TestHelper.GetDependencyManager();
            // Make sure we start out with nothing
            mgr.RemoveDependencyGroup(GROUPNAME);

            mgr.RegisterDependencyGroup(GROUPNAME);
            mgr.AddCacheKeyToDependencyGroup(GROUPNAME, "CacheItem1InGroup");

            var groupEntry = mgr.GetDependencyGroup(GROUPNAME).ToArray();
            Assert.IsNotNull(groupEntry, "Did not get a group entry");
            Assert.AreEqual<int>(2, groupEntry.Length);
            // 1st Item is always the group identifier
            Assert.AreEqual<string>(GROUPNAME, groupEntry[0].CacheKeyOrCacheGroup);
            // 2nd Item is the cache key within the group
            Assert.AreEqual<string>("CacheItem1InGroup", groupEntry[1].CacheKeyOrCacheGroup);
            Assert.AreEqual<CacheDependencyAction>(CacheDependencyAction.ClearDependentItems, groupEntry[1].Action);
        }

        [TestMethod]
        public void ShouldAddMultipleDependenciesToDependencyGroupWithNoConflict()
        {
            var mgr = TestHelper.GetDependencyManager();
            // Make sure we start out with nothing
            mgr.RemoveDependencyGroup(GROUPNAME);

            mgr.RegisterDependencyGroup(GROUPNAME);
            mgr.AddCacheKeyToDependencyGroup(GROUPNAME, "CacheItem1InGroup");
            mgr.AddCacheKeyToDependencyGroup(GROUPNAME, "CacheItem2InGroup");
            // Add the same onein again. Should not error out
            mgr.AddCacheKeyToDependencyGroup(GROUPNAME, "CacheItem1InGroup");

            var groupEntry = mgr.GetDependencyGroup(GROUPNAME).ToArray();
            Assert.IsNotNull(groupEntry, "Did not get a group entry");
            Assert.AreEqual<int>(3, groupEntry.Length);
            // 1st Item is always the group identifier
            Assert.AreEqual<string>(GROUPNAME, groupEntry[0].CacheKeyOrCacheGroup);
            // 2nd Item is the cache key within the group
            Assert.AreEqual<string>("CacheItem1InGroup", groupEntry[1].CacheKeyOrCacheGroup);
            Assert.AreEqual<CacheDependencyAction>(CacheDependencyAction.ClearDependentItems, groupEntry[1].Action);
            Assert.AreEqual<string>("CacheItem2InGroup", groupEntry[2].CacheKeyOrCacheGroup);
            Assert.AreEqual<CacheDependencyAction>(CacheDependencyAction.ClearDependentItems, groupEntry[1].Action);
        }

        [TestMethod]
        public void ShouldRemoveDependencyGroupButReturnOneWhenRequested()
        {
            var mgr = TestHelper.GetDependencyManager();

            // Make sure we start out with nothing
            mgr.RemoveDependencyGroup(GROUPNAME);

            mgr.RegisterDependencyGroup(GROUPNAME);

            var groupEntry = mgr.GetDependencyGroup(GROUPNAME);
            Assert.IsNotNull(groupEntry, "Did not get a group entry");
            Assert.AreEqual<int>(1, groupEntry.Count());
            Assert.AreEqual<string>(GROUPNAME, groupEntry.First().CacheKeyOrCacheGroup);

            mgr.RemoveDependencyGroup(GROUPNAME);
            groupEntry = mgr.GetDependencyGroup(GROUPNAME);
            Assert.IsNotNull(groupEntry);
        }

        [TestMethod]
        public void ShouldRemoveCacheItemsInGroupFromCache()
        {
            var cache = TestHelper.GetCacheFromConfig();
            var mgr = TestHelper.GetDependencyManager();

            // Make sure we start out with nothing
            mgr.RemoveDependencyGroup(GROUPNAME);

            mgr.RegisterDependencyGroup(GROUPNAME);

            // Define our cache dependency groupmembers
            mgr.AddCacheKeyToDependencyGroup(GROUPNAME, "Key1");
            mgr.AddCacheKeyToDependencyGroup(GROUPNAME, "Key2");
            mgr.AddCacheKeyToDependencyGroup(GROUPNAME, "Key3");

            // Add items to cache with same keys as per dependency group
            cache.Add("Key1", DateTime.Now.AddDays(1),"Data1");
            cache.Add("Key2", DateTime.Now.AddDays(1), "Data2");
            cache.Add("Key3", DateTime.Now.AddDays(1), "Data3");

            // Assert that the group is defined
            var groupEntry = mgr.GetDependencyGroup(GROUPNAME);
            Assert.IsNotNull(groupEntry, "Did not get a group entry");
            Assert.AreEqual<int>(4, groupEntry.Count());

            // Assert we have the items actually in the cache
            Assert.IsNotNull(cache.Get<string>("Key1"));
            Assert.AreEqual<string>("Data1", cache.Get<string>("Key1"));
            Assert.IsNotNull(cache.Get<string>("Key2"));
            Assert.AreEqual<string>("Data2", cache.Get<string>("Key2"));
            Assert.IsNotNull(cache.Get<string>("Key3"));
            Assert.AreEqual<string>("Data3", cache.Get<string>("Key3"));

            //Now clear the dependencies
            mgr.CheckGroupDependenciesAndPerformAction(GROUPNAME);

            // All items in group should be cleared from cache
            Assert.IsNull(cache.Get<string>("Key1"));
            Assert.IsNull(cache.Get<string>("Key2"));
            Assert.IsNull(cache.Get<string>("Key3"));
        }
    }
}
