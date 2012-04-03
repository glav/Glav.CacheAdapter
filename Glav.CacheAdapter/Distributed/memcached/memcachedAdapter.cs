using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Glav.CacheAdapter.Core;
using Glav.CacheAdapter.Core.Diagnostics;
using Glav.CacheAdapter.Distributed.memcached.Protocol;

namespace Glav.CacheAdapter.Distributed.memcached
{
	public class memcachedAdapter : ICache
	{
		private CacheServerFarm _serverFarm;
		private ILogging _logger;

		public memcachedAdapter(ILogging logger)
		{
			_logger = logger;

			var factory = new memcachedCacheFactory(_logger);
			_serverFarm = factory.ConstructCacheFarm();

			if (_serverFarm == null || _serverFarm.NodeList == null || _serverFarm.NodeList.Count == 0)
				throw new ArgumentException("Must specify at least 1 server node to use for memcached");
		}


		public T Get<T>(string cacheKey) where T : class
		{
			try
			{
				var sanitisedKey = SanitiseCacheKey(cacheKey);
				var node = _serverFarm.FindCacheServerNodeForKey(sanitisedKey);
				var cmd = new GetCommand(_logger, node.IPAddressOrHostName, node.Port);
				cmd.CacheKey = sanitisedKey;
				cmd.CommunicationFailure += new EventHandler<CommunicationFailureEventArgs>(HandleCommunicationFailureEvent);
				var response = cmd.ExecuteCommand();
				if (response.Status != CommandResponseStatus.Ok)
				{
					_logger.WriteErrorMessage(string.Format("Unable to Get item from memcached cache: {0},{1}", response.Status, response.ResponseText));
					return null;
				}

				return response.ResponseObject as T;
			}
			catch (Exception ex)
			{
				_logger.WriteException(ex);
			}
			return null;
		}

		public void Add(string cacheKey, DateTime absoluteExpiry, object dataToAdd)
		{
			try
			{
				var sanitisedKey = SanitiseCacheKey(cacheKey);
				var node = _serverFarm.FindCacheServerNodeForKey(sanitisedKey);
				var cmd = new SetCommand(_logger, node.IPAddressOrHostName, node.Port);
				cmd.CacheKey = sanitisedKey;
				cmd.ItemExpiry = absoluteExpiry;
				cmd.Data = dataToAdd;
				cmd.CommunicationFailure += new EventHandler<CommunicationFailureEventArgs>(HandleCommunicationFailureEvent);

				var response = cmd.ExecuteCommand();
				if (response.Status != CommandResponseStatus.Ok)
				{
					_logger.WriteErrorMessage(string.Format("Unable to Add item to memcached cache: {0},{1}", response.Status, response.ResponseText));
				}
			}
			catch (Exception ex)
			{
				_logger.WriteException(ex);
			}
		}

		public void Add(string cacheKey, TimeSpan slidingExpiryWindow, object dataToAdd)
		{
			var sanitisedKey = SanitiseCacheKey(cacheKey);
			// memcached does not support sliding windows so we convert it to an absolute expiry
			var absoluteExpiry = DateTime.Now.AddSeconds(slidingExpiryWindow.TotalSeconds);
			Add(sanitisedKey, absoluteExpiry, dataToAdd);
		}

		public void InvalidateCacheItem(string cacheKey)
		{
			try
			{
				var sanitisedKey = SanitiseCacheKey(cacheKey);
				var node = _serverFarm.FindCacheServerNodeForKey(sanitisedKey);
				var cmd = new DeleteCommand(_logger, node.IPAddressOrHostName, node.Port);
				cmd.CacheKey = sanitisedKey;
				cmd.CommunicationFailure += new EventHandler<CommunicationFailureEventArgs>(HandleCommunicationFailureEvent);
				var response = cmd.ExecuteCommand();
				if (response.Status != CommandResponseStatus.Ok)
				{
					_logger.WriteErrorMessage(string.Format("Unable to Delete item from memcached cache: {0},{1}", response.Status, response.ResponseText));
				}
			}
			catch (Exception ex)
			{
				_logger.WriteException(ex);
			}
		}

		public void AddToPerRequestCache(string cacheKey, object dataToAdd)
		{
			// memcached does not have a per request concept nor does it need to since all cache nodes should be in sync
			// You could simulate this in code with a dependency on the ASP.NET framework and its inbuilt request
			// objects but we wont do that here. We simply add it into the cache for 1 second.
			// Its hacky but this behaviour will be specific to the scenario at hand.
			Add(cacheKey, TimeSpan.FromSeconds(1), dataToAdd);
		}

		void HandleCommunicationFailureEvent(object sender, CommunicationFailureEventArgs e)
		{
			if (e.FailureException != null)
			{
				_logger.WriteException(e.FailureException);
			}
			if (e.FailedNode != null)
			{
				_logger.WriteErrorMessage(string.Format("A memcached node has failed! [{0}:{1}]", e.FailedNode.IPAddressOrHostName,
				                                        e.FailedNode.Port));
			}
			_serverFarm.SetNodeToDead(e.FailedNode);
		}

		public CacheSetting CacheType
		{
			get { return CacheSetting.memcached; }
		}

		private string SanitiseCacheKey(string cacheKey)
		{
			if (string.IsNullOrWhiteSpace(cacheKey))
			{
				throw new ArgumentException("Cannot have an empty or NULL cache key");
			}
			return cacheKey.Replace(" ", string.Empty);
		}
	}
}
