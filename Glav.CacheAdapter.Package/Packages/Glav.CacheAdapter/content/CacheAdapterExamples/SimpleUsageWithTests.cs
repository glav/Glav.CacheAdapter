using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Glav.CacheAdapter.Helpers;

namespace Glav.CacheAdapter.ExampleUsage
{
    class SimpleUsageWithTests
    {
        public static bool ExampleAddAndRetrieveFromCache()
        {
            int _accessCounter = 0;
            bool _allTestsPassed = true;

            ConsoleHelper.WriteHeadingSeparator("Simple Add and Retrieve Examples");

            var cacheProvider = CacheConfig
                .Create()
                .BuildCacheProvider(new InMemoryLogger());  // Use the settings from config

            // Can do via dependency helper too
            //var cacheProvider = AppServices.Cache;

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
                ConsoleHelper.WriteErrMsgToConsole("Cache not added to, test result failed!");
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
                ConsoleHelper.WriteErrMsgToConsole("Data item not found in cachewhen it should have been found in cache, test result failed!");
            }

            // Here we wait a period of time so that the cached data expiry time has passed and the data is
            // removed from the cache. We make the same call to retrieve the data using the same cache key, the data
            // is not found in the cache, so once again the anonymous function is executed, whatever is returned is
            // added to the cache, and then returned to the caller.

            ConsoleHelper.Wait(5);
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
                ConsoleHelper.WriteErrMsgToConsole("Cache not added to, test result failed!");
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
                ConsoleHelper.WriteErrMsgToConsole("Cache not added to, test result failed!");
                _allTestsPassed = false;
            }

            Console.WriteLine("#5: Getting Some More Data which should BE cached.");
            var data5 = cacheProvider.Get<SomeData>(DateTime.Now.AddSeconds(2), getCacheData);
            if (_accessCounter != 3)
            {
                ConsoleHelper.WriteErrMsgToConsole("Data item not found in cache when it should have been found in cache, test result failed!");
                _allTestsPassed = false;
            }

            ConsoleHelper.Wait(3);
            Console.WriteLine("#6: Getting Some More Data which should NOT be cached.");
            var data6 = cacheProvider.Get<SomeData>(DateTime.Now.AddSeconds(2), getCacheData);
            if (_accessCounter != 4)
            {
                ConsoleHelper.WriteErrMsgToConsole("Cache not added to, test result failed!");
                _allTestsPassed = false;
            }

            return _allTestsPassed;
        }
    }
}
