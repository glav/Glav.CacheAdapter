using Glav.CacheAdapter.Core;

namespace Glav.CacheAdapter.Features
{
    public interface ICacheFeatureSupport
    {
        bool SupportsClearingCacheContents(ICache cache);
        bool SupportsClearingCacheContents();
        ICache Cache { get; set; }
    }
}
