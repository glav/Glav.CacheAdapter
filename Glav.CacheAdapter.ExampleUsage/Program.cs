#region Using Statements
using System;
using Glav.CacheAdapter.Core.DependencyInjection;
using Glav.CacheAdapter.Bootstrap;

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
	    private static int _accessCounter = 0;
	    private static bool _allTestsPassed = true;
        
        static void Main(string[] args)
        {
            // Basic examples usage
            ExampleAddAndRetrieveFromCache();

            // Basic dependency management usage - if not enabled in the
            // app.config file, these wont work.
            ExampleAddAndClearWithDependencies();

            // Uncomment this line to simulate about 100,000 hits to the cache engine
            //HammerTheCache.StartHammering();

            // Uncomment this line to see how config can be changed/supplied programmatically.
            //  The example switches the config to use memcached to you need that running
            //ExampleSettingConfigurationViaCode();

            if (_allTestsPassed)
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

        #region Examples Adding and Clearing with Dependencies
        private static void ExampleAddAndClearWithDependencies()
        {
            Console.WriteLine("\n*** Simple Cache Dependency examples\n");
            var cacheProvider = AppServices.Cache;

            Console.WriteLine("1. Adding the main data to cache which acts as the trigger for the other added items");
            var masterData = cacheProvider.Get<string>("MasterData", DateTime.Now.AddDays(1), () => "Master Data Item");

            Console.WriteLine("\n2. Attempting to get some related Data from cache provider which will\nimplicitly add it to the cache and related it to the 'MasterItem'\nadded previously.");
            var bitOfRelatedData1 = cacheProvider.Get<string>("Bit1", DateTime.Now.AddDays(1), () => "Some Bit Of Data1","MasterData");
            var bitOfRelatedData2 = cacheProvider.Get<string>("Bit2", DateTime.Now.AddDays(1), () => "Some Bit Of Data2","MasterData");

            Console.WriteLine("\n3. Make sure everything is in the cache as expected.");
            var bit1Ok = cacheProvider.InnerCache.Get<string>("Bit1") != null;
            var bit2Ok = cacheProvider.InnerCache.Get<string>("Bit2") != null;
            var masterDataOk = cacheProvider.InnerCache.Get<string>("MasterData") != null;

            Console.WriteLine("Bit1 is in the cache?:{0}", bit1Ok);
            Console.WriteLine("Bit2 is in the cache?:{0}", bit2Ok);
            Console.WriteLine("MasterData item is in the cache?:{0}", masterDataOk);


            if (!bit1Ok || !bit2Ok || !masterDataOk)
            {
                WriteErrMsgToConsole("Items were not present in cache.");
                return;
            }

            Console.WriteLine("\n5. Now clearing the master data item which should also clear the related items");
            cacheProvider.InvalidateCacheItem("MasterData");

            Console.WriteLine("\n6. Make sure everything is NOT in the cache as expected.");
            bit1Ok = cacheProvider.InnerCache.Get<string>("Bit1") == null;
            bit2Ok = cacheProvider.InnerCache.Get<string>("Bit2") == null;
            masterDataOk = cacheProvider.InnerCache.Get<string>("MasterData") == null;

            Console.WriteLine("Bit1 is NOT in the cache?:{0}", bit1Ok);
            Console.WriteLine("Bit2 is NOT in the cache?:{0}", bit2Ok);
            Console.WriteLine("MasterData item is NOT in the cache?:{0}", masterDataOk);

            if (!bit1Ok || !bit2Ok || !masterDataOk)
            {
                WriteErrMsgToConsole("Items were present in cache when they should have been removed.");
                return;
            }

            Console.WriteLine("All dependencies worked as expected.");

            cacheProvider.ClearAll();
        }

        #endregion

        #region Simple Example of Add and Retrieve from Cache
        private static void ExampleAddAndRetrieveFromCache()
        {
            Console.WriteLine("*** Simple Add and Retrieve Examples\n");

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

            Console.WriteLine("Getting Some Data.");
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

            Console.WriteLine("Getting Some More Data which should now be cached.");
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

            System.Threading.Thread.Sleep(5000);
            Console.WriteLine("Getting Some More Data which should not be cached.");
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
			Console.WriteLine("Getting Some Data which should NOT BE cached.");
			var data4 = cacheProvider.Get<SomeData>(DateTime.Now.AddSeconds(2), getCacheData );
            if (_accessCounter != 3)
            {
                WriteErrMsgToConsole("Cache not added to, test result failed!");
            }

        	System.Threading.Thread.Sleep(1000);
			Console.WriteLine("Getting Some More Data which should BE cached.");
			var data5 = cacheProvider.Get<SomeData>(DateTime.Now.AddSeconds(2), getCacheData );
            if (_accessCounter != 3)
            {
                WriteErrMsgToConsole("Data item not found in cache when it should have been found in cache, test result failed!");
            }

			System.Threading.Thread.Sleep(3000);
			Console.WriteLine("Getting Some More Data which should NOT be cached.");
			var data6 = cacheProvider.Get<SomeData>(DateTime.Now.AddSeconds(2), getCacheData );
            if (_accessCounter != 4)
            {
                WriteErrMsgToConsole("Cache not added to, test result failed!");
            }

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
            AppServices.SetConfig(new Glav.CacheAdapter.CacheConfig() { CacheToUse = "memcached", DistributedCacheServers = "localhost:11211" });

            Console.WriteLine("getting data");
            var data = AppServices.Cache.InnerCache.Get<string>("test");
            Console.WriteLine("data is [{0}]", data);

            Console.ReadLine();
        }
        #endregion

        #region Helper Functions
        private static void WriteErrMsgToConsole(string msg)
        {
            var originalColour = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(msg);
            Console.ForegroundColor = originalColour;
            _allTestsPassed = false;
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
