using Glav.CacheAdapter.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Glav.CacheAdapter.Core;

namespace Glav.CacheAdapter.Distributed.AppFabric
{
    class AppFabricFeatureSupport : ICacheFeatureSupport
    {

        public bool SupportsClearingCacheContents()
        {
            return false;
        }

    }
}
