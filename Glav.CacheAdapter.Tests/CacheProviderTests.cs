using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Glav.CacheAdapter.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Glav.CacheAdapter.Bootstrap;
using Glav.CacheAdapter.Features;

namespace Glav.CacheAdapter.Tests
{
    [TestClass]
    public class CacheProviderTests
    {
        [TestMethod]
        public void ShouldImplicitlyAddItemToCache()
        {
            var cacheProvider = TestHelper.GetCacheProvider();
            var cache = TestHelper.GetCacheFromConfig();

            // Ensure we have nodata in the cache
            cacheProvider.InvalidateCacheItem("TestItem");

            // Attempt to get it via the provider
            cacheProvider.Get<string>("TestItem", DateTime.Now.AddDays(1),
                                      () =>
                                      {
                                          return "TestData";
                                      });
            // Assert that it was inplicitly added to the cache via the cache provider
            Assert.IsNotNull(cache.Get<string>("TestItem"), "Expected item not in the cache");
        }

        [TestMethod]
        public void ShouldImplicitlyAddItemToCacheAndExpireItem()
        {
            var cacheProvider = TestHelper.GetCacheProvider();
            var cache = TestHelper.GetCacheFromConfig();

            // Ensure we have nodata in the cache
            cacheProvider.InvalidateCacheItem("TestItem");

            // Attempt to get it via the provider
            cacheProvider.Get<string>("TestItem", DateTime.Now.AddSeconds(2),
                                      () =>
                                      {
                                          return "TestData";
                                      });
            // Assert that it was implicitly added to the cache via the cache provider
            Assert.IsNotNull(cache.Get<string>("TestItem"), "Expected item not in the cache");

            // Wait for it to expire/be evicted and assert that it has gone from the cache
            System.Threading.Thread.Sleep(4000);
            Assert.IsNull(cache.Get<string>("TestItem"), "Item still in the cache when it should have expired/been evicted");
        }

        [TestMethod]
        public void ShouldInvalidateCacheItem()
        {
            var cacheProvider = TestHelper.GetCacheProvider();
            var cache = TestHelper.GetCacheFromConfig();

            // Ensure we have nodata in the cache
            cacheProvider.InvalidateCacheItem("TestItem");
            cache.Add("TestItem", DateTime.Now.AddDays(1), "TestData");

            // Invalidate/clear item via the provider
            cacheProvider.InvalidateCacheItem("TestItem");

            // Assert that it has been removed
            Assert.IsNull(cache.Get<string>("TestItem"), "Item still in the cache when it should have expired/been evicted");
        }

        [TestMethod]
        public void ShouldStoreCacheItemUsingDynamicallyGeneratedKey()
        {
            var cacheProvider = TestHelper.GetCacheProvider();
            var cache = TestHelper.GetCacheFromConfig();

            // Ensure we have nodata in the cache
            cache.InvalidateCacheItem("TestItem");
            // Implicitly add it in - bool flag should be set to true
            var data = GetTestItemFromCache(cacheProvider,"TestData");

            // Now access it again. If not found in cache, delegate will execute bt return empty
            // which is not right. It should not need to run delegate to get data so
            // we get a valid value from cache
            data = GetTestItemFromCache(cacheProvider,"Junk");

            // Assert that it has actually there and a subsequent access did not return from data store
            Assert.IsNotNull(data, "Item NOT in the cache when it should have been");
            Assert.AreEqual<string>("TestData", data);
        }

        [TestMethod]
        public void ShouldClearCache()
        {
            var cacheConfig = new CacheConfig();

            const int NumDataItems = 100;
            var cache = TestHelper.GetCacheFromConfig();
            var cacheProvider = TestHelper.GetCacheProvider();

            // Add data to cache
            for (var cnt = 0; cnt < NumDataItems; cnt++ )
            {
                cache.Add(string.Format("Key{0}", cnt), DateTime.Now.AddDays(1), string.Format("Data-{0}", cnt));
            }

            // Assert data is in cache
            for (var cnt = 0; cnt < NumDataItems; cnt++)
            {
                Assert.IsNotNull(cache.Get<string>(string.Format("Key{0}", cnt)));
            }

            if (cacheProvider.FeatureSupport.SupportsClearingCacheContents())
            {
                // Clear it
                cacheProvider.ClearAll();

                // Assert data is cleared from cache
                for (var cnt = 0; cnt < NumDataItems; cnt++)
                {
                    Assert.IsNull(cache.Get<string>(string.Format("Key{0}", cnt)));
                }
            } else
            {
                Assert.Inconclusive("Cache does not support cleating contents.");
            }
        }

        private string GetTestItemFromCache(ICacheProvider cacheProvider, string dataToReturnFromDelegate)
        {
            return cacheProvider.Get<string>(DateTime.Now.AddDays(1), () =>
            {
                return dataToReturnFromDelegate;
            });

        }


    }
}
