using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Glav.CacheAdapter.Core.Diagnostics;

namespace Glav.CacheAdapter.Distributed.memcached
{
	public class memcachedCacheFactory : DistributedCacheFactoryBase
	{
		private const string DEFAULT_IpAddress = "127.0.0.1";
		private const int DEFAULT_Port = 11211;

		private int _minPoolSize = 10;
		private int _maxPoolSize = 20;
		private TimeSpan _connectTimeout = TimeSpan.FromSeconds(5);
		private TimeSpan _deadNodeTimeout = TimeSpan.FromSeconds(30);


		public memcachedCacheFactory(ILogging logger, CacheConfig config = null) : base(logger,config)
		{
			ParseConfig(DEFAULT_IpAddress, DEFAULT_Port);
			ExtractCacheSpecificConfig();
		}

		public int MinimumPoolSize { get { return _minPoolSize; }}
		public int MaximumPoolSize { get { return _maxPoolSize; } }
		public TimeSpan ConnectTimeout { get { return _connectTimeout; } }
		public TimeSpan DeadNodeTimeout { get { return _deadNodeTimeout; } }

		public CacheServerFarm ConstructCacheFarm()
		{
			try
			{
				var serverFarm = new CacheServerFarm(Logger);
				serverFarm.Initialise(CacheConfiguration.ServerNodes);
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
			var minPoolSizeValue = SafeGetCacheConfigValue(memcachedConstants.CONFIG_MinimumConnectionPoolSize);
			var maxPoolSizeValue = SafeGetCacheConfigValue(memcachedConstants.CONFIG_MaximumConnectionPoolSize);
			int value;
			if (int.TryParse(minPoolSizeValue, out value))
			{
				_minPoolSize = value;
			}
			if (int.TryParse(maxPoolSizeValue, out value))
			{
				_maxPoolSize = value;
			}

			var connectTimeoutValue = SafeGetCacheConfigValue(memcachedConstants.CONFIG_ConnectionTimeout);
			var deadTimeoutValue = SafeGetCacheConfigValue(memcachedConstants.CONFIG_DeadNodeTimeout);

			TimeSpan timeValue;
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

		private string SafeGetCacheConfigValue(string key)
		{
			if (CacheConfiguration.ProviderSpecificValues.ContainsKey(key))
			{
				return CacheConfiguration.ProviderSpecificValues[key];
			}

			return null;
		}
	}
}
