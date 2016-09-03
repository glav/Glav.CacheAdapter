using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Glav.CacheAdapter.Core.DependencyInjection;
using System.Diagnostics;
using System.Threading;
using Glav.CacheAdapter.Core.Diagnostics;

namespace Glav.CacheAdapter.ExampleUsage
{
    /// <summary>
    /// Some code to stress out the cache
    /// </summary>
    public static class HammerTheCache
    {
        private const int NUMBER_THREADS = 10;
        private const int NUMBER_OPERATIONS_PER_TASK = 200;
        private static readonly TimeSpan MINIMUM_TIME_TO_RUN = TimeSpan.FromSeconds(2);
        private static readonly CacheConfig _config = new CacheConfig();
        public static void StartHammering()
        {
            Console.WriteLine();
            Console.WriteLine("About to perform a simple stress test on the cache.");
            Console.WriteLine("Cache Dependencies enabled:{0}", _config.IsCacheDependencyManagementEnabled);
            Console.WriteLine("Press any key to start.");
            Console.WriteLine(new string('*', 40));
            Console.ReadKey();

            AppServices.SetLogger(new ConsoleLogger());
            StartTestExecution();

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine(new string('*', 40));
            Console.WriteLine("Test Complete.");
            Console.WriteLine("Press any key to end.");
            Console.ReadKey();
        }

        private static void StartTestExecution()
        {
            AppServices.SetLogger(new InMemoryLogger());

            Console.WriteLine("Clearing the cache in preparation...");
            AppServices.Cache.ClearAll();
            Console.WriteLine();
            Console.WriteLine("Starting {0} threads, each performing {1} iterations, running for a minimum time of: {2} minutes {3} seconds", NUMBER_THREADS, NUMBER_OPERATIONS_PER_TASK, MINIMUM_TIME_TO_RUN.Minutes, MINIMUM_TIME_TO_RUN.Seconds);

            Console.WriteLine();
            Console.Write("Up to iteration: #");
            int top = Console.CursorTop;
            int infoTop = Console.CursorTop + 4;
            int left = Console.CursorLeft;
            Stopwatch watch = new Stopwatch();
            long shortestTime = long.MaxValue, longestTime = 0;
            long masterKeyClearTime = 0;
            double total = 0;
            int numTimes = 0;
            var avgTimes = new List<double>();



            var masterKeys = new List<string>();
            List<Thread> storeTasks = new List<Thread>(NUMBER_THREADS);

            watch.Start();
            var currentIterationCount = 0;
            var rnd = new Random(DateTime.Now.Millisecond);

            while (watch.Elapsed < MINIMUM_TIME_TO_RUN)
            {
                currentIterationCount++;
                Console.CursorTop = top;
                Console.CursorLeft = left;
                Console.Write(currentIterationCount);


                for (int tCnt = 0; tCnt < NUMBER_THREADS; tCnt++)
                {
                    var seed = rnd.Next(1, 100);
                    var masterKey1 = string.Format("masterkey-{0}", tCnt);
                    masterKeys.Add(masterKey1);
                    var threadStart = new ThreadStart(() =>
                                                          {
                                                              var key = string.Format("Key-{0}", currentIterationCount * seed);
                                                              var timer = new Stopwatch();
                                                              timer.Start();
                                                              // get the data, which then also adds it
                                                              var testData = AppServices.Cache.Get<MoreDummyData>(key, DateTime.Now.AddMinutes(10), () =>
                                                                                                                                          {
                                                                                                                                              return GetDataToCache(seed);
                                                                                                                                          }, masterKey1);

                                                              if (testData == null)
                                                              {
                                                                  Console.WriteLine("Received NULL for cache retrieval");
                                                              }

                                                              // Now Clear it
                                                              //AppServices.Cache.InvalidateCacheItem(key);

                                                              timer.Stop();
                                                              if (timer.ElapsedMilliseconds > longestTime) { longestTime = timer.ElapsedMilliseconds; }
                                                              if (timer.ElapsedMilliseconds < shortestTime) { shortestTime = timer.ElapsedMilliseconds; }
                                                              total += timer.ElapsedMilliseconds;
                                                              numTimes++;
                                                          });
                    var cacheThread = new Thread(threadStart);
                    storeTasks.Add(cacheThread);
                }

                storeTasks.ForEach(t => t.Start());
                storeTasks.ForEach(t => t.Join());
                storeTasks.Clear();

                Console.CursorTop = infoTop;
                Console.CursorLeft = 0;
                var avgTime = total / (double)numTimes;
                avgTimes.Add(avgTime);
                Console.WriteLine("Avg Time: {0} mseconds", avgTime);
            }

            Console.WriteLine("Clearing child items of master/parent keys...");
            var masterKeyTimer = new Stopwatch();
            masterKeyTimer.Start();
            AppServices.Cache.InvalidateCacheItems(masterKeys);
            masterKeyTimer.Stop();
            masterKeyClearTime = masterKeyTimer.ElapsedMilliseconds;
            total += masterKeyClearTime;

            Console.WriteLine("Number of items cached: {0}", numTimes);
            Console.WriteLine("Shortest Time: {0} mseconds", shortestTime);
            Console.WriteLine("Longest Time: {0} mseconds", longestTime);
            Console.WriteLine("Clearing child items of master keys took: {0} mseconds", masterKeyClearTime);
            InMemoryLogger.FlushToDisk("CacheAdatper.log");
        }

        private static MoreDummyData GetDataToCache(int tCnt)
        {
            Random rnd = new Random(DateTime.Now.Millisecond);
            var data = new MoreDummyData();
            data.Stuff = string.Format("Stuff-{0}", rnd.Next(0, 10));
            for (int d = 0; d < NUMBER_OPERATIONS_PER_TASK; d++)
            {
                data.ListOfStuff.Add(new ItemStuff()
                                        {
                                            ItemIdentifier = string.Format("T:{0}-{1}", tCnt, d),
                                            ItemName = string.Format("Item Task:{0}, Iteration Count: {1}", tCnt, d)
                                        });
            }
            return data;

        }

        static void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            Console.WriteLine(">> Task threw an exception: {0}", e.Exception.Message);
            e.SetObserved();
        }
    }

    public class MoreDummyData
    {
        public string Stuff;
        public List<ItemStuff> ListOfStuff = new List<ItemStuff>();
    }

    public class ItemStuff
    {
        public string ItemName;
        public string ItemIdentifier;
    }

    public class ConsoleLogger : ILogging
    {
        public void WriteInfoMessage(string message)
        {
            Trace.WriteLine(message);
        }

        public void WriteErrorMessage(string message)
        {
            Console.WriteLine(message);
        }

        public void WriteException(Exception ex)
        {
            var msg = string.Format("Exception: {0}, Message:[{1}], StackTrace:[{2}]", ex.GetType(), ex.Message, ex.StackTrace);
            Console.WriteLine(msg);
        }
    }

}
