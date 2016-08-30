using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Glav.CacheAdapter.Helpers
{
    public static class CacheKeyNamingExtensions
    {
        public static string GetCacheKey<T>(this Func<T> getData) where T : class
        {
            if (getData.Method.DeclaringType != null)
                return getData.Method.DeclaringType.FullName + "-" + getData.Method.Name;
            throw new ArgumentNullException("getData");
        }

        public static string GetCacheKey<T>(this Func<Task<T>> getData) where T : class
        {
            if (getData.Method.DeclaringType != null)
                return getData.Method.DeclaringType.FullName + "-" + getData.Method.Name;
            throw new ArgumentNullException("getData");
        }

    }
}
