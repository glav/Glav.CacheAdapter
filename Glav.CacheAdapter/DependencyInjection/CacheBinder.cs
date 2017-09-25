using System;
using Glav.CacheAdapter.Core.Diagnostics;
using Glav.CacheAdapter.DependencyInjection;
using Glav.CacheAdapter.Serialisation;

namespace Glav.CacheAdapter.Core.DependencyInjection
{
    public static class CacheBinder
    {
        private static CacheConfig _config;
        private static ILogging _logger;
        private static ICacheAdapterResolver _resolver;
        private static ICacheFactoryAssemblyResolver _cacheFactoryAssemblyResolver;

        public static CacheConfig Configuration
        {
            get { return _config; }
        }
        public static ILogging Logger
        {
            get { return _logger; }
        }
        public static ICacheAdapterResolver Resolver
        {
            get { return _resolver; }
        }

        public static ICacheFactoryAssemblyResolver FactoryAssemblyResolver
        {
            get { return _cacheFactoryAssemblyResolver; }
        }

        /// <summary>
        /// Construct the ICacheProvider implementation using the configuration provided, the
        /// logging implementation provided, and the cache adapter resolver provided. If
        /// any of these items are passed as null, the existing values/settings are used. If there
        /// are no existing settings, then the default implementation is used.
        /// </summary>
        /// <param name="config">Configuration to use when creating the cache engine. if NULL
        /// is passed in, then the configuration is created based on values in the configuration file.</param>
        /// <param name="logger">The logging implementation to use. If NULL is provided, then the
        /// generic logging implementation is used.</param>
        /// <param name="resolver">The CacheAdapter resolver to use. If NULL is provided, the 
        /// default resolver is used. You would typically provided your own resolver if you
        /// wanted to use different dependency resolution engines such as Ninject or Autofac which
        /// provide much richer lifetime support.</param>
        /// <param name="cacheFactoryAssemblyResolver">The CacheFactory assembly resolver to use. If NULL is provided the
        /// default factory resolver is used. This is responsible for resolving external assemblies such as those from
        /// the distributed cache nuget packages which are separate to this core assembly.</param>
        /// <returns></returns>
        public static ICacheProvider ResolveCacheFromConfig(CacheConfig config, ILogging logger = null, 
                ICacheAdapterResolver resolver = null, ICacheFactoryAssemblyResolver cacheFactoryAssemblyResolver = null)
        {
            _config = config;
            _logger = logger;
            _resolver = resolver;
            _cacheFactoryAssemblyResolver = cacheFactoryAssemblyResolver;
            EnsureObjectPropertiesAreValidObjects();
            return _resolver.ResolveCacheFromConfig(_config);
        }

        private static void EnsureObjectPropertiesAreValidObjects()
        {
            if (_config == null)
            {
                _config = new CacheConfig();
            }
            if (_logger == null)
            {
                _logger = new Logger(_config);
            }
            if (_cacheFactoryAssemblyResolver == null)
            {
                _cacheFactoryAssemblyResolver = new CacheFactoryAssemblyResolver(_logger);
            }
            if (_resolver == null)
            {
                _resolver = new CacheAdapterResolver(_logger,_cacheFactoryAssemblyResolver);
            }
            if (_config.ObjectSerialiser == null)
            {
                _config.ObjectSerialiser = new DefaultDataContractSerialiser();
            }
        }
    }
}
