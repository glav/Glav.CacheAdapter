using System;
using Glav.CacheAdapter.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace Glav.CacheAdapter.Tests
{
    [TestClass]
    public class CacheProviderAsyncTests
    {
        [TestMethod]
        public void ShouldImplicitlyAddItemToCacheAsync()
        {
            var cacheProvider = TestHelper.GetCacheProvider();
            var cache = TestHelper.BuildTestCache();

            // Ensure we have nodata in the cache
            cacheProvider.InvalidateCacheItem("TestItem");

            // Attempt to get it via the provider
            var taskResult = cacheProvider.GetAsync<string>("TestItem", DateTime.Now.AddDays(1),
                                      () =>
                                      {
                                          return Task.Run<string>(() => { return "TestData"; });
                                      }).Result;
            // Assert that it was implicitly added to the cache via the cache provider
            Assert.IsNotNull(cache.Get<string>("TestItem"), "Expected item not in the cache");
        }

        [TestMethod]
        public void ShouldAddItemToCacheUsingDelegateAsKeyAsync()
        {
            var cacheProvider = TestHelper.GetCacheProvider();
            var cache = TestHelper.BuildTestCache();
            int accessCount = 0;

            var cacheDataDelegate = new Func<Task<string>>(() =>
            {
                return Task.Run<string>(() =>
                {
                    accessCount++;
                    return "some data";
                });

            });
            var data = cacheProvider.GetAsync<string>(DateTime.Now.AddSeconds(10), cacheDataDelegate).Result;
            // Item not in cache, delegate was called, so accessCount incremented
            Assert.AreEqual<int>(1, accessCount);

            data = cacheProvider.GetAsync<string>(DateTime.Now.AddSeconds(10), cacheDataDelegate).Result;
            // Item was in cache, so delegate was not called, so accessCount not incremented
            Assert.AreEqual<int>(1, accessCount);

        }

        [TestMethod]
        public void SettingACacheKeyAsAParentShouldNotClearItsCacheValueContentsAsync()
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

            cacheProvider.GetAsync<string>(cacheMasterKey, DateTime.Now.AddYears(1), () =>
            {
                return Task.Run<string>(() => { return cacheData; });
            }).Wait(500);

            var testGet = cacheProvider.GetAsync<string>(cacheMasterKey, DateTime.Now.AddYears(1), () =>
            {
                return Task.Run<string>(() => { return "this should never be returned since the original data will not have expired"; });
            }).Result;

            // Assert the mastercachekey contains the data we expect
            Assert.AreEqual<string>(cacheData, testGet);

            // Add some children
            // IMPORTANT NOTE: When using the generic dependency manager, calling multiple Async tasks with parent keys and having them all run concurrently can
            //                 Lead to race conditions where there is no parent key defined, and it then gets registered by *both* concurrent tasks executing
            //                 simultaneously meaning one child key potentially gets missed in the dependency list
            var task1 = cacheProvider.GetAsync<string>(cacheChildKey1, DateTime.Now.AddYears(1), () =>
            {
                return Task.Run<string>(() => { return cacheDataChild1; });
            }, cacheMasterKey).Wait(500);
            var task2 = cacheProvider.GetAsync<string>(cacheChildKey2, DateTime.Now.AddYears(1), () =>
            {
                return Task.Run<string>(() => { return cacheDataChild2; });
            }, cacheMasterKey).Wait(500);

            // Assert the child cache keys contain data we expect
            var childGet1 = cacheProvider.GetAsync<string>(cacheChildKey1, DateTime.Now.AddYears(1), () =>
            {
                return Task.Run<string>(() => { return "some different childdata1"; });
            }, cacheMasterKey);

            var childGet2 = cacheProvider.GetAsync<string>(cacheChildKey2, DateTime.Now.AddYears(1), () =>
            {
                return Task.Run<string>(() => { return "some different childdata2"; });
            }, cacheMasterKey);
            Task.WaitAll(new Task[] { childGet1, childGet2 });

            Assert.AreEqual<string>(cacheDataChild1, childGet1.Result);
            Assert.AreEqual<string>(cacheDataChild2, childGet2.Result);

            // Now check the masterkey cache data to ensure that the data is still valid after it has been converted to
            // a master key
            var testGetAgain = cacheProvider.GetAsync<string>(cacheMasterKey, DateTime.Now.AddYears(1), () =>
            {
                return Task.Run<string>(() => { return "this should never be returned since the original data will not have expired"; });
            });

            // Assert the mastercachekey contains the data we expect
            Assert.AreEqual<string>(cacheData, testGetAgain.Result);

            // And clear it all just to make sure
            cacheProvider.InvalidateCacheItem(cacheMasterKey);

            // Since all these values should have been cleared, we should now get the newly returned data
            var testGetMaster = cacheProvider.GetAsync<string>(cacheMasterKey, DateTime.Now.AddYears(1), () =>
            {
                return Task.Run<string>(() => { return "ok"; });
            });
            var childGet1Final = cacheProvider.GetAsync<string>(cacheChildKey1, DateTime.Now.AddYears(1), () =>
            {
                return Task.Run<string>(() => { return "ok"; });
            });
            var childGet2Final = cacheProvider.GetAsync<string>(cacheChildKey2, DateTime.Now.AddYears(1), () =>
            {
                return Task.Run<string>(() => { return "ok"; });
            });

            Task.WaitAll(new Task[3] { testGetMaster, childGet1Final, childGet2Final });
            Assert.AreEqual<string>("ok", testGetMaster.Result);
            Assert.AreEqual<string>("ok", childGet1Final.Result);
            Assert.AreEqual<string>("ok", childGet2Final.Result);


        }

        [TestMethod]
        public async Task ShouldImplicitlyAddItemToCacheAndExpireItemAsync()
        {
            var cacheProvider = TestHelper.GetCacheProvider();
            var cache = TestHelper.BuildTestCache();
            var expectedData = "TestData";
            // Ensure we have nodata in the cache
            cacheProvider.InvalidateCacheItem("TestItem");

            // Attempt to get it via the provider
            var result = await cacheProvider.GetAsync<string>("TestItem", DateTime.Now.AddSeconds(2),
                                      () =>
                                      {
                                          return Task.FromResult<string>(expectedData);
                                      });
            // Assert that it was implicitly added to the cache via the cache provider
            var directCacheResult = cache.Get<string>("TestItem");
            Assert.IsNotNull(directCacheResult, "Expected item not in the cache");
            Assert.AreEqual<string>(expectedData, directCacheResult, "Data was in cache, but data incorrect");

            // Assert that it was implicitly added to the cache via the cache provider
            Assert.AreEqual<string>(expectedData, result, "Data was in cache via async, but data incorrect");

            // Wait for it to expire/be evicted and assert that it has gone from the cache
            System.Threading.Thread.Sleep(4000);
            Assert.IsNull(cache.Get<string>("TestItem"), "Item still in the cache when it should have expired/been evicted");
        }

        [TestMethod]
        public async Task ShouldStoreCacheItemUsingDynamicallyGeneratedKeyAsync()
        {
            var cacheProvider = TestHelper.GetCacheProvider();
            var cache = TestHelper.BuildTestCache();

            // Implicitly add it in - bool flag should be set to true
            var data = await GetTestItemFromCacheAsync(cacheProvider, "TestData");

            // Now access it again. If not found in cache, delegate will execute but return "Junk"
            // which is not right. It should not need to run delegate to get data so
            // we get a valid value from cache
            data = await GetTestItemFromCacheAsync(cacheProvider,"Junk");

            // Assert that it has actually there and a subsequent access did not return from data store
            Assert.IsNotNull(data, "Item NOT in the cache when it should have been");
            Assert.AreEqual<string>("TestData", data);
        }

        private Task<string> GetTestItemFromCacheAsync(ICacheProvider cacheProvider, string dataToReturnFromDelegate)
        {
            return cacheProvider.GetAsync<string>(DateTime.Now.AddSeconds(5), () =>
            {
                return Task.FromResult<string>(dataToReturnFromDelegate);
            });

        }


    }
}
