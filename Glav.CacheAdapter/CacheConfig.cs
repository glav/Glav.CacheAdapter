using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Glav.CacheAdapter.Distributed;

namespace Glav.CacheAdapter
{
	public class CacheConfig
	{
		private List<ServerNode> _serverNodes = new List<ServerNode>();
		public List<ServerNode> ServerNodes { get { return _serverNodes; } }

		private List<KeyValuePair<string,string>> _providerSpecificValues = new List<KeyValuePair<string, string>>();
		public List<KeyValuePair<string, string>> ProviderSpecificValues { get { return _providerSpecificValues; } }

	}
}
