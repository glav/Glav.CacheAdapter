using Glav.CacheAdapter.DependencyManagement;
using Glav.CacheAdapter.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Glav.CacheAdapter.Core
{
    public class CacheFactoryComponentResult
    {
        private CacheFactoryComponentResult()
        {
        }
        public ICache Cache { get; private set; }
        public ICacheDependencyManager DependencyManager { get; private set; }
        public ICacheFeatureSupport FeatureSupport { get; private set; }

        public CacheConfig ConfigUsed { get; private set; }

        public static CacheFactoryComponentResult Create(ICache cache, ICacheDependencyManager dependencyManager, ICacheFeatureSupport featureSupport, CacheConfig configUsed )
        {
            var result = new CacheFactoryComponentResult();
            if (cache == null || featureSupport == null)
            {
                throw new ArgumentNullException("Cache or Feature support component cannot be null");
            }
            result.Cache = cache;
            result.DependencyManager = dependencyManager;
            result.FeatureSupport = featureSupport;
            result.ConfigUsed = configUsed;
            return result;
        }
    }
}
