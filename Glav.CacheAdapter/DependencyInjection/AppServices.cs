using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Glav.CacheAdapter.Bootstrap;
using Glav.CacheAdapter.Distributed.AppFabric;
using Glav.CacheAdapter.Web;
using Glav.CacheAdapter.Core.Diagnostics;

namespace Glav.CacheAdapter.Core.DependencyInjection
{
    public static class AppServices
    {
    	private static ICacheProvider _cacheProvider;
    	private static ICache _cache;
    	private static ILogging _logger;

    	static AppServices()
    	{
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
        	_logger = new Logger();
			switch (MainConfig.Default.CacheToUse.ToLowerInvariant())
			{
				case CacheTypes.MemoryCache:
					_cache = new MemoryCacheAdapter(_logger);
					break;
				case CacheTypes.WebCache:
					_cache = new WebCacheAdapter(_logger);
					break;
				case CacheTypes.AppFabricCache:
					_cache = new AppFabricCacheAdapter(_logger);
					break;
				default:
					_cache = new MemoryCacheAdapter(_logger);
					break;
			}
			_cacheProvider = new CacheProvider(_cache,_logger);
		}

		public static ICacheProvider Cache { get { return _cacheProvider; } }
    }
}
