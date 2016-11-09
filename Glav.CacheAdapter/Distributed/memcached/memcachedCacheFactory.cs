using System;
using Glav.CacheAdapter.Core.Diagnostics;
using Enyim.Caching;
using Enyim.Caching.Memcached;
using Glav.CacheAdapter.Core;
using Glav.CacheAdapter.DependencyManagement;

namespace Glav.CacheAdapter.Distributed.memcached
{
    public class memcachedCacheFactory : CacheConstructionFactoryBase
    {
        private const string DEFAULT_IpAddress = "127.0.0.1";
        private const int DEFAULT_Port = 11211;

        private int _minPoolSize = 10;
        private int _maxPoolSize = 20;
        private TimeSpan _connectTimeout = TimeSpan.FromSeconds(5);
        private TimeSpan _deadNodeTimeout = TimeSpan.FromSeconds(30);
        private static bool _isInitialised;
        private static readonly object _lockRef = new object();
        private static IMemcachedClient _client;


        public memcachedCacheFactory(ILogging logger, CacheConfig config = null)
            : base(logger, config)
        {
            ParseConfig(DEFAULT_IpAddress, DEFAULT_Port);
            ExtractCacheSpecificConfig();
        }

        public int MinimumPoolSize { get { return _minPoolSize; } }
        public int MaximumPoolSize { get { return _maxPoolSize; } }
        public TimeSpan ConnectTimeout { get { return _connectTimeout; } }
        public TimeSpan DeadNodeTimeout { get { return _deadNodeTimeout; } }

        public override CacheFactoryComponentResult CreateCacheComponents()
        {
            var cacheEngine = CreateCacheEngine();
            var dependencyMgr = new GenericDependencyManager(cacheEngine, Logger, CacheConfiguration);
            var featureSupport = new memcachedFeatureSupport();
            var result = CacheFactoryComponentResult.Create(cacheEngine, dependencyMgr, featureSupport, CacheConfiguration, Logger);
            return result;
        }

        private ICache CreateCacheEngine()
        {
            if (!_isInitialised)
            {
                lock (_lockRef)
                {
                    if (!_isInitialised)
                    {
                        _isInitialised = true;
                        var config = new Enyim.Caching.Configuration.MemcachedClientConfiguration();
                        var serverFarm = ConstructCacheFarm();
                        serverFarm.NodeList.ForEach(n => config.AddServer(n.IPAddressOrHostName, n.Port));
                        config.SocketPool.ConnectionTimeout = ConnectTimeout;
                        config.SocketPool.DeadTimeout = DeadNodeTimeout;

                        config.SocketPool.MaxPoolSize = MaximumPoolSize;
                        config.SocketPool.MinPoolSize = MinimumPoolSize;

                        // Note: Tried using the Binary protocol here but I consistently got unreliable results in tests.
                        // TODO: Need to investigate further why Binary protocol is unreliable in this scenario.
                        // Could be related to memcached version and/or transcoder.
                        config.Protocol = MemcachedProtocol.Text;
                        config.Transcoder = new DataContractTranscoder();
                        _client = new MemcachedClient(config);
                        Logger.WriteInfoMessage("memcachedAdapter initialised.");
                        LogManager.AssignFactory(new LogFactoryAdapter(Logger));
                    }
                }
            }
            return new memcachedAdapter(Logger,_client);

        }

        /// <summary>
        /// Disposes of the memcached client that was created by this factory
        /// </summary>
        /// <param name="client"></param>
        public void DestroyCache()
        {
            if (_client != null)
            {
                _client.Dispose();
            }
        }

        private CacheServerFarm ConstructCacheFarm()
        {
            try
            {
                var serverFarm = new CacheServerFarm(Logger);
                serverFarm.Initialise(CacheConfiguration.ServerNodes);
                if (serverFarm.NodeList == null || serverFarm.NodeList.Count == 0)
                {
                    var msg = "Must specify at least 1 server node to use for memcached";
                    Logger.WriteErrorMessage(msg);
                    throw new ArgumentException(msg);
                }
                return serverFarm;
            }
            catch (Exception ex)
            {
                Logger.WriteException(ex);
                throw;
            }
        }

        private void ExtractCacheSpecificConfig()
        {
            var minPoolSizeValue = CacheConfiguration.GetConfigValueFromProviderSpecificValues(memcachedConstants.CONFIG_MinimumConnectionPoolSize);
            var maxPoolSizeValue = CacheConfiguration.GetConfigValueFromProviderSpecificValues(memcachedConstants.CONFIG_MaximumConnectionPoolSize);
            int value;
            if (int.TryParse(minPoolSizeValue, out value))
            {
                _minPoolSize = value;
            }
            if (int.TryParse(maxPoolSizeValue, out value))
            {
                _maxPoolSize = value;
            }

            var connectTimeoutValue = CacheConfiguration.GetConfigValueFromProviderSpecificValues(memcachedConstants.CONFIG_ConnectionTimeout);
            var deadTimeoutValue = CacheConfiguration.GetConfigValueFromProviderSpecificValues(memcachedConstants.CONFIG_DeadNodeTimeout);

            try
            {
                if (connectTimeoutValue != null)
                {
                    _connectTimeout = TimeSpan.Parse(connectTimeoutValue);
                }
                if (deadTimeoutValue != null)
                {
                    _deadNodeTimeout = TimeSpan.Parse(deadTimeoutValue);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteErrorMessage(string.Format("Unable to parse timeout values. [{0}]", ex.Message));
            }
        }

    }
}
