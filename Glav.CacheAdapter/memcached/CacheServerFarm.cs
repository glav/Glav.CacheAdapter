using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Glav.CacheAdapter.memcached
{
	class CacheServerFarm
	{
		private const int NUMBER_OF_KEYS = 100;

		Dictionary<uint, ServerNode> _serverFarmKeys = new Dictionary<uint, ServerNode>();
		private uint[] _allKeys;

		public void BuildCacheFarmHashKeys(List<ServerNode> nodes)
		{
			var keys = new uint[nodes.Count * NUMBER_OF_KEYS];

			int nodeIdx = 0;

			foreach (ServerNode node in nodes)
			{
				var tmpKeys = GenerateCacheServerKeys(node);

				for (var i = 0; i < tmpKeys.Length; i++)
				{
					_serverFarmKeys[tmpKeys[i]] = node;
				}

				tmpKeys.CopyTo(keys, nodeIdx);
				nodeIdx += NUMBER_OF_KEYS;
			}

			Array.Sort<uint>(keys);
			Interlocked.Exchange(ref _allKeys, keys);
		}

		public ServerNode FindCacheServerNodeForKey(string key)
		{
			if (_serverFarmKeys.Count == 0) return null;

			uint itemKeyHash = BitConverter.ToUInt32(new DistributedFNV().ComputeHash(Encoding.UTF8.GetBytes(key)), 0);
			int foundIndex = Array.BinarySearch<uint>(_serverFarmKeys.Keys.ToArray(), itemKeyHash);

			// no exact match was found in the BinarySearch
			if (foundIndex < 0)
			{
				// The Binary search provides a negative value which is the bitwise complement of the nearest match so we need to 
				// to "flip" the value to get the positive index
				foundIndex = ~foundIndex;

				if (foundIndex == 0)
				{
					foundIndex = _serverFarmKeys.Count - 1;
				}
				else if (foundIndex >= _serverFarmKeys.Count)
				{
					foundIndex = 0;
				}
			}

			if (foundIndex < 0 || foundIndex > _serverFarmKeys.Count)
				return null;

			return _serverFarmKeys[_allKeys[foundIndex]];
		}

		/// <summary>
		/// We generate a set of potential keys for a server node based off its address. We then later use these keys to get
		/// a match when searching for the cache key hash, or a close match
		/// </summary>
		/// <param name="node"></param>
		/// <returns></returns>
		private uint[] GenerateCacheServerKeys(ServerNode node)
		{
			const int KeyLength = 4;

			var keyList = new uint[NUMBER_OF_KEYS];

			string address = node.GetFullHostAddress();
			var fnv = new DistributedFNV();

			for (int keyCount = 0; keyCount < NUMBER_OF_KEYS; keyCount++) 
			{
				byte[] data = fnv.ComputeHash(Encoding.ASCII.GetBytes(String.Concat(address, "-", keyCount)));

				keyList[keyCount] = BitConverter.ToUInt32(data, 0);
			}

			return keyList;
		}

	}

	public class ServerNode
	{
		public ServerNode()
		{
			IsAlive = true;
		}
		public string IPAddress { get; set; }
		public string Port { get; set; }
		public bool IsAlive { get; set; }

		public string GetFullHostAddress()
		{
			if (string.IsNullOrWhiteSpace(Port))
				Port = "11211";
			if (string.IsNullOrWhiteSpace(IPAddress))
				IPAddress = "127.0.0.1";

			return string.Format("{0}:{1}", IPAddress, Port);
		}
	}
}
