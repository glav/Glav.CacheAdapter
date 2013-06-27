using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Glav.CacheAdapter.Tests
{
    [TestClass]
    public class CacheProviderDependencyManagementTests
    {
        [TestMethod]
        public void ShouldImplicitlyAddAssociatedDependentKeysToDependencyListForMasterCacheKeyAndClearThem()
        {
            var cacheProvider = TestHelper.GetCacheProvider();
            var cache = TestHelper.GetCacheFromConfig();

            // Add items to cache that are dependent upon a master key
            cacheProvider.Get<string>("ChildKey1", DateTime.Now.AddDays(1),() => "ChildData1", null, "MasterKey");
            cacheProvider.Get<string>("ChildKey2", DateTime.Now.AddDays(1), () => "ChildData2", null, "MasterKey");
            cacheProvider.Get<string>("ChildKey3", DateTime.Now.AddDays(1), () => "ChildData3", null, "MasterKey");

            // Assert the child items exist in the cache
            Assert.AreEqual<string>("ChildData1", cache.Get<string>("ChildKey1"));
            Assert.AreEqual<string>("ChildData2", cache.Get<string>("ChildKey2"));
            Assert.AreEqual<string>("ChildData3", cache.Get<string>("ChildKey3"));

            // Invalidate the master cache key
            cacheProvider.InvalidateCacheItem("MasterKey");

            // Assert that the dependent items have been removed from the cache
            Assert.IsNull(cache.Get<string>("ChildKey1"));
            Assert.IsNull(cache.Get<string>("ChildKey2"));
            Assert.IsNull(cache.Get<string>("ChildKey3"));

        }

        [TestMethod]
        public void ShouldAddCacheItemsToDependencyGroupAndClearThem()
        {
            var cacheProvider = TestHelper.GetCacheProvider();
            var cache = TestHelper.GetCacheFromConfig();

            // Add items to cache that are dependent upon a master key
            cacheProvider.Get<string>("ChildKey1", DateTime.Now.AddDays(1), () => "ChildData1", "Group1", null);
            cacheProvider.Get<string>("ChildKey2", DateTime.Now.AddDays(1), () => "ChildData2", "Group2", null);
            cacheProvider.Get<string>("ChildKey3", DateTime.Now.AddDays(1), () => "ChildData3", "Group1", null);

            // Assert the child items exist in the cache
            Assert.AreEqual<string>("ChildData1", cache.Get<string>("ChildKey1"));
            Assert.AreEqual<string>("ChildData2", cache.Get<string>("ChildKey2"));
            Assert.AreEqual<string>("ChildData3", cache.Get<string>("ChildKey3"));

            // Invalidate the master cache key
            cacheProvider.InvalidateDependenciesForGroup("Group1");

            // Assert that the dependent items have been removed from the cache
            Assert.IsNull(cache.Get<string>("ChildKey1"));
            Assert.IsNull(cache.Get<string>("ChildKey3"));

            // Assert that the item not in the group that was cleared is still present
            Assert.AreEqual<string>("ChildData2",cache.Get<string>("ChildKey2"));

        }
    }
}
