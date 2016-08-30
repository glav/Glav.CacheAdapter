using Glav.CacheAdapter.Bootstrap;
using Glav.CacheAdapter.Core;

namespace Glav.CacheAdapter.Features
{
    public class DefaultFeatureSupport : ICacheFeatureSupport
    {

        public bool SupportsClearingCacheContents()
        {
            return false;
        }

    }
}
