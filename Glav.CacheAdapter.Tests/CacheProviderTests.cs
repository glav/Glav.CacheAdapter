using System;
using Glav.CacheAdapter.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Glav.CacheAdapter.Tests
{
    [TestClass]
    public class CacheProviderTests
    {
        [TestMethod]
        public void ShouldImplicitlyAddItemToCache()
        {
            var cacheProvider = TestHelper.GetCacheProvider();
            var cache = TestHelper.BuildTestCache();

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
        public void ShouldNotBlowUpWhenRemovingAteemptingToRemoveAnItemNotInTheCache()
        {
            var cacheProvider = TestHelper.GetCacheProvider();
            var cache = TestHelper.BuildTestCache();

            // Ensure we have nodata in the cache
            cacheProvider.InvalidateCacheItem("hoochy coochy blah blah");

            cacheProvider.InvalidateCacheItems(new string[] { "dont kill me", "really dont", "fine then kill me" });

        }

        [TestMethod]
        public void ShouldAddItemToCacheUsingDelegateAsKey()
        {
            var cacheProvider = TestHelper.GetCacheProvider();
            var cache = TestHelper.BuildTestCache();
            int accessCount = 0;

            var cacheDataDelegate = new Func<string>(() =>
            {
                accessCount++;
                return "some data";

            });
            var data = cacheProvider.Get<string>(DateTime.Now.AddSeconds(10), cacheDataDelegate);
            // Item not in cache, delegate was called, so accessCount incremented
            Assert.AreEqual<int>(1, accessCount);

            data = cacheProvider.Get<string>(DateTime.Now.AddSeconds(10), cacheDataDelegate);
            // Item was in cache, so delegate was not called, so accessCount not incremented
            Assert.AreEqual<int>(1, accessCount);

        }

        [TestMethod]
        public void SettingACacheKeyAsAParentShouldNotClearItsCacheValueContents()
        {
            const string cacheData = "Some data to cache";
            const string cacheDataChild1 = "childdata1";
            const string cacheDataChild2 = "childdata2";
            const string cacheMasterKey = "masterKeyTest";
            const string cacheChildKey1 = "childkey1";
            const string cacheChildKey2 = "childkey2";

            var cacheProvider = TestHelper.GetCacheProvider();
            var cache = TestHelper.BuildTestCache();

            // Ensure we have nodata in the cache
            cacheProvider.ClearAll();

            cacheProvider.Get<string>(cacheMasterKey, DateTime.Now.AddYears(1), () =>
            {
                return cacheData;
            });

            var testGet = cacheProvider.Get<string>(cacheMasterKey, DateTime.Now.AddYears(1), () =>
            {
                return "this should never be returned since the original data will not have expired";
            });

            // Assert the mastercachekey contains the data we expect
            Assert.AreEqual<string>(cacheData, testGet);

            // Add some children
            cacheProvider.Get<string>(cacheChildKey1, DateTime.Now.AddYears(1), () =>
            {
                return cacheDataChild1;
            }, cacheMasterKey);
            cacheProvider.Get<string>(cacheChildKey2, DateTime.Now.AddYears(1), () =>
            {
                return cacheDataChild2;
            }, cacheMasterKey);

            // Assert the child cache keys contain data we expect
            var childGet1 = cacheProvider.Get<string>(cacheChildKey1, DateTime.Now.AddYears(1), () =>
            {
                return "some different childdata1";
            }, cacheMasterKey);
            var childGet2 = cacheProvider.Get<string>(cacheChildKey2, DateTime.Now.AddYears(1), () =>
            {
                return "some different childdata2";
            }, cacheMasterKey);
            Assert.AreEqual<string>(cacheDataChild1, childGet1);
            Assert.AreEqual<string>(cacheDataChild2, childGet2);

            // Now check the masterkey cache data to ensure that the data is still valid after it has been converted to
            // a master key
            var testGetAgain = cacheProvider.Get<string>(cacheMasterKey, DateTime.Now.AddYears(1), () =>
            {
                return "this should never be returned since the original data will not have expired";
            });

            // Assert the mastercachekey contains the data we expect
            Assert.AreEqual<string>(cacheData, testGet);

            // And clear it all just to make sure
            cacheProvider.InvalidateCacheItem(cacheMasterKey);

            // Since all these values should have been cleared, we should now get the newly returned data
            var testGetMaster = cacheProvider.Get<string>(cacheMasterKey, DateTime.Now.AddYears(1), () =>
            {
                return "ok";
            });
            var childGet1Final = cacheProvider.Get<string>(cacheChildKey1, DateTime.Now.AddYears(1), () =>
            {
                return "ok";
            });
            var childGet2Final = cacheProvider.Get<string>(cacheChildKey2, DateTime.Now.AddYears(1), () =>
            {
                return "ok";
            });
            Assert.AreEqual<string>("ok", testGetMaster);
            Assert.AreEqual<string>("ok", childGet1Final);
            Assert.AreEqual<string>("ok", childGet2Final);


        }

        [TestMethod]
        public void ShouldInvalidateASeriesOfCacheKeys()
        {
            var cacheProvider = TestHelper.GetCacheProvider();
            var cache = TestHelper.BuildTestCache();

            cache.Add("one", DateTime.Now.AddMinutes(100), "test data for one");
            cache.Add("two", DateTime.Now.AddMinutes(100), "test data for two");
            cache.Add("three", DateTime.Now.AddMinutes(100), "test data for three");
            cache.Add("four", DateTime.Now.AddMinutes(100), "test data for four");
            
            // Ensure we have the data in the cache
            Assert.IsNotNull(cache.Get<string>("one"));
            Assert.IsNotNull(cache.Get<string>("two"));
            Assert.IsNotNull(cache.Get<string>("three"));
            Assert.IsNotNull(cache.Get<string>("four"));

            // Now clear them all
            cacheProvider.InvalidateCacheItems(new string[] { "one","two","three","four" });

            // Ensure we have removed the data in the cache
            Assert.IsNull(cache.Get<string>("one"));
            Assert.IsNull(cache.Get<string>("two"));
            Assert.IsNull(cache.Get<string>("three"));
            Assert.IsNull(cache.Get<string>("four"));

        }

        [TestMethod]
        public void ShouldImplicitlyAddItemToCacheAndExpireItem()
        {
            var cacheProvider = TestHelper.GetCacheProvider();
            var cache = TestHelper.BuildTestCache();

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
            var cache = TestHelper.BuildTestCache();

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
            var cache = TestHelper.BuildTestCache();

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
            var cache = TestHelper.BuildTestCache();
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
