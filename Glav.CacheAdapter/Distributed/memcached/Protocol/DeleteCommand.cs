using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Glav.CacheAdapter.Distributed.memcached.Protocol
{
	public class DeleteCommand : GenericCommandProcessor
	{
		public DeleteCommand(string ipAddress, int port)
			: base(SupportedCommands.Delete, ipAddress, port)
		{
		}

		public string CacheKey { get; set; }

		public override CommandResponse ExecuteCommand()
		{
			SetCommandParameters(CacheKey);
			return base.ExecuteCommand();
		}

		protected override CommandResponse ProcessResponse(byte[] rawResponse)
		{
			var response = base.ProcessResponse(rawResponse);
			if (response.Status == CommandResponseStatus.Ok)
			{
				var okResponse1 = ServerProtocol.ItemDeletedSuccessResponse + ServerProtocol.Command_Terminator;
				var okResponse2 = ServerProtocol.ItemDeletedNotFoundResoinse + ServerProtocol.Command_Terminator;
				if (CheckForByteSequenceInArray(rawResponse,okResponse1) || CheckForByteSequenceInArray(rawResponse,okResponse2))
				{
					response.Status = CommandResponseStatus.Ok;
					response.ResponseObject = null;
					return response;
				}
			}

			response.Status = CommandResponseStatus.Error;
			return response;
		}
	}
}
