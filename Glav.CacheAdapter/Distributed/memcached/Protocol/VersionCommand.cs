using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Glav.CacheAdapter.Distributed.memcached.Protocol
{
	public class VersionCommand : GenericCommandProcessor
	{
		public VersionCommand(string ipAddress, int port)
			: base(SupportedCommands.Version, ipAddress, port)
		{

		}

		public override CommandResponse ExecuteCommand()
		{
			var response = base.ExecuteCommand();
			if (response.Status == CommandResponseStatus.Ok)
			{
				response.ResponseText = UTF8Encoding.ASCII.GetString(response.RawData);
				if (CheckForByteSequenceInArray(response.RawData, ServerProtocol.VersionResponse))
				{
					response.Status = CommandResponseStatus.Ok;
				}
				else
				{
					response.Status = CommandResponseStatus.Error;
				}
				return response;
			}

			response.Status = CommandResponseStatus.Error;
			return response;
		}
	}
}
