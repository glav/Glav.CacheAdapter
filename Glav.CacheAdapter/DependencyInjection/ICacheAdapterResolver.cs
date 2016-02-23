using Glav.CacheAdapter.Core;
using Glav.CacheAdapter.Core.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Glav.CacheAdapter.DependencyInjection
{
    public interface ICacheAdapterResolver
    {
        //ICacheProvider ResolveCacheFromConfig(CacheConfig config, ILogging logger);
        ICacheProvider ResolveCacheFromConfig(CacheConfig config);
        void SetLogger(ILogging logger);

    }
}
