#region Using Statements
using System;
using Glav.CacheAdapter.Core.DependencyInjection;

#endregion

namespace Glav.CacheAdapter.ExampleUsage
{
    class Program
    {
        static void Main(string[] args)
        {
            ExampleAddAndRetrieveFromCache();

            Console.WriteLine("Done.");
            Console.ReadLine();

        }

        private static void ExampleAddAndRetrieveFromCache()
        {
        	var cacheProvider = AppServices.Cache;

            // First try and get some data. It wont be in the cache, so the anonymous function is executed,
            // the item is automatically added to the cache and returned.

            Console.WriteLine("Getting Some Data.");
            var data1 = cacheProvider.Get<SomeData>("cache-key", DateTime.Now.AddSeconds(3), () =>
            {
                // This is the anonymous function which gets called if the data is not in the cache.
                // This method is executed and whatever is returned, is added to the cache with the
                // passed in expiry time.
                Console.WriteLine("... => Adding data to the cache... 1st call");
                var someData = new SomeData() { SomeText = "cache example1", SomeNumber = 1 };
                return someData;
            });
            Console.WriteLine("... => SomeData values: SomeText=[{0}], SomeNumber={1}\n", data1.SomeText, data1.SomeNumber);

            // Now try and get some data using the same cache-key and before the cached data expiry time elapses.
            // The data will be located in the cache and returned, and the anonymous function is NOT executed.

            Console.WriteLine("Getting Some More Data which should now be cached.");
            var data2 = cacheProvider.Get<SomeData>("cache-key", DateTime.Now.AddSeconds(5), () =>
            {
                // This is the anonymous function which gets called if the data is not in the cache.
                // This method is executed and whatever is returned, is added to the cache with the
                // passed in expiry time.
                Console.WriteLine("... => Adding data to the cache...2nd call");
                var someData = new SomeData() { SomeText = "cache example2", SomeNumber = 2 };
                return someData;
            });
            Console.WriteLine("... => SomeData values: SomeText=[{0}], SomeNumber={1}\n", data2.SomeText, data2.SomeNumber);

            // Finally, we wait a period of time so that the cached data expiry time has passed and the data is
            // removed from the cache. We make the same call to retrieve the data using the same cache key, the data
            // is not found in the cache, so once again the anonymous function is executed, whatever is returned is
            // added to the cache, and then returned to the caller.

            System.Threading.Thread.Sleep(4000);
            Console.WriteLine("Getting Some More Data which should not be cached.");
            var data3 = cacheProvider.Get<SomeData>("cache-key", DateTime.Now.AddSeconds(5), () =>
            {
                // This is the anonymous function which gets called if the data is not in the cache.
                // This method is executed and whatever is returned, is added to the cache with the
                // passed in expiry time.
                Console.WriteLine("... => Adding data to the cache...3rd call");
                var someData = new SomeData() { SomeText = "cache example3", SomeNumber = 3 };
                return someData;
            });
            Console.WriteLine("... => SomeData values: SomeText=[{0}], SomeNumber={1}\n", data3.SomeText, data3.SomeNumber);
        }

    }
}
