using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Glav.CacheAdapter.Distributed.memcached;
using Glav.CacheAdapter.Distributed.memcached.Protocol;

namespace Glav.CacheAdapter.Distributed.memcached
{
	public class CacheServerFarm
	{
		private const int NUMBER_OF_KEYS = 100;

		Dictionary<uint, ServerNode> _serverFarmKeys = new Dictionary<uint, ServerNode>();
		private uint[] _allKeys;
		private List<ServerNode> _nodes;
		private DeadNodePool _deadNodes= new DeadNodePool();
		private static object _lockObject = new object();

		public List<ServerNode> NodeList { get { return _nodes; } }
		public DeadNodePool DeadNodes { get { return _deadNodes; } }

		public void Initialise(List<ServerNode> nodes)
		{
			_nodes = nodes;
			_deadNodes.DeadNodesBackAlive += new EventHandler<DeadNodeBackToLifeEventArgs>(_deadNodes_DeadNodesBackAlive);
			BuildCacheFarmHashKeys();
		}

		void _deadNodes_DeadNodesBackAlive(object sender, DeadNodeBackToLifeEventArgs e)
		{
			foreach (var node in e.NodesBackAlive)
			{
				var formerlyDeadNode = _nodes.Where(n => n.IPAddress == node.IPAddress && n.Port == node.Port && n.IsAlive == false).FirstOrDefault();
				if (formerlyDeadNode != null)
					formerlyDeadNode.IsAlive = true;
			}
			BuildCacheFarmHashKeys();
		}
		
		protected void BuildCacheFarmHashKeys()
		{
			var numberAliveNodes = CheckIfNodesAreAlive(_nodes);
			var keys = new uint[numberAliveNodes * NUMBER_OF_KEYS];
			_serverFarmKeys.Clear();

			int nodeIdx = 0;

			foreach (ServerNode node in _nodes)
			{
				if (node.IsAlive)
				{
					var tmpKeys = GenerateCacheServerKeys(node);

					for (var i = 0; i < tmpKeys.Length; i++)
					{
						_serverFarmKeys[tmpKeys[i]] = node;
					}

					tmpKeys.CopyTo(keys, nodeIdx);
					nodeIdx += NUMBER_OF_KEYS;
				}
			}

			Array.Sort<uint>(keys);
			Interlocked.Exchange(ref _allKeys, keys);
		}

		public void SetNodeToDead(ServerNode node)
		{
			lock (_lockObject)
			{
				var deadNode =
					_nodes.Where(n => n.IPAddress == node.IPAddress && n.Port == node.Port && n.IsAlive == true).FirstOrDefault();
				if (deadNode != null)
				{
					deadNode.IsAlive = false;
					BuildCacheFarmHashKeys();
					_deadNodes.AddDeadNodeToPool(deadNode);
				}
			}
		}

		private int CheckIfNodesAreAlive(List<ServerNode> nodes)
		{
			if (nodes != null)
			{
				nodes.ForEach(n =>
				              	{
				              		var pingCommand = new VersionCommand(n.IPAddress, n.Port);
				              		var response = pingCommand.ExecuteCommand();
									if (response.Status == CommandResponseStatus.Ok)
									{
										n.IsAlive = true;
									}
									else
									{
										n.IsAlive = false;
									}
				              	});
				return nodes.Where(n => n.IsAlive == true).Count();
			}
			return 0;
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

}
