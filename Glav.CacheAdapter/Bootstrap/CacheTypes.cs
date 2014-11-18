using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Glav.CacheAdapter.Bootstrap
{
    public static class CacheTypes
    {
        public const string MemoryCache = "memory";
        public const string WebCache = "web";
        public const string AppFabricCache = "appfabric";
    	public const string memcached = "memcached";
        public const string redis = "redis";

    }
}
