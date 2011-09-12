using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Glav.CacheAdapter.Core.Diagnostics;

namespace Glav.CacheAdapter.Distributed.memcached.Protocol
{
	public class StatisticsCommand : GenericCommandProcessor
	{
		public StatisticsCommand(ILogging logger, string ipAddress, int port) : base(logger,SupportedCommands.Statistics,ipAddress,port)
		{
			
		}

		public override CommandResponse ExecuteCommand()
		{
			var response = base.ExecuteCommand();
			response.ResponseText = UTF8Encoding.ASCII.GetString(response.RawData);
			return response;
		}
	}
}
