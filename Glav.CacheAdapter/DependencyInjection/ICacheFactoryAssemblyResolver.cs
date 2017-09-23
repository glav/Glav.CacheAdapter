namespace Glav.CacheAdapter.Core.DependencyInjection
{
    public interface ICacheFactoryAssemblyResolver
    {
        ICacheConstructionFactory ResolveCacheFactory(CacheConfig config);
    }
}