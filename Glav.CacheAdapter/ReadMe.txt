CacheAdatper
~~~~~~~~~~~~

This project provides a simple, consistent API into a number of different cache mechanisms.
You can program your application agsinst the simple ICache or ICacheProvider interface, yet change the 
implementation underlying that interface via configuration to use either:
 1. In Memory cache (config setting="memory")
 2. ASP.NET web cache (config setting="web")
 3. Distributed AppFabric cache. (config setting="AppFabric")
 4. Distributed memcached cache. (config setting="memcached")

This means you dont have to know how to program against these specific cache mechanisms, as this is all handled
by the various adapters within this project, and driven purely through configuration.

In addition, the use of an interface based approach means you can easily test any component using 
this cache interface as a dependency by mocking this interface implementation using tools such as 
RhinoMocks of MoQ.

If you need more information, please look at the following blog posts:
http://weblogs.asp.net/pglavich/archive/2010/10/13/caching-architecture-testability-dependency-injection-and-multiple-providers.aspx
http://weblogs.asp.net/pglavich/archive/2011/05/31/cacheadapter-now-a-nuget-package.aspx

