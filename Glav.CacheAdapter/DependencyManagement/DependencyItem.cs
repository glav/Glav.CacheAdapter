using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Glav.CacheAdapter.DependencyManagement
{
    public class DependencyItem
    {
        public string CacheKeyOrCacheGroup { get; set; }
        public CacheDependencyAction Action { get; set; }
    }
}
