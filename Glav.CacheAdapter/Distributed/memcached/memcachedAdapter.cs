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
			var node = _serverFarm.FindCacheServerNodeForKey(cacheKey);
			var cmd = new GetCommand(node.IPAddressOrHostName, node.Port);
			cmd.CacheKey = cacheKey;
			cmd.CommunicationFailure += new EventHandler<CommunicationFailureEventArgs>(HandleCommunicationFailureEvent);
			var response = cmd.ExecuteCommand();
			if (response.Status != CommandResponseStatus.Ok)
			{
				_logger.WriteErrorMessage(string.Format("Unable to Get item from memcached cache: {0},{1}", response.Status, response.ResponseText));
				return null;
			}

			return response.ResponseObject as T;
		}

		public void Add(string cacheKey, DateTime absoluteExpiry, object dataToAdd)
		{
			var node = _serverFarm.FindCacheServerNodeForKey(cacheKey);
			var cmd = new SetCommand(node.IPAddressOrHostName, node.Port);
			cmd.CacheKey = cacheKey;
			cmd.ItemExpiry = absoluteExpiry;
			cmd.Data = dataToAdd;
			cmd.CommunicationFailure += new EventHandler<CommunicationFailureEventArgs>(HandleCommunicationFailureEvent);
			
			var response = cmd.ExecuteCommand();
			if (response.Status != CommandResponseStatus.Ok)
			{
				_logger.WriteErrorMessage(string.Format("Unable to Add item to memcached cache: {0},{1}",response.Status,response.ResponseText));
			}
		}

		public void Add(string cacheKey, TimeSpan slidingExpiryWindow, object dataToAdd)
		{
			// memcached does not support sliding windows so we convert it to an absolute expiry
			var absoluteExpiry = DateTime.Now.AddSeconds(slidingExpiryWindow.TotalSeconds);
			Add(cacheKey,absoluteExpiry,dataToAdd);
		}

		public void InvalidateCacheItem(string cacheKey)
		{
			var node = _serverFarm.FindCacheServerNodeForKey(cacheKey);
			var cmd = new DeleteCommand(node.IPAddressOrHostName, node.Port);
			cmd.CacheKey = cacheKey;
			cmd.CommunicationFailure += new EventHandler<CommunicationFailureEventArgs>(HandleCommunicationFailureEvent);
			var response = cmd.ExecuteCommand();
			if (response.Status != CommandResponseStatus.Ok)
			{
				_logger.WriteErrorMessage(string.Format("Unable to Delete item from memcached cache: {0},{1}", response.Status, response.ResponseText));
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
	}
}
