using Glav.CacheAdapter.Core;

namespace Glav.CacheAdapter.Features
{
    public interface ICacheFeatureSupport
    {
        bool SupportsClearingCacheContents();
    }
}
