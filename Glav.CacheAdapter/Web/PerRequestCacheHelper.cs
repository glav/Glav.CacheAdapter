namespace Glav.CacheAdapter.Web
{
    internal class PerRequestCacheHelper
    {
        public void AddToPerRequestCache(string cacheKey, object dataToAdd)
        {
            // If not in a web context, do nothing
            if (InWebContext())
            {
                if (System.Web.HttpContext.Current.Items.Contains(cacheKey))
                {
                    System.Web.HttpContext.Current.Items.Remove(cacheKey);
                }
                System.Web.HttpContext.Current.Items.Add(cacheKey, dataToAdd);
            }
        }

        public T TryGetItemFromPerRequestCache<T>(string cacheKey) where T : class
        {
            // try per request cache first, but only if in a web context
            if (InWebContext())
            {
                if (System.Web.HttpContext.Current.Items.Contains(cacheKey))
                {
                    var data = System.Web.HttpContext.Current.Items[cacheKey];
                    var realData = data as T;
                    if (realData != null)
                    {
                        return realData;
                    }
                }
            }

            return null;
        }

        private static bool InWebContext()
        {
            return System.Web.HttpContext.Current != null;
        }
    }
}
