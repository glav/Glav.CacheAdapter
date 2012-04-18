using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Glav.CacheAdapter.Distributed.memcached
{
	public class memcachedConstants
	{
		public const string CONFIG_MinimumConnectionPoolSize = "MinPoolSize";
		public const string CONFIG_MaximumConnectionPoolSize = "MaxPoolSize";
		public const string CONFIG_ConnectionTimeout = "ConnectionTimeout";
		public const string CONFIG_DeadNodeTimeout = "DeadNodeTimeout";
	}
}
