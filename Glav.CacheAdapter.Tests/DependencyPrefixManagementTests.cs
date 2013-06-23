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
            mgr.ClearDependencyPrefix("TestCacheKeyPrefix");

            mgr.RegisterDependencyPrefix("TestCacheKeyPrefix");

            var prefixEntry = mgr.GetDependencyPrefix("TestCacheKeyPrefix");
            Assert.IsNotNull(prefixEntry, "Did not get a prefix entry");
            Assert.AreEqual<Type>(typeof(DateTime), prefixEntry.GetType());
        }

    }
}
