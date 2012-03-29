using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Glav.CacheAdapter.Core.DependencyInjection;

namespace Glav.CacheAdapter.ExampleUsage
{
	/// <summary>
	/// Some code to stress out the cache
	/// </summary>
	public static class HammerTheCache
	{
		private const int NUMBER_TASKS = 5000;
		private const int NUMBER_OPERATIONS_PER_TASK = 200;

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
			Console.WriteLine("Starting {0} tasks, each performing {1} iterations",NUMBER_TASKS,NUMBER_OPERATIONS_PER_TASK);

			System.Threading.Tasks.TaskScheduler.UnobservedTaskException += new EventHandler<UnobservedTaskExceptionEventArgs>(TaskScheduler_UnobservedTaskException);
			List<Task> storeTasks = new List<Task>(NUMBER_TASKS);
			for (int tCnt = 0; tCnt < NUMBER_TASKS; tCnt++)
			{
				var storeCacheTask = new Task(() =>
				                         	{
				                         		var dataToCache = GetDataToCache(tCnt);
												var testData = AppServices.Cache.Get<MoreDummyData>(dataToCache.Stuff,DateTime.Now.AddMinutes(1),() =>
				                         		{
													Console.Write(".");
				                         			return dataToCache;
				                         		});
												if (testData == null)
												{
													Console.WriteLine("Received NULL for cache retrieval");
												}
												
				                         	},TaskCreationOptions.PreferFairness);
				storeTasks.Add(storeCacheTask);
			}

			storeTasks.ForEach(t => t.Start());
			Console.WriteLine("All processes started running....");
			Task.WaitAll(storeTasks.ToArray());
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
