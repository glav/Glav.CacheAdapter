using Glav.CacheAdapter.Core;
using Glav.CacheAdapter.Core.Diagnostics;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Glav.CacheAdapter.Helpers;

namespace Glav.CacheAdapter.DependencyManagement
{
    public class RedisDependencyManager : BaseCacheDependencyManager
    {
        private IDatabase _redisDatabase;

        public RedisDependencyManager(ICache cache, ILogging logger, IDatabase redisDatabase, CacheConfig config = null) : base(cache, logger, config)
        {
            _redisDatabase = redisDatabase;
        }

        public override void RegisterParentDependencyDefinition(string parentKey, CacheDependencyAction actionToPerform = CacheDependencyAction.ClearDependentItems)
        {
            Logger.WriteInfoMessage(string.Format("Registering parent item:[{0}]", parentKey));

            var item = new DependencyItem { CacheKey = parentKey, Action = actionToPerform, IsParentNode = true };
            var depList = new DependencyItem[] { item };

            var cacheValueItems = new List<RedisValue>();

            var parentKeyExists = _redisDatabase.KeyExists(parentKey);
            if (parentKeyExists)
            {
                var currentValueType = _redisDatabase.KeyType(parentKey);
                if (currentValueType == RedisType.String)
                {
                    // it is NOT currently a List, which means it has a simple value and is not associated as a parent key at this time
                    // so we need to convert it
                    var currentKeyValue = _redisDatabase.StringGet(parentKey);
                    Cache.InvalidateCacheItem(parentKey);
                    cacheValueItems.Add(currentKeyValue);
                }
            }
            else
            {
                // Nothing exists thus far so we create the parent key as a list with the 1st item in the list
                // as empty as the 1st item in a list for a parent key is always reserved for the cache key value of the parent key
                // The dependent keys are in the list after that
                cacheValueItems.Add(string.Empty);
            }
            cacheValueItems.Add(depList.Serialize());
            _redisDatabase.ListRightPush(parentKey, cacheValueItems.ToArray());
        }

        public override void RemoveParentDependencyDefinition(string parentKey)
        {
            Cache.InvalidateCacheItem(parentKey);
        }

        public override void AssociateDependentKeysToParent(string parentKey, IEnumerable<string> dependentCacheKeys, CacheDependencyAction actionToPerform = CacheDependencyAction.ClearDependentItems)
        {
            Logger.WriteInfoMessage(string.Format("Associating list of cache keys to parent key:[{0}]", parentKey));

            if (dependentCacheKeys == null)
            {
                return;
            }

            var depList = new List<DependencyItem>();
            foreach (var dependentKey in dependentCacheKeys)
            {
                var item = new DependencyItem { CacheKey = dependentKey, Action = actionToPerform, IsParentNode = false };
                depList.Add(item);
            }

            RegisterParentDependencyDefinition(parentKey, actionToPerform);

            depList.ForEach(d =>
            {
                _redisDatabase.ListRightPush(parentKey, new RedisValue[1]  {d.Serialize()});
            });
        }

        public override IEnumerable<DependencyItem> GetDependentCacheKeysForParent(string parentKey, bool includeParentNode = false)
        {
            var itemList = new List<DependencyItem>();
            var keyList = _redisDatabase.ListRange(parentKey);
            if (keyList != null && keyList.Length > 0)
            {
                for (var keyCount=0; keyCount < keyList.Length;keyCount++)
                {
                    // 1st item in list is always reserved for the parent key value if there is one.
                    if (keyCount > 0)
                    {
                        var keyItem = keyList[keyCount];
                        try
                        {
                            var depItem = ((byte[])keyItem).Deserialize<DependencyItem>();
                            itemList.Add(depItem);
                        } catch (Exception ex)
                        {
                            Logger.WriteErrorMessage(string.Format("Unable to deserialise a DependencyItem #{0} for ParentKey: [{1}]",keyCount,parentKey));
                            Logger.WriteException(ex);
                        }
                    }
                }
            }

            if (!includeParentNode && itemList.Count > 0)
            {
                var parentNode = itemList.FirstOrDefault(n => n.IsParentNode == true);
                if (parentNode != null)
                {
                    itemList.Remove(parentNode);
                }
            }

            return itemList;
        }

        public override string Name
        {
            get { return "Redis Specific"; }
        }

 
    }
}
