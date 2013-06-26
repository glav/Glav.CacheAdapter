using System;
using System.Collections.Generic;
using Glav.CacheAdapter.Core;
using Glav.CacheAdapter.DependencyManagement;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Glav.CacheAdapter.Tests
{
    [TestClass]
    public class DependencyPrefixManagementTests
    {
        [TestMethod]
        public void ShouldAddSingleDependencyPrefixItem()
        {
            var mgr = new GenericDependencyManager(new MemoryCacheAdapter(), new MockLogger());
            // Make sure we start out with nothing
            mgr.RemoveDependencyGroup("TestCacheKeyGroup");

            mgr.RegisterDependencyGroup("TestCacheKeyGroup");

            var groupEntry = mgr.GetDependencyGroup("TestCacheKeyGroup");
            Assert.IsNotNull(groupEntry, "Did not get a group entry");
            Assert.AreEqual<int>(1,groupEntry.Count());
            Assert.AreEqual<string>("TestCacheKeyGroup", groupEntry.First().CacheKeyOrCacheGroup);
        }

    }
}
