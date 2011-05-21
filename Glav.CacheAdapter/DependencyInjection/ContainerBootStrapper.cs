using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Unity;
using Glav.CacheAdapter.Core.Diagnostics;

namespace Glav.CacheAdapter.Core.DependencyInjection
{
    /// <summary>
    /// The application bootstrapper to register dependencies into the Unity Container.
    /// These dependencies are common across all tiers. Web tier and application tier specific components should perform
    /// their own dependency registration with the container that is returned from this class
    /// </summary>
    public class ContainerBootStrapper
    {
        public IUnityContainer InitialiseContainer()
        {
            IUnityContainer container = new UnityContainer();
            
            container.RegisterType<IApplicationService, ApplicationService>();

            // Register the logging class as a singleton
            container.RegisterType<ILogging, Logger>(new ContainerControlledLifetimeManager());

            return container;
        }
    }
}
