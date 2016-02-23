using System;
using Glav.CacheAdapter.Core.Diagnostics;
using Glav.CacheAdapter.DependencyInjection;

namespace Glav.CacheAdapter.Core.DependencyInjection
{
    /// <summary>
    /// A utility class that provides static, simple access to the cache provider mechanism.
    /// Large scale application may typically forego the use of this class and provide their
    /// own resolution mechanism.
    /// </summary>
    public static class AppServices
    {
        private static ICacheProvider _cacheProvider;
        private static ICache _cache;
        private static bool _isInitialised;
        private static readonly object _lockRef = new object();

        static AppServices()
        {
        }

        public static void SetLogger(ILogging logger)
        {
            _isInitialised = false;
            PreStartInitialise(logger);
        }

        public static void SetConfig(CacheConfig config)
        {
            _isInitialised = false;
            PreStartInitialise(null, config);
        }

        public static void SetResolver(ICacheAdapterResolver resolver)
        {
            _isInitialised = false;
            PreStartInitialise(null, null, resolver);
        }

        public static void SetDependencies(ILogging logger = null, CacheConfig config = null, ICacheAdapterResolver resolver = null)
        {
            _isInitialised = false;
            PreStartInitialise(logger, config, resolver);
        }

        /// <summary>
        /// Initialise the container with core dependencies. The cache/cache provider should be set to be
        /// singletons if adapting to use with a Dependency Injection mechanism
        /// </summary>
        /// <remarks>Note: In a .Net 4 web app, this method could be invoked using the new PreApplicationStartMethod attribute
        /// as in: <code>[assembly: PreApplicationStartMethod(typeof(MyStaticClass), "PreStartInitialise")]</code>
        /// Also note this section should be replaced with the DependencyInjection container of choice. This
        /// code simply acts as a cheap and simple utility mechanism for this without requiring a dependency on a 
        /// container that you dont like/use. You can opt to replace the ICacheAdapterResolver with
        /// your resolver of choice as well to utilise your own dependency resolution mechanism.
        /// </remarks>
        public static void PreStartInitialise(ILogging logger = null, CacheConfig config = null, ICacheAdapterResolver resolver = null)
        {
            if (!_isInitialised)
            {
                lock (_lockRef)
                {
                    if (!_isInitialised)
                    {
                        try
                        {
                            _cacheProvider = CacheBinder.ResolveCacheFromConfig(config, logger, resolver);
                            CacheBinder.Logger.WriteInfoMessage(string.Format("Initialised cache of type: {0}", CacheBinder.Configuration.CacheToUse));
                            _cache = _cacheProvider.InnerCache;
                            _isInitialised = true;
                        }
                        catch (Exception ex)
                        {
                            var outerEx = new ApplicationException(string.Format("Problem initialising cache of type: {0}", CacheBinder.Configuration.CacheToUse), ex);
                            CacheBinder.Logger.WriteException(outerEx);
                            throw outerEx;
                        }
                    }
                }
            }
        }

        public static ICacheProvider Cache
        {
            get
            {
                PreStartInitialise();
                return _cacheProvider;
            }
        }
    }
}
