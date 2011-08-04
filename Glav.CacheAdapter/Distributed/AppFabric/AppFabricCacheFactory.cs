using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.ApplicationServer.Caching;
using System.Diagnostics;
using Glav.CacheAdapter.Core.Diagnostics;
using System.Security;

namespace Glav.CacheAdapter.Distributed.AppFabric
{
    public class AppFabricCacheFactory: DistributedCacheFactoryBase
    {
        public AppFabricCacheFactory(ILogging logger) : base(logger)
        {
        }

        public DataCache ConstructCache()
        {
			var config = ParseConfig(AppFabricConstants.DEFAULT_ServerAddress, AppFabricConstants.DEFAULT_Port);
			var dataCacheEndpoints = new List<DataCacheServerEndpoint>();
			config.ServerNodes.ForEach(e => dataCacheEndpoints.Add(new DataCacheServerEndpoint(e.IPAddressOrHostName,e.Port)));

            var factoryConfig = new DataCacheFactoryConfiguration();
			SetSecuritySettings(config, factoryConfig);

			factoryConfig.Servers = dataCacheEndpoints;

            try
            {
				var factory = new DataCacheFactory(factoryConfig);
                DataCacheClientLogManager.ChangeLogLevel(System.Diagnostics.TraceLevel.Error);


				// Note: When setting up AppFabric. The configured cache needs to be created by the admin using the New-Cache powershell command
            	string cacheName;
				// Prefer the new config mechanismover the explicit entry but still support it. So we
				// try and extract config from the ProviderSpecificValues first.
				if (config.ProviderSpecificValues.ContainsKey(AppFabricConstants.CONFIG_CacheNameKey))
				{
					cacheName = config.ProviderSpecificValues[AppFabricConstants.CONFIG_CacheNameKey];
				} else
				{
					cacheName = MainConfig.Default.DistributedCacheName;
				}
				var cache = factory.GetCache(cacheName);

                return cache;
            }
            catch (Exception ex)
            {
                Logger.WriteException(ex);
                throw;
            }
        }

		private void SetSecuritySettings(CacheConfig config, DataCacheFactoryConfiguration factoryConfig)
		{
			string securityModeValue = null;
			// Set the security mode if required
			if (config.ProviderSpecificValues.ContainsKey(AppFabricConstants.CONFIG_SecurityModeKey))
			{
				securityModeValue = config.ProviderSpecificValues[AppFabricConstants.CONFIG_SecurityModeKey];
			}

			// Set the authorization info/value if required
			string securityAuthValue = null;
			if (config.ProviderSpecificValues.ContainsKey(AppFabricConstants.CONFIG_SecurityMessageAuthorisationKey))
			{
				securityAuthValue = config.ProviderSpecificValues[AppFabricConstants.CONFIG_SecurityMessageAuthorisationKey];
			}

			string useSslValue = null;
			if (config.ProviderSpecificValues.ContainsKey(AppFabricConstants.CONFIG_UseSslKey))
			{
				useSslValue = config.ProviderSpecificValues[AppFabricConstants.CONFIG_UseSslKey];
			}

			var normalisedSecurityMode = string.IsNullOrWhiteSpace(securityModeValue) ? string.Empty : securityModeValue.ToLowerInvariant();
			var normalisedSslValue = string.IsNullOrWhiteSpace(useSslValue) ? string.Empty : useSslValue.ToLowerInvariant();
			if (!string.IsNullOrWhiteSpace(securityAuthValue))
			{
				if (normalisedSecurityMode == AppFabricConstants.CONFIG_SecurityMode_Message)
				{
					var secureToken = new SecureString();
					foreach (var ch in securityAuthValue)
					{
						secureToken.AppendChar(ch);
					}
					bool useSsl = false;
					if (normalisedSslValue == "true" || normalisedSslValue == "1")
					{
						useSsl = true;
					}
					DataCacheSecurity securityProps = new DataCacheSecurity(secureToken, useSsl);
					factoryConfig.SecurityProperties = securityProps;
				}

				if (normalisedSecurityMode == AppFabricConstants.CONFIG_SecurityMode_Transport)
				{
					DataCacheSecurity securityProps = new DataCacheSecurity(DataCacheSecurityMode.Transport,DataCacheProtectionLevel.None);
					factoryConfig.SecurityProperties = securityProps;
				}
			}
		}

    }
}
