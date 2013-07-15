using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
