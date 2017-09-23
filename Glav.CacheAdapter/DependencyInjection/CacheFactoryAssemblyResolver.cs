using Glav.CacheAdapter.Core.Diagnostics;
using System;

namespace Glav.CacheAdapter.Core.DependencyInjection
{
    public class CacheFactoryAssemblyResolver : ICacheFactoryAssemblyResolver
    {
        private readonly ILogging _logger;

        public CacheFactoryAssemblyResolver(ILogging logger)
        {
            _logger = logger;
        }
        public ICacheConstructionFactory ResolveCacheFactory(CacheConfig config)
        {
            _logger.WriteInfoMessage($"Determining assembly name for cache setting of: {config.CacheToUse.ToString()}");
            var cacheSetting = config.CacheToUse.ToCacheSetting();
            var assemblyName = cacheSetting.GetCacheFactoryAssemblyName();
            if (string.IsNullOrWhiteSpace(assemblyName))
            {
                _logger.WriteInfoMessage($"No assembly defined for cache setting of: {config.CacheToUse} so no assembly loading performed.");
                return null;
            }

            _logger.WriteInfoMessage($"Attempting to load CacheFactory from Assembly: {assemblyName}");
            try
            {
                var asm = System.Reflection.Assembly.LoadFrom(assemblyName);
                _logger.WriteInfoMessage($"Assembly loaded: {assemblyName}");
                var factoryType = asm.GetType(cacheSetting.GetCacheFactoryTypeName());
                _logger.WriteInfoMessage("ICacheConstructionFactory type located.");
                var factory = Activator.CreateInstance(factoryType, _logger, config) as ICacheConstructionFactory;
                _logger.WriteInfoMessage("ICacheConstructionFactory type instantiated.");

                return factory;
            }
            catch (Exception ex)
            {
                _logger.WriteException(ex);
                throw;
            }
        }
    }
}
