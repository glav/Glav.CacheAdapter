using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using Glav.CacheAdapter.Distributed.memcached;
using Glav.CacheAdapter.Distributed.memcached.Protocol;

namespace Glav.CacheAdapter.Distributed.memcached
{
	public class DeadNodePool
	{
		private Timer _timer = new Timer(60000);
		private List<ServerNode> _deadNodes = new List<ServerNode>();
		private static object _lockObject = new object();
		public event EventHandler<DeadNodeBackToLifeEventArgs> DeadNodesBackAlive;
		
		public DeadNodePool()
		{
			_timer.Elapsed += new ElapsedEventHandler(_timer_Elapsed);
			_timer.Start();
		}

		void _timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			if (_deadNodes.Count > 0)
			{
				var aliveNodes = new List<ServerNode>();
				// If we have some dead nodes, then ping them to see if they still dead
				lock(_lockObject)
				{
					foreach (var node in _deadNodes)
					{
						var verCmd = new VersionCommand(node.IPAddressOrHostName, node.Port);
						var response = verCmd.ExecuteCommand();
						if (response.Status == CommandResponseStatus.Ok)
						{
							aliveNodes.Add(node);
						}
					}
					aliveNodes.ForEach(an => _deadNodes.Remove(an));
				}

				if (aliveNodes.Count > 0)
				{
					FireDeadNodesBackAliveEvent(aliveNodes);
				}
			}
		}

		void FireDeadNodesBackAliveEvent(List<ServerNode> nodes)
		{
			if (DeadNodesBackAlive != null && nodes != null && nodes.Count > 0)
			{
				DeadNodesBackAlive(this, new DeadNodeBackToLifeEventArgs(nodes));
			}
		}

		public void AddDeadNodeToPool(ServerNode node)
		{
			if (node == null)
				return;

			lock (_lockObject)
			{
				_deadNodes.Add(node);
			}
		}
	}

	public class DeadNodeBackToLifeEventArgs: EventArgs
	{
		public DeadNodeBackToLifeEventArgs(List<ServerNode> nodesBackToLife)
		{
			NodesBackAlive = nodesBackToLife;
		}
		public List<ServerNode> NodesBackAlive { get; set; }
	}
}
