using Glav.CacheAdapter.Core.Diagnostics;
using Microsoft.ApplicationServer.Caching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;

namespace Glav.CacheAdapter.Distributed.AppFabric
{
    internal class FactoryConfigConverter
    {
        private ILogging _logger;
        public FactoryConfigConverter(ILogging logger)
        {
            _logger = logger;
        }

        internal void MapSettingsFromConfigToAppFabricSettings(CacheConfig config, DataCacheFactoryConfiguration factoryConfig)
        {
            SetSecuritySettings(config, factoryConfig);
            SetMaxConnectionsToServer(config, factoryConfig);
            SetChannelOpenTimeout(config, factoryConfig);
        }

        private void SetSecuritySettings(CacheConfig config, DataCacheFactoryConfiguration factoryConfig)
        {
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
            var normalisedProtectionLevel = string.IsNullOrWhiteSpace(protectionLevelValue) ? string.Empty : useSslValue.ToLowerInvariant();
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
                    if (normalisedSslValue == CacheConstants.ConfigValueTrueText || normalisedSslValue == CacheConstants.ConfigValueTrueNumeric)
                    {
                        useSsl = true;
                    }
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
            string channelOpenTimeout = null;
            if (config.ProviderSpecificValues.ContainsKey(AppFabricConstants.CONFIG_ChannelOpenTimeout))
            {
                channelOpenTimeout = config.ProviderSpecificValues[AppFabricConstants.CONFIG_ChannelOpenTimeout];
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
            int maxConnectionsToServer;
            if (config.ProviderSpecificValues.ContainsKey(AppFabricConstants.CONFIG_MaxConnectionsToServer))
            {
                if (int.TryParse(config.ProviderSpecificValues[AppFabricConstants.CONFIG_MaxConnectionsToServer], out maxConnectionsToServer))
                {
                    factoryConfig.MaxConnectionsToServer = maxConnectionsToServer;
                    _logger.WriteInfoMessage(string.Format("AppFabric MaxConnectionsToServer: [{0}]", maxConnectionsToServer));
                }
            }
        }

    }
}
