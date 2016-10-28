using System;
using Glav.CacheAdapter.Core.Diagnostics;
using Glav.CacheAdapter.DependencyInjection;

namespace Glav.CacheAdapter.Core.DependencyInjection
{
    public static class CacheBinder
    {
        private static CacheConfig _config;
        private static ILogging _logger;
        private static ICacheAdapterResolver _resolver;

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
        /// <returns></returns>
        public static ICacheProvider ResolveCacheFromConfig(CacheConfig config, ILogging logger = null, ICacheAdapterResolver resolver = null)
        {
            _config = config;
            _logger = logger;
            _resolver = resolver;
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
            if (_resolver == null)
            {
                _resolver = new CacheAdapterResolver(_logger);
            }
        }
    }
}
