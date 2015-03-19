# Glav.CacheAdapter - README #

This library allows developers to code against a single interface ICacheProvider and be able to run against a variety of cache implementations simply by changing configuration values. The currently supported list of cache engine/implementations are:
* .Net memory cache
* ASP.Net Web cache
* [Windows Azure Appfabric](http://blogs.msdn.com/b/cie/archive/2014/03/05/caching-on-windows-azure-microsoft-appfabric-cache-azure-cache-service-managed-cache-dedicated-cache-in-role-cache-co-located-cache-shared-cache-azure-role-based-cache-clarifying-the-naming-confusion.aspx) (also knows as shared cache). Note: Microsoft will be retiring this technology.
* [memcached](http://memcached.org/)
* [Redis](http://redis.io/)

A brief introduction to the purpose of the CacheAdatper, however for more detailed information, see the related blog posts listed below.

### What is this repository for? ###

* Quick summary
* Version
* [Learn Markdown](https://bitbucket.org/tutorials/markdowndemo)

### How do I get set up? ###

* Nuget

Easiest way is to install the full [nuget package](http://www.nuget.org/packages/Glav.CacheAdapter/) into a simple console app, delete the '*program.cs*' file that gets added by default, then run the app. You can see the example code at work.

* Download/Clone and play

You can download or clone this repository, then have a look at the example code provided.

### Blog Posts ###
* [Version 2.0](https://weblogs.asp.net/pglavich/cacheadapter-v2-now-with-memcached-support) has details on memcached support.
* [Version 3.0](http://weblogs.asp.net/pglavich/cacheadapter-3-0-released) has details on Cache dependency management features.
* [Version 3.2](http://weblogs.asp.net/pglavich/cacheadapter-3-2-released) has details on package structure changes.
* [Version 4.0](http://weblogs.asp.net/pglavich/cacheadapter-4-0-just-released-with-redis-support) has details on redis support.

### Who do I talk to? ###

* Paul Glavich (Owner). I can be contacted via twitter ( @glav ), or through the issue register within this repository.