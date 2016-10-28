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
        static void Main(string[] args)
        {
            
            // Basic examples usage
            var allTestsPassed = SimpleUsageWithTests.ExampleAddAndRetrieveFromCache();

            if (allTestsPassed)
            {
                // Basic dependency management usage - if not enabled in the
                // app.config file, these wont work.
                allTestsPassed = SimpleUsageWithDependencies.ExampleAddAndClearWithDependencies();
            }

            if (allTestsPassed)
            {
                allTestsPassed = SimpleUsageAsync.ExampleAddUsingAsyncCalls();
            }

            if (allTestsPassed)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("All Tests passed.");
            }
            else
            {
                ConsoleHelper.WriteErrMsgToConsole("One or more tests failed.");
            }

            // Uncomment this line to simulate about 100,000 hits to the cache engine. 
            // This is only available if you have download/copied/cloned the entire solutions source code. If
            // you have simply installed the nuget package, uncommenting this line will cause an error since
            // the cache 'hammering' class in not included in the nuget package.
            //HammerTheCache.StartHammering();

            Console.WriteLine("Done.");
            Console.ReadLine();

        }

    }


}
