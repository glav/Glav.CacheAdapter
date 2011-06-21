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
configured. You can use either ICache or ICacheProvider, and it will use the underlying configured cache mechanism. ICacheProvider
is simply provided to give a more fluent API to cache usage.

In the config file, if you set the 'CacheToUse' setting to either 'AppFabric' or 'memcached', then the 'DistributedCacheServers'
should be a comma separated list of server IP addresses and port numbers that represent the cache servers in your cache farm. 
For example:
    <setting name="DistributedCacheServers" serializeAs="String">
      <value>localhost:11211,localhost:11212</value>
    </setting>
This configuration states that there are 2 cache servers in the farm. One at address localhost (127.0.0.1), port 11211 and the
other at address localhost (127.0.0.1), port 11212.


If you need more information, please look at the following blog posts:
http://weblogs.asp.net/pglavich/archive/2010/10/13/caching-architecture-testability-dependency-injection-and-multiple-providers.aspx
http://weblogs.asp.net/pglavich/archive/2011/05/31/cacheadapter-now-a-nuget-package.aspx

