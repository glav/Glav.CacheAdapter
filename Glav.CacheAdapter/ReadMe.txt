CacheAdatper
~~~~~~~~~~~~

This project provides a simple, consistent API into a number of different cache mechanisms.
You can program your application agsinst the simple ICache or ICacheProvider interface, yet change the 
implementation underlying that interface via configuration to use either:
 1. In Memory cache (config setting="memory")
 2. ASP.NET web cache (config setting="web")
 3. Distributed AppFabric cache. (config setting="AppFabric")
 4. Distributed memcached cache. (config setting="memcached")

For example:
            <setting name="CacheToUse" serializeAs="String">
                <value>memcached</value>
            </setting>
Means the underlying cache mechanism uses memcached and it expects to find memcached server nodes at the address listed in
the 'DistributedCacheServers' configuration element (see below).

This means you dont have to know how to program against these specific cache mechanisms, as this is all handled
by the various adapters within this project, and driven purely through configuration.

In addition, the use of an interface based approach means you can easily test any component using 
this cache interface as a dependency by mocking this interface implementation using tools such as 
RhinoMocks of MoQ.

This library consists of 2 main interfaces:
ICache
ICacheProvider

ICache is what is implemented by each each cache mechanism such as memory, web, app fabric, memcached. It contains the raw Add/Get/Delete
methods for each cache implementation. ICacheProvider is a more fluent API that makes use of what ever ICache implementation is
configured. You can use either ICache or ICacheProvider directly, and it will use the underlying configured cache mechanism. ICacheProvider
is simply provided to give a more fluent API to cache usage.

In the config file, if you set the 'CacheToUse' setting to either 'AppFabric' or 'memcached', then the 'DistributedCacheServers'
should be a comma separated list of server IP addresses and port numbers that represent the cache servers in your cache farm. 
For example:
    <setting name="DistributedCacheServers" serializeAs="String">
      <value>localhost:11211;localhost:11212</value>
    </setting>
This configuration states that there are 2 cache servers in the farm. One at address localhost (127.0.0.1), port 11211 and the
other at address localhost (127.0.0.1), port 11212.

Please note that each distributed cache mechanism has different default ports that they use if
none are specified. The following is the default ports for each implementation:
* Windows AppFabric Caching = Port 22223
* memcached = Port 11211

Note: IN previous versions of this library, you could separate the DistributedCacheServers by using a ',' character.
While this is still supported, the preferred separator is to use the ';'character to make it consistent with the
CacheSpecificData setting (discussed below)

Currently, only Windows AppFabric within Azure requires specific security settings to work. To enable this in a generic
manner that may be used for other cache mechanisms, the CacheSpecificData config element has been introduced. It is
separated set of key/value pairs used to provide specific configuration to a cache mechanism when it may only apply
to one cache mechanism and not make sense for others.
For example, Windows Azure AppFabric(ie.AppFabric only when used within Azure) requires security type and a security
key to work properly. This can be set like so:
    
	<setting name="CacheSpecificData" serializeAs="String">
      <value>UseSsl=false;SecurityMode=Message;MessageSecurityAuthorizationInfo=your_secure_key_from_azure_dashboard</value>
    </setting>

This data is telling Windows AppFabric client to not use SSL, SecurityMode = Message and the 
authorizationInfo = 'your_secure_key_from_azure_dashboard'  (this key is supplied from the Azure dashboard)

This data aims to replicate the Azure specific config shown below, without having all the extra elements in the 
config:
<dataCacheClients>
    <dataCacheClient name="default">
      <hosts>
        <host name="YOUR-CACHE-HOST.cache.windows.net" cachePort="22233" />
      </hosts>
      <securityProperties mode="Message">
        <messageSecurity authorizationInfo="YWNzOmh0dHBzOi8vYnVyZWxhdGVzd=="></messageSecurity>
      </securityProperties>
    </dataCacheClient>
  </dataCacheClients>

Note: This configuration data is specific to Windows Azure AppFabric only. Using AppFabric outside of Azure, this
config is not required. Also, other cache mechanisms such as memcached do not require this information.
However future versions of this library may support specific config data for memcached in this element and indeed other
distributed mechanisms may also support this.

Also, since the 'DistributedCacheName' configuration element is only AppFabric specific, this can also be specified
in the CacheSpecificData setting instead of its own element. The library looks for this data (if AppFabric is used)
in the CacheSpecificData element first before checking the single config element.
So you could have something like:
	<setting name="CacheSpecificData" serializeAs="String">
      <value>DistributedCacheName=MyCache;UseSsl=false;SecurityMode=Message;MessageSecurityAuthorizationInfo=your_secure_key_from_azure_dashboard</value>
    </setting>

Also note that a blank entry for DistributedCacheName config setting will result in the default cache being used/accessed in AppFabric.

Disabling the cache globally
~~~~~~~~~~~~~~~~~~~~~~~~~~~~
You can completely disable the use of any cache so that all GET attempts will result in a cache miss 
and execute the delegate if one is provided. You can do this by setting the configuration
setting "IsCacheEnabled" to false.
            <setting name="IsCacheEnabled" serializeAs="String">
                <value>True</value>
            </setting>
Note: This feature only works if you are using the CacheProvider method of access. If you access the 
InnerCache or ICache directly, you will still be able to access the cache itself and cache operations will work as normal.

Notes on Version 2.5
~~~~~~~~~~~~~~~~~~~~
This version takes a dependency upon enyim memcached. The reason is simply performance. I was doing a lot of performance work only to  realise I was duplicating work already tried and tested in Enyim memcached caching component so have taken a dependency on that. This release is again only has changes related to memcached. The performance of enyim memcached is fantastic so you you should see some really good gains.

If you need more information, please look at the following blog posts:
http://weblogs.asp.net/pglavich/archive/2010/10/13/caching-architecture-testability-dependency-injection-and-multiple-providers.aspx
http://weblogs.asp.net/pglavich/archive/2011/05/31/cacheadapter-now-a-nuget-package.aspx


Notes on Version 2.5.3
~~~~~~~~~~~~~~~~~~~~~~
* Fixed bug where setting minpool size and max pool size resulted in an error when minpoolsize > default max pool size of 20. 
* Rewrite of adding per request cache dependency. Always uses web cache for this purpose if available, otherwise does nothing.

Notes on Version 3.0
~~~~~~~~~~~~~~~~~~~~
* Feature Addition: Rudimentary support of CacheDependencies
  Note: * Enabling this feature when using the default dependency support, incurs some performance 
        hit due to more calls being made to the caching engine. This can result in a more "chatty"
		interface to the cache engine,and higher cache usage, therefore more memory and connections 
		to the cache.
		* This feature (and all advanced features) are only available when using the CacheProvider
		  interface implementation. This is by design. The ICache abstraction is a raw abstraction over
		  the basic cache engine.
  Includes a generic cache dependency mechanism which acts as a common base. Not the most efficient but intent is to
  later introduce cache dependency managers which utilise specific features of the cache engine to maximise performance.

  2 Forms of cache dependencies are initially supported.
  1. Associated Cache dependency items
  2. Cache groups

* Modifying configuration to support storing values in AppSettings section using "Cache." as keyprefix
  This means you can use the same named config settings in <appSettings> section(or in a separate
  appSettings file) as long as you prefix the appSetting with 'Cache.'
  For example, normally the main config section has:
    <Glav.CacheAdapter.MainConfig>
      <setting name="CacheToUse" serializeAs="String">
        <value>memcached</value>
      </setting>
	</Glav.CacheAdapter.MainConfig>
  in the appSettings, you could override this by having:
    <appSettings>
      <add key="Cache.CacheToUse" value="memory"/>
	</appSettings>
  In other words, you no longer need a <Glav.CacheAdapter.MainConfig> section. You can
  use the <appSettings> section only if you choose.

