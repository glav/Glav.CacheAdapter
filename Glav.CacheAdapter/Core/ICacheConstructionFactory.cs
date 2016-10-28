using Glav.CacheAdapter.DependencyManagement;
using Glav.CacheAdapter.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Glav.CacheAdapter.Core
{
    public interface ICacheConstructionFactory
    {
        CacheFactoryComponentResult CreateCacheComponents();
    }
}
