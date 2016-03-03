namespace Glav.CacheAdapter.Distributed.AppFabric
{
    public static class AppFabricConstants
    {
        public const string DEFAULT_ServerAddress = "localhost";
        public const string CONFIG_CacheNameKey = "DistributedCacheName";
        public const string CONFIG_SecurityModeKey = "SecurityMode";
        public const string CONFIG_ProtectionLevelKey = "ProtectionMode";
        public const string CONFIG_SecurityMessageAuthorisationKey = "MessageSecurityAuthorizationInfo";
        public const string CONFIG_UseSslKey = "UseSsl";
        public const string CONFIG_ChannelOpenTimeout = "ChannelOpenTimeout";
        public const string CONFIG_MaxConnectionsToServer = "MaxConnectionsToServer";

        public const string CONFIG_LocalCache_IsEnabled = "LocalCache.IsEnabled";
        public const string CONFIG_LocalCache_DefaultTimeout = "LocalCache.DefaultTimeout";
        public const string CONFIG_LocalCache_ObjectCount = "LocalCache.ObjectCount";
        public const string CONFIG_LocalCache_InvalidationPolicy = "LocalCache.InvalidationPolicy";
        public const string CONFIG_LocalCache_InvalidationPolicyValue_TimeoutBased = "timeoutbased";
        public const string CONFIG_LocalCache_InvalidationPolicyValue_NotificationBased = "notificationbased";
        public const string CONFIG_UseDomainServiceAccount = "UseDomainServiceAccount";

        public const string CONFIG_SecurityMode_Message = "message";
        public const string CONFIG_SecurityMode_None = "none";
        public const string CONFIG_SecurityMode_Transport = "transport";

        public const string CONFIG_ProtectionLevel_None = "none";
        public const string CONFIG_ProtectionLevel_Sign = "sign";
        public const string CONFIG_ProtectionLevel_EncryptAndSign = "encryptandsign";

        public const int DEFAULT_Port = 22233;
    }
}
