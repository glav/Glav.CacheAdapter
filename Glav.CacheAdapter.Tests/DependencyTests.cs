using System;
using Glav.CacheAdapter.Core;
using Glav.CacheAdapter.DependencyManagement;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Glav.CacheAdapter.Tests
{
    [TestClass]
    public class DependencyTests
    {
        [TestMethod]
        public void TestMethod1()
        {
            var mgr = new GenericDependencyManager(new MemoryCacheAdapter(), new MockLogger());
            var dependencies = mgr.GetDependentCacheKeysForMasterCacheKey("Test");
            // Make sure we start out with nothing
            Assert.IsNull(dependencies);

            mgr.AssociateCacheKeyToDependentKey("Test", "Child");

            dependencies = mgr.GetDependentCacheKeysForMasterCacheKey("Test");
            Assert.IsNotNull(dependencies, "Did not get any dependencies");
            Assert.AreEqual<int>(1, dependencies.Count());
            Assert.AreEqual<string>("Child", dependencies.FirstOrDefault());
        }
    }
}
