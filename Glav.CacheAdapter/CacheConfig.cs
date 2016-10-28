using System.Collections.Generic;
using System.Configuration;
using Glav.CacheAdapter.Distributed;
using Glav.CacheAdapter.Helpers;
using Glav.CacheAdapter.Diagnostics;
using Glav.CacheAdapter.Core;
using Glav.CacheAdapter.Bootstrap;

namespace Glav.CacheAdapter
{
    public class CacheConfig
    {
        public const string AppSettingsKeyPrefix = "Cache.";

        public CacheConfig()
        {
            IsCacheEnabled = true;
            RetrieveAppSettingsIfPresent();
        }

        public static CacheConfig Create()
        {
            return new CacheConfig();
        }
        public string CacheToUse { get;  set;  }

        public string DependencyManagerToUse { get; set; }

        private readonly List<ServerNode> _serverNodes = new List<ServerNode>();
        public List<ServerNode> ServerNodes { get { return _serverNodes; } }

        private readonly Dictionary<string, string> _providerSpecificValues = new Dictionary<string, string>();
        public Dictionary<string, string> ProviderSpecificValues { get { return _providerSpecificValues; } }

        public bool IsCacheEnabled { get; set; }

        public string DistributedCacheServers { get; set; }

        /// <summary>
        /// Enables support of cache dependencies using parent and child/associated cache keys
        /// </summary>
        /// <remarks>This can require extra calls to the cache engine and so can incur a
        /// performance degradation due to extra network cache calls</remarks>
        public bool IsCacheDependencyManagementEnabled { get; set; }

        public string CacheSpecificData { get; set; }

        public LoggingLevel LoggingLevel { get; set; }

        /// <summary>
        /// Overrides the current settings taken from the config file, with any present in the
        /// AppSettings section. This allows applications to NOT use the main config and rely on
        /// app settings section instead
        /// </summary>
        private void RetrieveAppSettingsIfPresent()
        {
            var cacheEnabledKey = string.Format("{0}IsCacheEnabled", AppSettingsKeyPrefix);
            var isCacheDependencyManagementKey = string.Format("{0}IsCacheDependencyManagementEnabled", AppSettingsKeyPrefix);
            var cacheSpecificDataKey = string.Format("{0}CacheSpecificData", AppSettingsKeyPrefix);
            var cacheToUseKey = string.Format("{0}CacheToUse", AppSettingsKeyPrefix);
            var dependencyMgrToUseKey = string.Format("{0}DependencyManagerToUse", AppSettingsKeyPrefix);
            var distributedCacheServersKey = string.Format("{0}DistributedCacheServers", AppSettingsKeyPrefix);
            var logLevelKey = string.Format("{0}LoggingLevel", AppSettingsKeyPrefix);

            if (ConfigurationManager.AppSettings[cacheToUseKey].HasValue())
            {
                CacheToUse = ConfigurationManager.AppSettings[cacheToUseKey].ToLowerInvariant();
            } else
            {
                CacheToUse = CacheTypes.MemoryCache;
            }
            if (ConfigurationManager.AppSettings[cacheEnabledKey].HasValue())
            {
                IsCacheEnabled = ConfigurationManager.AppSettings[cacheEnabledKey].ToBoolean();
            }
            if (ConfigurationManager.AppSettings[isCacheDependencyManagementKey].HasValue())
            {
                IsCacheDependencyManagementEnabled = ConfigurationManager.AppSettings[isCacheDependencyManagementKey].ToBoolean();
            }
            if (ConfigurationManager.AppSettings[cacheSpecificDataKey].HasValue())
            {
                CacheSpecificData = ConfigurationManager.AppSettings[cacheSpecificDataKey];
            }
            if (ConfigurationManager.AppSettings[dependencyMgrToUseKey].HasValue())
            {
                DependencyManagerToUse = ConfigurationManager.AppSettings[dependencyMgrToUseKey];
            }
            if (ConfigurationManager.AppSettings[distributedCacheServersKey].HasValue())
            {
                DistributedCacheServers = ConfigurationManager.AppSettings[distributedCacheServersKey];
            }

            if (ConfigurationManager.AppSettings[logLevelKey].HasValue())
            {
                LoggingLevel = ConfigurationManager.AppSettings[logLevelKey].ToLoggingLevel();

            } else
            {
                LoggingLevel = LoggingLevel.Information;
            }
        }

        public string GetConfigValueFromProviderSpecificValues(string key)
        {
            if (_providerSpecificValues.ContainsKey(key))
            {
                return _providerSpecificValues[key];
            }

            return null;
        }

    }
}
