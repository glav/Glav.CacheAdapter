using Glav.CacheAdapter.Core.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Glav.CacheAdapter.Distributed.Redis
{
    public class RedisCacheFactory : DistributedCacheFactoryBase
    {
        private ILogging _logger;
        private const string DEFAULT_IpAddress = "127.0.0.1";
        private const int DEFAULT_Port = 6379;

        public RedisCacheFactory(ILogging logger, CacheConfig config = null) : base(logger,config)
        {
        }
    }
}
