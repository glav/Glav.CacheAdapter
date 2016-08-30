using System.Collections.Generic;
using Glav.CacheAdapter.Core.Diagnostics;
using Glav.CacheAdapter.Core;

namespace Glav.CacheAdapter.Distributed.memcached
{
	public class CacheServerFarm
	{
		private List<ServerNode> _nodes;
		private ILogging _logger;

		public List<ServerNode> NodeList { get { return _nodes; } }

		public CacheServerFarm(ILogging logger)
		{
			_logger = logger;
		}

		public void Initialise(List<ServerNode> nodes)
		{
			_nodes = nodes;
		}
	}

}
