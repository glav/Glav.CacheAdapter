using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Glav.CacheAdapter.Core.DependencyInjection;
using System.Diagnostics;
using System.Threading;

namespace Glav.CacheAdapter.ExampleUsage
{
	/// <summary>
	/// Some code to stress out the cache
	/// </summary>
	public static class HammerTheCache
	{
		private const int NUMBER_THREADS = 1000;
		private const int NUMBER_OPERATIONS_PER_TASK = 200;
		private static readonly TimeSpan MINIMUM_TIME_TO_RUN = TimeSpan.FromMinutes(10);

		public static void StartHammering()
		{
			Console.WriteLine();
			Console.WriteLine("About to perform a simple stress test on the cache.");
			Console.WriteLine("Press any key to start.");
			Console.WriteLine(new string('*',40));
			Console.ReadKey();

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
			Console.WriteLine();
			Console.WriteLine("Starting {0} threads, each performing {1} iterations, running for a minimum time of: {2} minutes", NUMBER_THREADS, NUMBER_OPERATIONS_PER_TASK, MINIMUM_TIME_TO_RUN.Minutes);

			Console.WriteLine();
			Console.Write("Up to iteration: #");
			int top = Console.CursorTop;
			int left = Console.CursorLeft;
			Stopwatch watch = new Stopwatch();

			List<Thread> storeTasks = new List<Thread>(NUMBER_THREADS);

			watch.Start();
			var currentIterationCount = 0;

			while (watch.Elapsed < MINIMUM_TIME_TO_RUN)
			{
				currentIterationCount++;
				Console.CursorTop = top;
				Console.CursorLeft = left;
				Console.Write(currentIterationCount);

				for (int tCnt = 0; tCnt < NUMBER_THREADS; tCnt++)
				{
					var threadStart = new ThreadStart(() =>
					                                  	{
					                                  		var dataToCache = GetDataToCache(tCnt);
					                                  		var testData = AppServices.Cache.Get<MoreDummyData>(dataToCache.Stuff, DateTime.Now.AddMinutes(1), () =>
					                                  		                                                                                                   	{
					                                  		                                                                                                   		return dataToCache;
					                                  		                                                                                                   	});
															if (testData == null)
					                                  		{
					                                  			Console.WriteLine("Received NULL for cache retrieval");
					                                  		}

					                                  	});
					var cacheThread = new Thread(threadStart);
					storeTasks.Add(cacheThread);
				}

				storeTasks.ForEach(t => t.Start());
				storeTasks.ForEach(t => t.Join());
				storeTasks.Clear();
			}
		}

		private static MoreDummyData GetDataToCache(int tCnt)
		{
			Random rnd = new Random(DateTime.Now.Millisecond);
			var data = new MoreDummyData();
			data.Stuff = string.Format("Stuff #{0}", rnd.Next(0,10));
			for (int d = 0; d < NUMBER_OPERATIONS_PER_TASK; d++)
			{
				data.ListOfStuff.Add(new ItemStuff()
				                     	{
				                     		ItemIdentifier = string.Format("T:{0}-{1}",tCnt,d),
											ItemName = string.Format("Item Task:{0}, Iteration Count: {1}",tCnt,d)
				                     	});
			}
			return data;

		}

		static void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
		{
			Console.WriteLine(">> Task threw an exception: {0}",e.Exception.Message);
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
}
