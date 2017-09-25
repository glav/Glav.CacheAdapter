using Glav.CacheAdapter.Features;

namespace Glav.CacheAdapter.Memory
{
    class MemoryFeatureSupport : ICacheFeatureSupport
    {

        public bool SupportsClearingCacheContents()
        {
            return true;
        }

    }
}
