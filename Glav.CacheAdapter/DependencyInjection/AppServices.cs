using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Unity;

namespace Glav.CacheAdapter.Core.DependencyInjection
{
    public static class AppServices
    {
        private static bool _isInitialised;
        private static IUnityContainer _container;

        public static IUnityContainer Container
        {
            get { return _container; }
        }

        /// <summary>
        /// Initialise the container with core dependencies.
        /// </summary>
        /// <remarks>Note: In a .Net 4 web app, this method could be invoked using the new PreApplicationStartMethod attribute
        /// as in: <code>[assembly: PreApplicationStartMethod(typeof(MyStaticClass), "PreStartInitialise")]</code>
        /// </remarks>
        public static void PreStartInitialise()
        {
            if (_isInitialised)
                return;

            // Initialise the IoC container
            var bootstrapper = new ContainerBootStrapper();
            _container = bootstrapper.InitialiseContainer();

            _isInitialised = true;
        }

        /// <summary>
        /// A convenience method so that consumers do not have to include Microsoft.Practices as a using
        /// statement to be able to use the strongly typed extension methods.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="overrides"></param>
        /// <returns></returns>
        public static T Resolve<T>(params ResolverOverride[] overrides)
        {
            if (_container == null)
                PreStartInitialise();
            return _container.Resolve<T>(overrides);
        }
    }
}
