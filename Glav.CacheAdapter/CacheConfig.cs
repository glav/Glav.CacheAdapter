using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using Glav.CacheAdapter.Distributed;
using Glav.CacheAdapter.Helpers;

namespace Glav.CacheAdapter
{
	public class CacheConfig
	{
        public const string AppSettingsKeyPrefix = "Cache.";

	    private string _cacheToUse = null;
        public string CacheToUse { get { return _cacheToUse; } set { _cacheToUse = value; } }

        private string _dependencyManagerToUse = null;
        public string DependencyManagerToUse { get { return _dependencyManagerToUse; } set { _dependencyManagerToUse = value; } }
        
        private List<ServerNode> _serverNodes = new List<ServerNode>();
		public List<ServerNode> ServerNodes { get { return _serverNodes; } }

		private Dictionary<string, string> _providerSpecificValues = new Dictionary<string, string>();
		public Dictionary<string,string> ProviderSpecificValues { get { return _providerSpecificValues; } }

		private bool _isCacheEnabled = true;
		public bool IsCacheEnabled { get { return _isCacheEnabled; } set { _isCacheEnabled = value; } }

        private string _distributedCacheServers = null;
	    public string DistributedCacheServers { get { return _distributedCacheServers; } set { _distributedCacheServers = value; }}

	    bool _isCacheKeysDependenciesEnabled = false;
        /// <summary>
        /// Enables support of cache dependencies using master and child/associated cache keys
        /// </summary>
        /// <remarks>This can require extra calls to the cache engine and so can incur a
        /// performance degradation due to extra network cache calls</remarks>
        public bool IsCacheKeysDependenciesEnabled { get { return _isCacheKeysDependenciesEnabled; } set { _isCacheKeysDependenciesEnabled = value; } }
        
        private string _cacheSpecificData = null;
        public string CacheSpecificData { get { return _cacheSpecificData; } set { _cacheSpecificData=value; } }
        
        private bool _isCacheGroupDependenciesEnabled = false;
        /// <summary>
        /// Enables support of cache dependencies using cache key prefix
        /// </summary>
        /// <remarks>This can require extra calls to the cache engine and so can incur a
        /// performance degradation due to extra network cache calls</remarks>
        public bool IsCacheGroupDependenciesEnabled { get { return _isCacheGroupDependenciesEnabled; } set { _isCacheGroupDependenciesEnabled = value; } }
       
        public CacheConfig()
        {
            ApplySettingsFromDefaultConfig();
            OverrideSettingsWithAppSettingsIfPresent();
		}

        private void ApplySettingsFromDefaultConfig()
        {
            IsCacheEnabled = MainConfig.Default.IsCacheEnabled;
            IsCacheKeysDependenciesEnabled = MainConfig.Default.IsCacheKeyDependenciesEnabled;
            IsCacheGroupDependenciesEnabled = MainConfig.Default.IsCacheGroupDependenciesEnabled;
            CacheSpecificData = MainConfig.Default.CacheSpecificData;
            CacheToUse = !string.IsNullOrWhiteSpace(MainConfig.Default.CacheToUse) ? MainConfig.Default.CacheToUse.ToLowerInvariant() : string.Empty;
            DependencyManagerToUse = MainConfig.Default.DependencyManagerToUse;
            DistributedCacheServers = MainConfig.Default.DistributedCacheServers;
        }

        /// <summary>
        /// Overrides the current settings taken from the config file, with any present in the
        /// AppSettings section. This allows applications to NOT use the main config and rely on
        /// app settings section instead
        /// </summary>
        private void OverrideSettingsWithAppSettingsIfPresent()
        {
            var cacheEnabledKey = string.Format("{0}IsCacheEnabled", AppSettingsKeyPrefix);
            var isCacheKeysDependenciesKey = string.Format("{0}IsCacheKeysDependenciesEnabled",AppSettingsKeyPrefix);
            var isCachePrefixDependenciesKey = string.Format("{0}IsCacheGroupDependenciesEnabled", AppSettingsKeyPrefix);
            var cacheSpecificDataKey = string.Format("{0}CacheSpecificData", AppSettingsKeyPrefix);
            var cacheToUseKey = string.Format("{0}CacheToUse", AppSettingsKeyPrefix);
            var dependencyMgrToUseKey = string.Format("{0}DependencyManagerToUse", AppSettingsKeyPrefix);
            var distributedCacheServersKey = string.Format("{0}DistributedCacheServers", AppSettingsKeyPrefix);

            if (ConfigurationManager.AppSettings[cacheToUseKey].HasValue())
            {
                _cacheToUse = ConfigurationManager.AppSettings[cacheToUseKey].ToLowerInvariant();
            }
            if (ConfigurationManager.AppSettings[cacheEnabledKey].HasValue())
            {
                _isCacheEnabled = ConfigurationManager.AppSettings[cacheEnabledKey].ToBoolean();
            }
            if (ConfigurationManager.AppSettings[isCacheKeysDependenciesKey].HasValue())
            {
                _isCacheKeysDependenciesEnabled = ConfigurationManager.AppSettings[isCacheKeysDependenciesKey].ToBoolean();
            }
            if (ConfigurationManager.AppSettings[isCachePrefixDependenciesKey].HasValue())
            {
                _isCacheGroupDependenciesEnabled = ConfigurationManager.AppSettings[isCachePrefixDependenciesKey].ToBoolean();
            }
            if (ConfigurationManager.AppSettings[cacheSpecificDataKey].HasValue())
            {
                _cacheSpecificData = ConfigurationManager.AppSettings[cacheSpecificDataKey];
            }
            if (ConfigurationManager.AppSettings[dependencyMgrToUseKey].HasValue())
            {
                _dependencyManagerToUse = ConfigurationManager.AppSettings[dependencyMgrToUseKey];
            }
            if (ConfigurationManager.AppSettings[distributedCacheServersKey].HasValue())
            {
                _distributedCacheServers = ConfigurationManager.AppSettings[distributedCacheServersKey];
            }
        }
	}
}
