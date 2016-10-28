using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Glav.CacheAdapter.Helpers;

namespace Glav.CacheAdapter.ExampleUsage
{
    static class SimpleUsageAsync
    {
        public static bool ExampleAddUsingAsyncCalls()
        {
            ConsoleHelper.WriteHeadingSeparator("Simple Cache examples using Async calls");
            var cacheProvider = CacheConfig.Create().BuildCacheProvider(new InMemoryLogger());

            cacheProvider.ClearAll();
            const string cacheData = "AsyncData";
            const string cacheKey = "async-key";

            Console.WriteLine("Call GetAsync with long running task to populate cache");
            var expensiveData = cacheProvider.GetAsync<string>(cacheKey, DateTime.Now.AddDays(1), () =>
            {
                return System.Threading.Tasks.Task.Run(() =>
                {
                    ConsoleHelper.Wait(2);  // simulate some long running operation
                    return cacheData;
                });
            });

            Console.WriteLine("...Doing some small work and then checking if item is in cache yet (which it should not be)");
            // Do some work
            ConsoleHelper.Wait(1);
            var doesCacheItemExistInCacheYet = (cacheProvider.InnerCache.Get<string>(cacheKey) != null);

            if (doesCacheItemExistInCacheYet == true)
            {
                ConsoleHelper.WriteErrMsgToConsole("Cache item was already in cache when it was not expected to be in there yet via the async method");
                return false;
            }

            Console.WriteLine("...Doing a little more work which should take longer than the async task that places data in cache so we expect it to exist in cache.");
            ConsoleHelper.Wait(1.1);

            var doesCacheItemExistInCacheNow = (cacheProvider.InnerCache.Get<string>(cacheKey) != null);
            if (doesCacheItemExistInCacheNow == false)
            {
                ConsoleHelper.WriteErrMsgToConsole("Cache item was NOT in cache when it was expected to be in there via the async method");
                return false;
            }

            Console.WriteLine(">> Item was placed in cache via async method as expected\n\n");
            return true;
        }
    }
}
