namespace Glav.CacheAdapter.DependencyManagement
{
    public class DependencyItem
    {
        public bool IsParentNode { get; set; }
        public string CacheKey { get; set; }
        public CacheDependencyAction Action { get; set; }
    }
}
