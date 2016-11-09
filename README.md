# Glav.CacheAdapter - README #

This library allows developers to code against a single interface **ICacheProvider** and be able to run against a variety of cache implementations simply by changing configuration values. The currently supported list of cache engine/implementations are:

* .Net memory cache
* ASP.Net Web cache
* [Windows Azure Appfabric](http://blogs.msdn.com/b/cie/archive/2014/03/05/caching-on-windows-azure-microsoft-appfabric-cache-azure-cache-service-managed-cache-dedicated-cache-in-role-cache-co-located-cache-shared-cache-azure-role-based-cache-clarifying-the-naming-confusion.aspx) (also knows as shared cache). Note: Microsoft will be retiring this technology.
* [memcached](http://memcached.org/)
* [Redis](http://redis.io/)

Note that this is only a brief introduction to the CacheAdatper, however for more detailed information, see the related blog posts listed below.

### How do I get set up? ###

* Nuget

Easiest way is to install the full [nuget package](http://www.nuget.org/packages/Glav.CacheAdapter/) into a simple console app, delete the '*program.cs*' file that gets added by default, then run the app. You can see the example code at work.

* Download/Clone and play

You can download or clone this repository, then have a look at the example code provided.

* A bit more details on getting setup
Modify your app.config/web.config to contain what cache to use. 
~~~~
 <appSetttings>
  <add key="Cache.CacheToUse" value="memory" />
  <add key="Cache.IsCacheEnabled" value="true" />
  <add key="Cache.IsCacheDependencyManagementEnabled" value="true" />
  <add key="Cache.CacheSpecificData" value="" />
  <add key="Cache.LoggingLevel" value="Information"/>
 </appSetttings>
~~~~

This configuration will use the memory cache for all operations, enabled dependency management (parent/child key relationships - see below), and logs all information and errors.

Then, in code you can do:
~~~~
  var data1 = AppServices.Cache.Get<string>("cache-key", DateTime.Now.AddSeconds(5), () =>
  {
    return "Some data from a data store";
  });
~~~~

which will return the data from cache if it exists in the cache, or use the lambda function to retrieve the data, add it to the cache , then return it for you. The data will remain in the cache for 5 seconds.

### Blog Posts ###
* [Version 2.0](https://weblogs.asp.net/pglavich/cacheadapter-v2-now-with-memcached-support) has details on memcached support.
* [Version 3.0](http://weblogs.asp.net/pglavich/cacheadapter-3-0-released) has details on Cache dependency management features.
* [Version 3.2](http://weblogs.asp.net/pglavich/cacheadapter-3-2-released) has details on package structure changes.
* [Version 4.0](http://weblogs.asp.net/pglavich/cacheadapter-4-0-just-released-with-redis-support) has details on redis support.
* [Version 4.1](http://weblogs.asp.net/pglavich/cache-adapter-4-1-released) has async methods, configurable logging and targets the .Net framework 4.5.
* [Version 4.2/4.2.1](https://weblogs.asp.net/pglavich/cacheadapter-4-2-0-released) has a licence file, config bug fix and fluent configuration.

### Who do I talk to? ###

* Paul Glavich (Owner). I can be contacted via twitter ( @glav ), or through the issue register within this repository.

### Revision History ###

#### Version 4.2.2 (CacheAdapter)/4.2.1 (CacheAdaper.Core) ####
* API Cleanup - remove need for 'CacheConfig' and 'ILogging' arguments from BuildCacheProvider extension methods.


#### Version 4.2.0/4.2.1 ####
* Note: 4.2.1 was a simple package bug fix. No code changes.
* Added a licence file  - [Issue 49](https://bitbucket.org/glav/cacheadapter/issues/49/license)
* Overhauled internals to provide better structured factory creation of cache engines and dependencies.
* As a result of the overhaul above, provide a fluent configuration ability to configure the cache from code much easier [Issue 50](https://bitbucket.org/glav/cacheadapter/issues/50/configuration-system-overhaul). This also fixes a multiple config instance issue [Issue 40](https://bitbucket.org/glav/cacheadapter/issues/40/cacheadapter-always-creates-new-config)
~~~~
using Glav.CacheAdapter.Helpers;

var provider = CacheConfig.Create()
                .UseMemcachedCache()
                .UsingDistributedServerNode("127.0.0.1")
                .BuildCacheProviderWithTraceLogging();
~~~~

* **Breaking change**: There was previously 2 ways of expressing configuration in the configuration file. In addition, due to a configuration system overhaul, this has now been changed to support only one. Specifically, the configuration section option has been removed and now only the <appSettings> method of configuration is supported. So if you had something like:
~~~~
<applicationSettings>
        <Glav.CacheAdapter.MainConfig>
            <setting name="CacheToUse" serializeAs="String">
                <value>memcached</value>
            </setting>
            <setting name="DistributedCacheServers" serializeAs="String">
                <value>localhost:11211</value>
            </setting>
        </Glav.CacheAdapter.MainConfig>
</applicationSettings>
~~~~

That should now be changed to:
~~~~
<appSettings>
  <add key="Cache.CacheToUse" value="memcached" />
  <add key="Cache.DistributedCacheServers" value="localhost:11211" />
</appSettings>
~~~~


#### Version 4.1.1 ####
* Version 4.1.1 in Nuget is simply a revision of the packaging. Glav.CacheAdapter.Core package 4.1.1 only contains the assembly. Glav.CacheAdaper 4.1.1 simply references the 4.1.1 core package. There has been no assembly change. In both packages, the assembly version is 4.1.0.

#### Version 4.1.0 ####
* Allow control of logging detail via `<add key="Cache.LoggingLevel" value="Information|ErrorsOnly|None"/>` - [Issue 43](https://bitbucket.org/glav/cacheadapter/issues/43/enhancing-the-logging)
* Addition of Async methods on the cache provider interface. [Issue 27](https://bitbucket.org/glav/cacheadapter/issues/27/)
* Updated nuspec target framework to expect .Net 4.5 as a minimum to reduce dependencies. [Issue 42](https://bitbucket.org/glav/cacheadapter/issues/42/change-dependency-on-newer-net-framework)


#### Version 4.0.1 ####

* Fixed minor typo is naming of RedisCacheAdapter (was misspelled RedisCacheAdatper) - [Issue 34](https://bitbucket.org/glav/cacheadapter/issue/34/typo-in-class-name-rediscacheadatper)

#### Notes on Version 4.0 ####

* Added support for cache type of redis
`<add key="Cache.CacheToUse" value="redis"/>`
* Also adds support for a redis specific dependency manager which is more efficient than the default for redis
`<add key="Cache.DependencyManagerToUse" value="redis"/>`
*Note: using `<add key="Cache.DependencyManagerToUse" value="default"/>` will default to using the redis specific cache dependency manager if the redis cache engine is selected. You can override this to use the generic dependency managment engine by using: `<add key="Cache.DependencyManagerToUse" value="generic"/>`
* Fix for minor performance issue when checking the dependency management ([Issue 33](https://bitbucket.org/glav/cacheadapter/issue/33/call-to))
* Addition of an extra method on the **ICache/ICacheProvider** interface - `InvalidateCacheItems` - to allow efficient batch deletions/removals of cache items.
* Much more efficient DependencyManager (both generic and redis specific) to remove large lists of dependencies quicker.
* Fixed a bug where a new config was not properly applied, if applied after initial initialisation.


#### Version 3.2.1 ####

* Fix for Issue 29. Using `SecurityMode=None` incorrectly depended on SecurityMessageAuthorizationKey setting. This is now resolved.


#### Version 3.1 and also 3.2 (combined) ####

* Support for SecurityMode.None for AppAfabric caching (Issue 20)
* Support for LocalCaching configuration values (Issue 21)
* For local caching support, you can specify the following in the cache specific data:
`<add key="Cache.CacheSpecificData" value="LocalCache.IsEnabled=true;LocalCache.DefaultTimeout=300;LocalCache.ObjectCount;LocalCache.InvalidationPolicy={TimeoutBased|NotificationBased}"/>` *Note: DefaultTimeout value specifies amount of time in seconds.*
* Support for programmatically setting the configuration and initialising the cache. (Issue 19)
* Splitting Glav.CacheAdapter package into 2 packages - **Glav.CacheAdapter.Core** & **Glav.CacheAdapter**.  (Issue 13)
* The "Core" package contains just the Glav.CacheAdapter assembly and references to dependencies so it is much easier to update the package and NOT include the readme and example code all the time.
* Merged in changes from Darren Boon's cache optimisation to ensure data only added to cache when its enabled and non null. Involved code cleanup as this branch had been partially merged prior.
* Merged change from [https://bitbucket.org/c0dem0nkee/cacheadapter/branch/default](https://bitbucket.org/c0dem0nkee/cacheadapter/branch/default) to destroy cache when using local cache on calling `ClearAll` method.

#### Version 3.0.3 ####

* Minor bug fix to memcached dependency management. Would not store dependencies when trying to store master cache dependency list for longer than 25 years.

#### Version 3.0.2 ####

* Fixes to readme.txt and instructions


#### Version 3.0.1 ####

* Minor bug fix when using appfabric and NOT including the CacheSpecificData section. Would throw an error as default of this (if not present) contains an invalid example security key. Supply some CacheSpecificData would resolve this but this update fixes that.

#### Notes on Version 3.0 ####

* New Feature: Addition of new Cache Dependency Feature to provide initial support to associate cache items to other cache items so that when one is invalidated, all related items are automatically invalidated.
* Modification to configuration system to support storing configuration overrides for all settings in the `<appSettings>` element in config.
* New API Feature: Support for clearing the cache programmatically. You can now call the `ClearAll` API method to clear the entire contents of the cache programmatically.
* Support of `ChannelOpenTimeout` and `MaxConnectionsToServer` configuration value for Windows Azure and Appfabric caching (in seconds).  The `ChannelOpenTimeout` value allows easier debugging when having connection issues as sometimes the client can forcibly disconnect  early and not get an valid exception. Setting this value to much higher allows the client to wait longer for a valid error from the server. An example which sets the ChannelOpenTimeout to 2 minutes(120 seconds) is:
`<add key="Cache.CacheSpecificData" value="UseSsl=false;ChannelOpenTimeout=120;SecurityMode=Message;MessageSecurityAuthorizationInfo={your_security_token}"/>`
* `MaxConnectionsToServer` allows fine tuning performance for the number of concurrent connections to the cache service. Currently, the   Azure client defaults to 1.
* Supports an `ICacheFeatureSupport` interface and base implementation. This is provided as a property on the `ICacheProvider` allowing basic feature detection. Currently this only supports identifying if a cache can be cleared but this will be expanded in the future.eg. `cacheProvider.FeatureSupport.SupportsClearingCacheContents()`
* Modifying configuration to support storing values in AppSettings section using "Cache." as keyprefix. This means you can use the same named config settings in `<appSettings>` section(or in a separate  appSettings file) as long as you prefix the appSetting with 'Cache.' For example, normally the main config section has:
~~~~
<Glav.CacheAdapter.MainConfig>
      <setting name="CacheToUse" serializeAs="String">
        <value>memcached</value>
      </setting>
</Glav.CacheAdapter.MainConfig>
~~~~
  in the appSettings, you could override this by having:
~~~~
<appSettings>
      <add key="Cache.CacheToUse" value="memory"/>
</appSettings>
~~~~
* In other words, you no longer need a `<Glav.CacheAdapter.MainConfig>` section. You can use the `<appSettings>` section only if you choose.In fact, the `<appSettings>` approach is  the preferred method.


######CacheDependencyManagement Details######

* Feature Addition: Rudimentary support of CacheDependencies. Note: Enabling this feature when using the default dependency support, incurs some performance hit due to more calls being made to the caching engine. This can result in a more "chatty" interface to the cache engine,and higher cache usage, therefore more memory and connections to the cache.
* This feature (and all advanced features) are only available when using the `ICacheProvider` interface implementation. This is by design. The `ICache` abstraction is a raw abstraction over the basic cache engine.
* Includes a generic cache dependency mechanism which acts as a common base. Not the most efficient but intent is to later introduce cache dependency managers which utilise specific features of the cache engine to maximise performance.
* The cache dependency implementation works as a parent/child mechanism. You can register or associate one or more child cache keys to a parent item. The cache key can actually be the key of an item in the cache but it doesn't have to be. So the parent key can be an arbitrary name or the key of an item in the cache.If it is an item in the cache, it will get invalidated when instructed as normal. Additionally, a child key of a parent key, can itself act as the parent for other cache keys. When the top level parent is invalidated, all its dependent children, and any further nested dependent children will also be invalidated. For example:
~~~~
    // Gets some data from main store and implicitly adds it to cache with key 'ParentKey'
    cacheProvider.Get<string>("ParentKey",DateTime.Now.AddDays(1),() => "Data");
	// Gets some data from main store and implicitly adds it to cache with key 'FirstChildKey' 
	// and as a dependency to parent item with key "ParentKey"
    cacheProvider.Get<string>("FirstChildKey",DateTime.Now.AddDays(1),() => "Data","ParentKey");
	// Gets some data from main store and implicitly adds it to cache with key 'ChildOfFirstChildKey' 
	// and as a dependency to parent item with key "FirstChildKey" which itself is a dependency to item with key "ParentKey"
    cacheProvider.Get<string>("ChildOfFirstChildKey",DateTime.Now.AddDays(1),() => "FirstChildKey");

	// At this point, the cache item relationship looks like
	// ParentKey
	//    +-----> FirstChildKey
	//                   +-------> ChildOfFirstChildKey

	// Invalidate the top level Parent, which clears all depenedent keys, included nested items
	cacheProvider.InvalidateCacheItem("ParentKey");
~~~~
* Note: A Parent can have a child key(s) that are themselves parents of the top level key causing recursion. This is fully supported by the code and no infinite loops are created. All relevant cache keys are cleared/actioned as normal within the collective set of dependent keys.


#### Version 2.5.3 ####

* Fixed bug where setting minpool size and max pool size resulted in an error when minpoolsize > default max pool size of 20. 
* Rewrite of adding per request cache dependency. Always uses web cache for this purpose if available, otherwise does nothing.


#### Notes on Version 2.5 ####

* This version takes a dependency upon enyim memcached. The reason is simply performance. I was doing a lot of performance work only to realise I was duplicating work already tried and tested in Enyim memcached caching component so have taken a dependency on that. This release is again only has changes related to memcached. The performance of enyim memcached is fantastic so you you should see some really good gains.
* If you need more information, please look at the following blog posts:
[Caching Architecture–Testability, Dependency Injection and Multiple Providers](http://weblogs.asp.net/pglavich/archive/2010/10/13/caching-architecture-testability-dependency-injection-and-multiple-providers.aspx)
[CacheAdapter now a Nuget package](http://weblogs.asp.net/pglavich/archive/2011/05/31/cacheadapter-now-a-nuget-package.aspx)