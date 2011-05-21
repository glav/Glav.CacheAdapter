using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Glav.CacheAdapter.Core.DependencyInjection;
using Glav.CacheAdapter.Core;
using Glav.CacheAdapter.Web;
using Glav.CacheAdapter.Distributed;
using Microsoft.Practices.Unity;

namespace Glav.CacheAdapter.Bootstrap
{
    public static class CacheBootstrapper
    {
        /// <summary>
        /// Initialise the container with any cache dependencies depending on configuration.
        /// </summary>
        /// <remarks>Note: In a .Net 4 web app, this method could be invoked using the new PreApplicationStartMethod attribute
        /// as in: <code>[assembly: PreApplicationStartMethod(typeof(MyStaticClass), "InitialiseCache")]</code>
        /// </remarks>
        public static void InitialiseCache()
        {
            AppServices.PreStartInitialise();
            switch (MainConfig.Default.CacheToUse.ToLowerInvariant())
            {
                case CacheTypes.MemoryCache:
                    AppServices.Container.RegisterType<ICache, MemoryCacheAdapter>(new ContainerControlledLifetimeManager());
                    break;
                case CacheTypes.WebCache:
                    AppServices.Container.RegisterType<ICache, WebCacheAdapter>(new ContainerControlledLifetimeManager());
                    break;
                case CacheTypes.AppFabricCache:
                    AppServices.Container.RegisterType<ICache, AppFabricCacheAdapter>(new ContainerControlledLifetimeManager());
                    break;
                default:
                    AppServices.Container.RegisterType<ICache, MemoryCacheAdapter>(new ContainerControlledLifetimeManager());
                    break;
            }
            AppServices.Container.RegisterType<ICacheProvider, CacheProvider>(new ContainerControlledLifetimeManager());

        }
    }
}
