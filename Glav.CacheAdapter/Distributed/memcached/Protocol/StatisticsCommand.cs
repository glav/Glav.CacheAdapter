using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Glav.CacheAdapter.Distributed.memcached.Protocol
{
	public class StatisticsCommand : GenericCommandProcessor
	{
		public StatisticsCommand(string ipAddress, int port) : base(SupportedCommands.Statistics,ipAddress,port)
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
