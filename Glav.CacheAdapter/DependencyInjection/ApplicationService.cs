using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Unity;

namespace Glav.CacheAdapter.Core.DependencyInjection
{
    public class ApplicationService : Glav.CacheAdapter.Core.DependencyInjection.IApplicationService
    {
        private IUnityContainer _container = AppServices.Container;
        public IUnityContainer Container { get { return _container; } }

        public T Resolve<T>(params ResolverOverride[] overrides)
        {
            return Container.Resolve<T>(overrides);
        }

        public IEnumerable<T> ResolveAll<T>(params ResolverOverride[] overrides)
        {
            return Container.ResolveAll<T>(overrides);
        }

        public bool IsRegistered<T>()
        {
            return Container.IsRegistered<T>();
        }

        public bool IsRegistered<T>(string nameToCheck)
        {
            return Container.IsRegistered<T>(nameToCheck);
        }
    }
}
