#region Using Statements
using System;
using Glav.CacheAdapter.Core.DependencyInjection;

#endregion

namespace Glav.CacheAdapter.ExampleUsage
{
    /// <summary>
    /// This class is simply a very basic demonstration of the use of the CacheAdapter
    /// in an application. This class is not required in any way so feel free to delete or modify
    /// this class as you see fit.
    /// </summary>
    class Program
    {
        static ConsoleColor _originalColor = Console.ForegroundColor;
        static void Main(string[] args)
        {
            
            // Basic examples usage
            var allTestsPassed = ExampleAddAndRetrieveFromCache();

            if (allTestsPassed)
            {
                // Basic dependency management usage - if not enabled in the
                // app.config file, these wont work.
                allTestsPassed = ExampleAddAndClearWithDependencies();
            }

            if (allTestsPassed)
            {
                ExampleAddUsingAsyncCalls();
            }

            // Uncomment this line to simulate about 100,000 hits to the cache engine. 
            // This is only available if you have download/copied/cloned the entire solutions source code. If
            // you have simply installed the nuget package, uncommenting this line will cause an error since
            // the cache 'hammering' class in not included in the nuget package.
            //HammerTheCache.StartHammering();

            // Uncomment this line to see how config can be changed/supplied programmatically.
            //  The example switches the config to use memcached to you need that running
            //ExampleSettingConfigurationViaCode();

            if (allTestsPassed)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("All Tests passed.");
            }
            else
            {
                WriteErrMsgToConsole("One or more tests failed.");
            }

            Console.WriteLine("Done.");
            Console.ReadLine();

        }

        #region Simple Async Call examples
        private static bool ExampleAddUsingAsyncCalls()
        {
            WriteHeadingSeparator("Simple Cache examples using Async calls");
            var cacheProvider = AppServices.Cache;
            cacheProvider.ClearAll();
            const string  cacheData = "AsyncData";
            const string cacheKey = "async-key";

            Console.WriteLine("Call GetAsync with long running task to populate cache");
            var expensiveData = cacheProvider.GetAsync<string>(cacheKey, DateTime.Now.AddDays(1), () =>
             {
                 return System.Threading.Tasks.Task.Run(() =>
                 {
                     Wait(2);  // simulate some long running operation
                     return cacheData;
                 });
             });

            Console.WriteLine("...Doing some small work and then checking if item is in cache yet (which it should not be)");
            // Do some work
            Wait(1);
            var doesCacheItemExistInCacheYet = (cacheProvider.InnerCache.Get<string>(cacheKey) != null);

            if (doesCacheItemExistInCacheYet == true)
            {
                WriteErrMsgToConsole("Cache item was already in cache when it was not expected to be in there yet via the async method");
                return false;
            }

            Console.WriteLine("...Doing a little more work which should take longer than the async task that places data in cache so we expect it to exist in cache.");
            Wait(1.1);

            var doesCacheItemExistInCacheNow = (cacheProvider.InnerCache.Get<string>(cacheKey) != null);
            if (doesCacheItemExistInCacheNow == false)
            {
                WriteErrMsgToConsole("Cache item was NOT in cache when it was expected to be in there via the async method");
                return false;
            }

            Console.WriteLine(">> Item was placed in cache via async method as expected\n\n");
            return true;
        }
        #endregion

        #region Examples Adding and Clearing with Dependencies
        private static bool ExampleAddAndClearWithDependencies()
        {
            WriteHeadingSeparator("Simple Cache Dependency examples");
            var cacheProvider = AppServices.Cache;
            cacheProvider.ClearAll();
            const string masterDataKey = "MasterData";


            Console.WriteLine("1. Adding the main data to cache which acts as the trigger for the other added items");
            //cacheProvider.InvalidateCacheItem(masterDataKey);
            var masterData = cacheProvider.Get<string>(masterDataKey, DateTime.Now.AddDays(1), () => "Master Data Item");

            Console.WriteLine("\n2. Attempting to get some related Data from cache provider which will\nimplicitly add it to the cache and related it to the 'MasterItem'\nadded previously.");
            var bitOfRelatedData1 = cacheProvider.Get<string>("Bit1", DateTime.Now.AddDays(1), () => "Some Bit Of Data1", masterDataKey);
            var bitOfRelatedData2 = cacheProvider.Get<string>("Bit2", DateTime.Now.AddDays(1), () => "Some Bit Of Data2", masterDataKey);

            Console.WriteLine("\n3. Make sure everything is in the cache as expected.");
            var bit1Ok = cacheProvider.InnerCache.Get<string>("Bit1") != null;
            var bit2Ok = cacheProvider.InnerCache.Get<string>("Bit2") != null;
            var masterDataOk = cacheProvider.InnerCache.Get<string>(masterDataKey) != null;

            Console.WriteLine("Bit1 is in the cache?:{0}", bit1Ok);
            Console.WriteLine("Bit2 is in the cache?:{0}", bit2Ok);
            Console.WriteLine("MasterData item is in the cache?:{0}", masterDataOk);


            if (!bit1Ok || !bit2Ok || !masterDataOk)
            {
                WriteErrMsgToConsole("Items were not present in cache.");
                return false;
            }

            Console.WriteLine("\n5. Now clearing the master data item which should also clear the related items");
            cacheProvider.InvalidateCacheItem(masterDataKey);
            Wait(2);

            Console.WriteLine("\n6. Make sure everything is NOT in the cache as expected.");
            var bit1Value = cacheProvider.InnerCache.Get<string>("Bit1");
            var bit2Value = cacheProvider.InnerCache.Get<string>("Bit2");
            bit1Ok = bit1Value == null;
            bit2Ok = bit2Value == null;
            var masterDataValue = cacheProvider.InnerCache.Get<string>(masterDataKey);
            masterDataOk = masterDataValue == null;

            Console.WriteLine("Bit1 is NOT in the cache?:{0}", bit1Ok);
            Console.WriteLine("Bit2 is NOT in the cache?:{0}", bit2Ok);
            Console.WriteLine("MasterData item is NOT in the cache?:{0}", masterDataOk);

            if (!bit1Ok || !bit2Ok || !masterDataOk)
            {
                WriteErrMsgToConsole("Items were present in cache when they should have been removed.");
                return false;
            }

            Console.WriteLine("All dependencies worked as expected.");

            cacheProvider.ClearAll();
            return true;
        }

        #endregion

        #region Simple Example of Add and Retrieve from Cache
        private static bool ExampleAddAndRetrieveFromCache()
        {
            int _accessCounter = 0;
            bool _allTestsPassed = true;

            WriteHeadingSeparator("Simple Add and Retrieve Examples");

            //If you want to programmatically alter the configured values for the cache, you can
            // use the commented section below as an example
            //CacheConfig config = new CacheConfig();
            //config.CacheToUse = CacheTypes.MemoryCache;
            //AppServices.SetConfig(config);
            // Alternatively, you can use the commented method below to set the logging implementation,
            // configuration settings, and resolver to use when determining the ICacheProvider
            // implementation.
            //AppServices.PreStartInitialise(null, config);

            var cacheProvider = AppServices.Cache;

            // First try and get some data. It wont be in the cache, so the anonymous function is executed,
            // the item is automatically added to the cache and returned.

            Console.WriteLine("#1: Getting Some Data.");
            var data1 = cacheProvider.Get<SomeData>("cache-key", DateTime.Now.AddSeconds(5), () =>
            {
                // This is the anonymous function which gets called if the data is not in the cache.
                // This method is executed and whatever is returned, is added to the cache with the
                // passed in expiry time.
                _accessCounter++;
                Console.WriteLine("... => Adding data to the cache... 1st call");
                var someData = new SomeData() { SomeText = "cache example1", SomeNumber = 1 };
                return someData;
            });
            Console.WriteLine("... => SomeData values: SomeText=[{0}], SomeNumber={1}\n", data1.SomeText, data1.SomeNumber);
            if (_accessCounter != 1)
            {
                WriteErrMsgToConsole("Cache not added to, test result failed!");
            }

            // Now try and get some data using the same cache-key and before the cached data expiry time elapses.
            // The data will be located in the cache and returned, and the anonymous function is NOT executed.

            Console.WriteLine("#2: Getting Some More Data which should now be cached.");
            var data2 = cacheProvider.Get<SomeData>("cache-key", DateTime.Now.AddSeconds(3), () =>
            {
                // This is the anonymous function which gets called if the data is not in the cache.
                // This method is executed and whatever is returned, is added to the cache with the
                // passed in expiry time.
                _accessCounter++;
                Console.WriteLine("... => Adding data to the cache...2nd call- should not be displayed!");
                var someData = new SomeData() { SomeText = "cache example2", SomeNumber = 2 };
                return someData;
            });
            Console.WriteLine("... => SomeData values: SomeText=[{0}], SomeNumber={1}\n", data2.SomeText, data2.SomeNumber);
            if (_accessCounter == 2)
            {
                WriteErrMsgToConsole("Data item not found in cachewhen it should have been found in cache, test result failed!");
            }

            // Here we wait a period of time so that the cached data expiry time has passed and the data is
            // removed from the cache. We make the same call to retrieve the data using the same cache key, the data
            // is not found in the cache, so once again the anonymous function is executed, whatever is returned is
            // added to the cache, and then returned to the caller.

            Wait(5);
            Console.WriteLine("#3: Getting Some More Data which should not be cached.");
            var data3 = cacheProvider.Get<SomeData>("cache-key", DateTime.Now.AddSeconds(5), () =>
            {
                // This is the anonymous function which gets called if the data is not in the cache.
                // This method is executed and whatever is returned, is added to the cache with the
                // passed in expiry time.
                _accessCounter++;
                Console.WriteLine("... => Adding data to the cache...3rd call");
                var someData = new SomeData() { SomeText = "cache example3", SomeNumber = 3 };
                return someData;
            });
            Console.WriteLine("... => SomeData values: SomeText=[{0}], SomeNumber={1}\n", data3.SomeText, data3.SomeNumber);
            if (_accessCounter != 2)
            {
                WriteErrMsgToConsole("Cache not added to, test result failed!");
            }


            Func<SomeData> getCacheData = new Func<SomeData>(() =>
                                                                {
                                                                    // This is the anonymous function which gets called if the data is not in the cache.
                                                                    // This method is executed and whatever is returned, is added to the cache with the
                                                                    // passed in expiry time.
                                                                    _accessCounter++;
                                                                    Console.WriteLine("... => Adding data to the cache...4th call, with generated cache key - should only see this msg twice, NOT 3 times");
                                                                    var someData = new SomeData() { SomeText = "cache example4 - generated cache key", SomeNumber = 4 };
                                                                    return someData;

                                                                });
            // Here we use the really simple API call to get an item from the cache without a cache key specified.
            // The cache key is generated from the function we pass in as the delegate used to retrieve the data
            Console.WriteLine("#4: Getting Some Data which should NOT BE cached.");
            var data4 = cacheProvider.Get<SomeData>(DateTime.Now.AddSeconds(3), getCacheData);
            if (_accessCounter != 3)
            {
                WriteErrMsgToConsole("Cache not added to, test result failed!");
                _allTestsPassed = false;
            }

            Console.WriteLine("#5: Getting Some More Data which should BE cached.");
            var data5 = cacheProvider.Get<SomeData>(DateTime.Now.AddSeconds(2), getCacheData);
            if (_accessCounter != 3)
            {
                WriteErrMsgToConsole("Data item not found in cache when it should have been found in cache, test result failed!");
                _allTestsPassed = false;
            }

            Wait(3);
            Console.WriteLine("#6: Getting Some More Data which should NOT be cached.");
            var data6 = cacheProvider.Get<SomeData>(DateTime.Now.AddSeconds(2), getCacheData);
            if (_accessCounter != 4)
            {
                WriteErrMsgToConsole("Cache not added to, test result failed!");
                _allTestsPassed = false;
            }

            return _allTestsPassed;
        }

        #endregion

        #region Examples of setting config programmatically

        private static void ExampleSettingConfigurationViaCode()
        {
            Console.WriteLine("initialising");
            AppServices.PreStartInitialise();

            Console.WriteLine("adding data");
            AppServices.Cache.Add("test", DateTime.Now.AddMinutes(10), "blah");
            Console.WriteLine("clearing");
            AppServices.Cache.ClearAll();

            Console.WriteLine("Setting memcached");
            AppServices.SetConfig(new CacheConfig() { CacheToUse = "memcached", DistributedCacheServers = "localhost:11211" });

            Console.WriteLine("getting data");
            var data = AppServices.Cache.InnerCache.Get<string>("test");
            Console.WriteLine("data is [{0}]", data);

            Console.ReadLine();
        }
        #endregion

        #region Helper Functions
        private static void WriteErrMsgToConsole(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(msg);
            Console.ForegroundColor = _originalColor;
        }

        private static void WriteHeadingSeparator(string heading)
        {
            var headingText = "*** " + heading + " ***";
            var line = new string('*', headingText.Length);

            Console.WriteLine("\n\n");
            Console.WriteLine(line);
            Console.WriteLine(headingText);
            Console.WriteLine(line);
            Console.WriteLine("\n");
        }

        private static void Wait(double timeInSeconds)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("Waiting for {0} seconds...", timeInSeconds);
            System.Threading.Thread.Sleep((int)(timeInSeconds * 1000));
            Console.ForegroundColor = _originalColor;
        }

        #endregion

    }

    #region Just Test Data Class
    [Serializable]
    public class SomeData
    {
        public string SomeText { get; set; }
        public int SomeNumber { get; set; }
    }

    #endregion

}
