using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Glav.CacheAdapter.Helpers;

namespace Glav.CacheAdapter.ExampleUsage
{
    static class SimpleUsageWithDependencies
    {
        public static bool ExampleAddAndClearWithDependencies()
        {
            ConsoleHelper.WriteHeadingSeparator("Simple Cache Dependency examples");

            // Use cache from config file.
            var cacheProvider = CacheConfig.Create()
                .BuildCacheProvider(new InMemoryLogger());

            cacheProvider.ClearAll();
            const string masterDataKey = "MasterData";


            Console.WriteLine("1. Adding the main data to cache which acts as the trigger for the other added items");
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
                ConsoleHelper.WriteErrMsgToConsole("Items were not present in cache.");
                return false;
            }

            Console.WriteLine("\n5. Now clearing the master data item which should also clear the related items");
            cacheProvider.InvalidateCacheItem(masterDataKey);
            ConsoleHelper.Wait(2);

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
                ConsoleHelper.WriteErrMsgToConsole("Items were present in cache when they should have been removed.");
                return false;
            }

            Console.WriteLine("All dependencies worked as expected.");

            cacheProvider.ClearAll();
            return true;
        }

    }
}
