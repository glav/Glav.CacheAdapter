using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Enyim.Caching;
using Glav.CacheAdapter.Bootstrap;
using Glav.CacheAdapter.Distributed.AppFabric;
using Glav.CacheAdapter.Web;
using Glav.CacheAdapter.Core.Diagnostics;
using Glav.CacheAdapter.Distributed.memcached;

namespace Glav.CacheAdapter.Core.DependencyInjection
{
    public static class AppServices
    {
    	private static ICacheProvider _cacheProvider;
    	private static ICache _cache;
    	private static ILogging _logger;
    	private static bool _isInitialised = false;
		private static readonly object _lockRef = new object();
        private static readonly CacheConfig _config = new CacheConfig();

    	static AppServices()
    	{
    		PreStartInitialise();
    	}

		public static void SetLogger(ILogging logger)
		{
			_logger = logger;
			_isInitialised = false;
			PreStartInitialise();
		}

        /// <summary>
        /// Initialise the container with core dependencies. The cache/cache provider should be set to be
        /// singletons if adapting to use with a Dependency Injection mechanism
        /// </summary>
        /// <remarks>Note: In a .Net 4 web app, this method could be invoked using the new PreApplicationStartMethod attribute
        /// as in: <code>[assembly: PreApplicationStartMethod(typeof(MyStaticClass), "PreStartInitialise")]</code>
        /// Also note this section should be replaced with the DependencyInjection container of choice. This
        /// code simply acts as a cheap mechanism for this without requiring a dependency on a 
        /// container that you dont like/use.
        /// </remarks>
        public static void PreStartInitialise()
        {
			if (!_isInitialised)
			{
				lock (_lockRef)
				{
					if (!_isInitialised)
					{
						_isInitialised = true;
						_cacheProvider = CacheBinder.ResolveCacheFromConfig(_logger,_config.CacheToUse);
						_cache = _cacheProvider.InnerCache;
					}
				}
			}
        }

		public static ICacheProvider Cache { get { return _cacheProvider; } }
    }
}
