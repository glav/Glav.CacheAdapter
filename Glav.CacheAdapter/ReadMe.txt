CacheAdatper
~~~~~~~~~~~~

This project provides a simple, consistent API into a number of different cache mechanisms. Using a simple interface, you can program your application agsinst this interface, yet change the implementation underlying that interface via config to use either in memory cache, ASP.NET web cache or distributed AppFabric cache.
In addition, the use of an interface based approach means you can easily test any component using this cache interface as a dependency by mocking this interface implementation using tools such as RhinoMocks of MoQ.

If you need more information, please look at the following blog posts:
http://weblogs.asp.net/pglavich/archive/2010/10/13/caching-architecture-testability-dependency-injection-and-multiple-providers.aspx
http://weblogs.asp.net/pglavich/archive/2011/05/31/cacheadapter-now-a-nuget-package.aspx

