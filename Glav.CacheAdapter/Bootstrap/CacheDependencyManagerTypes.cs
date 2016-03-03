namespace Glav.CacheAdapter.Bootstrap
{
    internal static class CacheDependencyManagerTypes
    {
        /// <summary>
        /// The default dependency manager for a given cache type
        /// </summary>
        public const string Default = "default";
        /// <summary>
        /// Use the generic dependency manager explicitly
        /// </summary>
        public const string Generic = "generic";
        /// <summary>
        /// Use the redis dependency manager explicitly
        /// </summary>
        public const string Redis = "redis";
        /// <summary>
        /// Nothing specified
        /// </summary>
        public const string Unspecified = "";
    }
}
