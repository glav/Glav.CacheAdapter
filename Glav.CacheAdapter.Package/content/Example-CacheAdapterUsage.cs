#region Using Statements
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Glav.CacheAdapter.Core.DependencyInjection;
using Glav.CacheAdapter.Core;
using Glav.CacheAdapter.Web;
using Glav.CacheAdapter.Distributed;
using Glav.CacheAdapter.Bootstrap;
#endregion

namespace Glav.CacheAdapter.ExampleUsage
{
    class Program
    {
        static void Main(string[] args)
        {
            // Must initialise all the cache dependencies first before we can resolve any cache mechanisms.
            CacheBootstrapper.InitialiseCache();

            ExampleAddAndRetrieveFromCache();

            Console.WriteLine("Done.");
            Console.ReadLine();

        }

        private static void ExampleAddAndRetrieveFromCache()
        {
            // Resolve our cache adapter implementation using Dependency injection (Unity).
            var cacheProvider = AppServices.Resolve<ICacheProvider>();

            // First try and get some data. It wont be in the cache, so the anonymous function is executed,
            // the item is automatically added to the cache and returned.

            Console.WriteLine("Getting Some Data.");
            var cachedata = cacheProvider.Get<string>("cache-key", DateTime.Now.AddSeconds(5), () =>
            {
                // This is the anonymous function which gets called if the data is not in the cache.
                // This method is executed and whatever is returned, is added to the cache with the
                // passed in expiry time.
                Console.WriteLine("... => Adding data to the cache... 1st call");
                string data = string.Format("Data was created at {0}", DateTime.Now.ToLongTimeString());
                return data;
            });

            Console.WriteLine("Data is: [{0}]", cachedata);

            Console.WriteLine("Waiting 2 seconds..");
            System.Threading.Thread.Sleep(2000);
            cachedata = cacheProvider.Get<string>("cache-key", DateTime.Now.AddSeconds(5), () =>
            {
                string data = string.Format("Data was created at {0}", DateTime.Now.ToLongTimeString());
                return data;
            });
            Console.WriteLine("Data is: [{0}]", cachedata);
            Console.WriteLine("Done");
            Console.ReadLine();
        }

    }
}
