using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Glav.CacheAdapter.Bootstrap;
using Glav.CacheAdapter.Core;
using Glav.CacheAdapter.Distributed.AppFabric;
using Glav.CacheAdapter.Distributed.memcached;
using Glav.CacheAdapter.Web;

namespace Glav.CacheAdapter.Features
{
    public class CacheFeatureSupport : ICacheFeatureSupport
    {
        private CacheConfig _config;
        private ICache _cache;

        public CacheFeatureSupport()
        {
            _config = new CacheConfig();
        }
        public CacheFeatureSupport(CacheConfig config)
        {
            _config = config;
        }

        public CacheFeatureSupport(ICache cache)
        {
            _cache = cache;
        }

        public ICache Cache { get { return _cache; } set { _cache = value; } }

        public bool SupportsClearingCacheContents()
        {
            if (_cache != null)
            {
                return SupportsClearingCacheContents(_cache);
            }

            // Else use whats in config
            switch (_config.CacheToUse)
            {
                case CacheTypes.MemoryCache:
                    return true;
                    break;
                case CacheTypes.WebCache:
                    return true;
                    break;
                case CacheTypes.AppFabricCache:
                    return false;
                    break;
                case CacheTypes.memcached:
                    return true;
                    break;
                default:
                    return false;
                    break;

            }
        }
        public bool SupportsClearingCacheContents(ICache cache)
        {
            if (cache is AppFabricCacheAdapter)
            {
                return false;
            }

            return true;
        }
    }
}
