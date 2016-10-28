using Glav.CacheAdapter.Core.Diagnostics;
using Microsoft.ApplicationServer.Caching;
using System;
using System.Security;

namespace Glav.CacheAdapter.Distributed.AppFabric
{
    internal class FactoryConfigConverter
    {
        private readonly ILogging _logger;
        public FactoryConfigConverter(ILogging logger)
        {
            _logger = logger;
        }

        public bool IsLocalCacheEnabled { get; set; }

        internal void MapSettingsFromConfigToAppFabricSettings(CacheConfig config, DataCacheFactoryConfiguration factoryConfig)
        {
            SetSecuritySettings(config, factoryConfig);
            SetMaxConnectionsToServer(config, factoryConfig);
            SetChannelOpenTimeout(config, factoryConfig);
            SetLocalCacheConfiguration(config, factoryConfig);
        }

        private void SetLocalCacheConfiguration(CacheConfig config, DataCacheFactoryConfiguration factoryConfig)
        {
            if (config.ProviderSpecificValues.ContainsKey(AppFabricConstants.CONFIG_LocalCache_IsEnabled))
            {
                var isEnabledValue = config.ProviderSpecificValues[AppFabricConstants.CONFIG_LocalCache_IsEnabled];
                _logger.WriteInfoMessage(string.Format("Setting AppFabric LocalCache IsEnabled:[{0}]", isEnabledValue));

                bool isEnabled;
                bool.TryParse(isEnabledValue, out isEnabled);
                if (isEnabled)
                {
                    IsLocalCacheEnabled = true;
                    int defaultTimeoutInSeconds = 0;
                    int objectCount = 0;
                    if (config.ProviderSpecificValues.ContainsKey(AppFabricConstants.CONFIG_LocalCache_DefaultTimeout))
                    {
                        int.TryParse(config.ProviderSpecificValues[AppFabricConstants.CONFIG_LocalCache_DefaultTimeout], out defaultTimeoutInSeconds);
                    }
                    if (config.ProviderSpecificValues.ContainsKey(AppFabricConstants.CONFIG_LocalCache_ObjectCount))
                    {
                        int.TryParse(config.ProviderSpecificValues[AppFabricConstants.CONFIG_LocalCache_ObjectCount], out objectCount);
                    }
                    var invalidationPolicy = DataCacheLocalCacheInvalidationPolicy.NotificationBased; // the default
                    if (config.ProviderSpecificValues.ContainsKey(AppFabricConstants.CONFIG_LocalCache_InvalidationPolicy))
                    {
                        var policyValue = config.ProviderSpecificValues[AppFabricConstants.CONFIG_LocalCache_InvalidationPolicy];
                        var normalisedPolicyValue = policyValue.ToLowerInvariant();
                        if (normalisedPolicyValue == AppFabricConstants.CONFIG_LocalCache_InvalidationPolicyValue_TimeoutBased)
                        {
                            invalidationPolicy = DataCacheLocalCacheInvalidationPolicy.TimeoutBased;
                        }
                    }

                    factoryConfig.LocalCacheProperties = new DataCacheLocalCacheProperties(objectCount, TimeSpan.FromSeconds(defaultTimeoutInSeconds), invalidationPolicy);
                }
            }
        }

        private void SetSecuritySettings(CacheConfig config, DataCacheFactoryConfiguration factoryConfig)
        {
            // If appfabric service is running as a domain account the client must specify
            // http://blogs.msdn.com/b/distributedservices/archive/2012/10/29/authenticationexception-in-appfabric-1-1-caching-for-windows-server.aspx
            if (config.ProviderSpecificValues.ContainsKey(AppFabricConstants.CONFIG_UseDomainServiceAccount))
            {
                bool useDomainServiceAccount;
                var converted = bool.TryParse(config.ProviderSpecificValues[AppFabricConstants.CONFIG_UseDomainServiceAccount] ?? "false", out useDomainServiceAccount);
                if (converted && useDomainServiceAccount)
                {
                    _logger.WriteInfoMessage("Setting AppFabric DataCacheServiceAccountType to DomainAccount.");
                    factoryConfig.DataCacheServiceAccountType = DataCacheServiceAccountType.DomainAccount;
                }
            }

            string securityModeValue = null;
            // Set the security mode if required
            if (config.ProviderSpecificValues.ContainsKey(AppFabricConstants.CONFIG_SecurityModeKey))
            {
                securityModeValue = config.ProviderSpecificValues[AppFabricConstants.CONFIG_SecurityModeKey];
                _logger.WriteInfoMessage(string.Format("Setting AppFabric security mode:[{0}]", securityModeValue));
            }

            // Set the authorization info/value if required
            string securityAuthValue = null;
            if (config.ProviderSpecificValues.ContainsKey(AppFabricConstants.CONFIG_SecurityMessageAuthorisationKey))
            {
                securityAuthValue = config.ProviderSpecificValues[AppFabricConstants.CONFIG_SecurityMessageAuthorisationKey];
                _logger.WriteInfoMessage("Setting AppFabric security Authorisation value using supplied key");
            }

            string useSslValue = null;
            if (config.ProviderSpecificValues.ContainsKey(AppFabricConstants.CONFIG_UseSslKey))
            {
                useSslValue = config.ProviderSpecificValues[AppFabricConstants.CONFIG_UseSslKey];
                _logger.WriteInfoMessage(string.Format("AppFabric Use Ssl: [{0}]", useSslValue));
            }

            string protectionLevelValue = null;
            if (config.ProviderSpecificValues.ContainsKey(AppFabricConstants.CONFIG_ProtectionLevelKey))
            {
                protectionLevelValue = config.ProviderSpecificValues[AppFabricConstants.CONFIG_ProtectionLevelKey];
                _logger.WriteInfoMessage(string.Format("AppFabric Protection Mode: [{0}]", protectionLevelValue));
            }

            var normalisedSecurityMode = string.IsNullOrWhiteSpace(securityModeValue) ? string.Empty : securityModeValue.ToLowerInvariant();
            var normalisedSslValue = string.IsNullOrWhiteSpace(useSslValue) ? string.Empty : useSslValue.ToLowerInvariant();
            var normalisedProtectionLevel = string.IsNullOrWhiteSpace(protectionLevelValue) ? string.Empty : protectionLevelValue.ToLowerInvariant();
            if (!string.IsNullOrWhiteSpace(normalisedSecurityMode) && normalisedSecurityMode != AppFabricConstants.CONFIG_SecurityMode_None)
            {
                if (normalisedSecurityMode == AppFabricConstants.CONFIG_SecurityMode_Message)
                {
                    var secureToken = new SecureString();
                    foreach (var ch in securityAuthValue)
                    {
                        secureToken.AppendChar(ch);
                    }
                    var useSsl = normalisedSslValue == CacheConstants.ConfigValueTrueText || normalisedSslValue == CacheConstants.ConfigValueTrueNumeric;
                    DataCacheSecurity securityProps = new DataCacheSecurity(secureToken, useSsl);
                    factoryConfig.SecurityProperties = securityProps;
                }

                var actualProtectionLevel = DataCacheProtectionLevel.None;
                if (!string.IsNullOrWhiteSpace(normalisedProtectionLevel))
                {
                    if (normalisedProtectionLevel == AppFabricConstants.CONFIG_ProtectionLevel_Sign)
                    {
                        actualProtectionLevel = DataCacheProtectionLevel.Sign;
                    }
                    else if (normalisedProtectionLevel == AppFabricConstants.CONFIG_ProtectionLevel_EncryptAndSign)
                    {
                        actualProtectionLevel = DataCacheProtectionLevel.EncryptAndSign;
                    }
                }

                _logger.WriteInfoMessage(string.Format("AppFabric Protection Level:[{0}]", actualProtectionLevel));

                if (normalisedSecurityMode == AppFabricConstants.CONFIG_SecurityMode_Transport)
                {
                    DataCacheSecurity securityProps = new DataCacheSecurity(DataCacheSecurityMode.Transport, actualProtectionLevel);
                    factoryConfig.SecurityProperties = securityProps;
                }

                if (normalisedSecurityMode == AppFabricConstants.CONFIG_SecurityMode_None)
                {
                    DataCacheSecurity securityProps = new DataCacheSecurity(DataCacheSecurityMode.None, actualProtectionLevel);
                    factoryConfig.SecurityProperties = securityProps;
                }
            }
        }

        private void SetChannelOpenTimeout(CacheConfig config, DataCacheFactoryConfiguration factoryConfig)
        {
            // Set the channel open timeout if required - useful for debugging
            if (config.ProviderSpecificValues.ContainsKey(AppFabricConstants.CONFIG_ChannelOpenTimeout))
            {
                var channelOpenTimeout = config.ProviderSpecificValues[AppFabricConstants.CONFIG_ChannelOpenTimeout];
                int channelOpenTimeoutValue;
                if (int.TryParse(channelOpenTimeout, out channelOpenTimeoutValue))
                {
                    factoryConfig.ChannelOpenTimeout = TimeSpan.FromSeconds(channelOpenTimeoutValue);
                    _logger.WriteInfoMessage(string.Format("Setting AppFabric ChannelOpenTimeout to {0} seconds", channelOpenTimeoutValue));
                }
                else
                {
                    _logger.WriteInfoMessage(string.Format("AppFabric ChannelOpenTimeout set to invalid value of [{0}]. Not setting", channelOpenTimeout));
                }
            }
        }

        private void SetMaxConnectionsToServer(CacheConfig config, DataCacheFactoryConfiguration factoryConfig)
        {
            if (config.ProviderSpecificValues.ContainsKey(AppFabricConstants.CONFIG_MaxConnectionsToServer))
            {
                int maxConnectionsToServer;
                if (int.TryParse(config.ProviderSpecificValues[AppFabricConstants.CONFIG_MaxConnectionsToServer], out maxConnectionsToServer))
                {
                    factoryConfig.MaxConnectionsToServer = maxConnectionsToServer;
                    _logger.WriteInfoMessage(string.Format("AppFabric MaxConnectionsToServer: [{0}]", maxConnectionsToServer));
                }
            }
        }

    }
}
