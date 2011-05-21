using System;
using Microsoft.Practices.Unity;
using System.Collections.Generic;

namespace Glav.CacheAdapter.Core.DependencyInjection
{
    public interface IApplicationService
    {
        IUnityContainer Container { get; }
        bool IsRegistered<T>();
        bool IsRegistered<T>(string nameToCheck);
        T Resolve<T>(params ResolverOverride[] overrides);
        IEnumerable<T> ResolveAll<T>(params ResolverOverride[] overrides);
    }
}
