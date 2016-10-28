using Glav.CacheAdapter.Bootstrap;
using Glav.CacheAdapter.Core;

namespace Glav.CacheAdapter.Features
{
    public class CacheFeatureSupport : ICacheFeatureSupport
    {
        private readonly CacheConfig _config;
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

            return DetermineIfCahceSupportsClearingContents(_config.CacheToUse);
        }

        private bool DetermineIfCahceSupportsClearingContents(string cacheType)
        {
            // Else use whats in config
            switch (cacheType)
            {
                case CacheTypes.MemoryCache:
                    return true;
                case CacheTypes.WebCache:
                    return true;
                case CacheTypes.AppFabricCache:
                    return false;
                case CacheTypes.memcached:
                    return true;
                case CacheTypes.redis:
                    return true;
                default:
                    return false;

            }
        }
        public bool SupportsClearingCacheContents(ICache cache)
        {
            string cacheType = string.Empty;
            if (cache != null)
            {
                cacheType = cache.CacheType.ToStringType();
            }
            return DetermineIfCahceSupportsClearingContents(cacheType);
        }
    }
}
