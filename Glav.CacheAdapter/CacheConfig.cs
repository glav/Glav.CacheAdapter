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
        public const string AppSettingsKeyPrefix = "CacheAdapter.";

	    private string _cacheToUse = null;
        public string CacheToUse { get { return _cacheToUse; } set { _cacheToUse = value; } }

		private List<ServerNode> _serverNodes = new List<ServerNode>();
		public List<ServerNode> ServerNodes { get { return _serverNodes; } }

		private Dictionary<string, string> _providerSpecificValues = new Dictionary<string, string>();
		public Dictionary<string,string> ProviderSpecificValues { get { return _providerSpecificValues; } }

		private bool _isCacheEnabled = true;
		public bool IsCacheEnabled { get { return _isCacheEnabled; } set { _isCacheEnabled = value; } }

        
        bool _isCacheKeysDependenciesEnabled = false;
        /// <summary>
        /// Enables support of cache dependencies using master and child/associated cache keys
        /// </summary>
        /// <remarks>This can require extra calls to the cache engine and so can incur a
        /// performance degradation due to extra network cache calls</remarks>
        public bool IsCacheKeysDependeniesEnabled { get { return _isCacheKeysDependenciesEnabled; } set { _isCacheKeysDependenciesEnabled = value; } }
        
        private string _cacheSpecificData = null;
        public string CacheSpecificData { get { return _cacheSpecificData; } set { _cacheSpecificData=value; } }
        
        private bool _isCachePrefixDependenciesEnabled = false;
        /// <summary>
        /// Enables support of cache dependencies using cache key prefix
        /// </summary>
        /// <remarks>This can require extra calls to the cache engine and so can incur a
        /// performance degradation due to extra network cache calls</remarks>
        public bool IsCachePrefixDependenciesEnabled { get { return _isCachePrefixDependenciesEnabled; } set { _isCachePrefixDependenciesEnabled = value; } }
       
        public CacheConfig()
        {
            ApplySettingsFromDefaultConfig();
            OverrideSettingsWithAppSettingsIfPresent();
		}

        private void ApplySettingsFromDefaultConfig()
        {
            IsCacheEnabled = MainConfig.Default.IsCacheEnabled;
            IsCacheKeysDependeniesEnabled = MainConfig.Default.IsCacheKeyDependenciesEnabled;
            IsCachePrefixDependenciesEnabled = MainConfig.Default.IsCachePrefixDependenciesEnabled;
            CacheSpecificData = MainConfig.Default.CacheSpecificData;
            CacheToUse = !string.IsNullOrWhiteSpace(MainConfig.Default.CacheToUse) ? MainConfig.Default.CacheToUse.ToLowerInvariant() : string.Empty;
        }

        /// <summary>
        /// Overrides the current settings taken from the config file, with any present in the
        /// AppSettings section. This allows applications to NOT use the main config and rely on
        /// app settings section instead
        /// </summary>
        private void OverrideSettingsWithAppSettingsIfPresent()
        {
            var cacheEnabledKey = string.Format("{0}IsCacheEnabled", AppSettingsKeyPrefix);
            var isCacheKeysDependenciesKey = string.Format("{0}IsCacheKeysDependeniesEnabled",AppSettingsKeyPrefix);
            var isCachePrefixDependenciesKey = string.Format("{0}IsCachePrefixDependenciesEnabled", AppSettingsKeyPrefix);
            var cacheSpecificDataKey = string.Format("{0}CacheSpecificData", AppSettingsKeyPrefix);
            var cacheToUseKey = string.Format("{0}CacheToUse", AppSettingsKeyPrefix);

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
                _isCachePrefixDependenciesEnabled = ConfigurationManager.AppSettings[isCachePrefixDependenciesKey].ToBoolean();
            }
            if (ConfigurationManager.AppSettings[cacheSpecificDataKey].HasValue())
            {
                _cacheSpecificData = ConfigurationManager.AppSettings[cacheSpecificDataKey];
            }
        }
	}
}
