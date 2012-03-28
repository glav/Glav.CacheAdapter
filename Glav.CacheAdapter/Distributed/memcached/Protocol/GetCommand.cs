using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Glav.CacheAdapter.Core.Diagnostics;

namespace Glav.CacheAdapter.Distributed.memcached.Protocol
{
	public class GetCommand : GenericCommandProcessor
	{
		public GetCommand(ILogging logger,string ipAddress, int port)
			: base(logger, SupportedCommands.Get, ipAddress, port)
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
				if (response.ResponseText == ServerProtocol.ServerSuccessEndResponse + ServerProtocol.Command_Terminator)
				{
					// We did not find anything as the server only returned "END"
					response.ResponseObject = null;
					return response;
				}

				// The first line of a GET is always an information line with VALUE, flags etc specified so in order to get at
				// the actual object response, we need to move past that position

				// So read in the first line
				var byteSequenceToCompare = new byte[ServerProtocol.Command_Terminator.Length];
				var byteSequenceToLookFor = UTF8Encoding.ASCII.GetBytes(ServerProtocol.Command_Terminator);
				int fndPosition = -1;
				for (var byteCnt = 0; byteCnt < rawResponse.Length - ServerProtocol.Command_Terminator.Length-1; byteCnt++ )
				{
					Array.Copy(rawResponse,byteCnt,byteSequenceToCompare,0,byteSequenceToCompare.Length);
					if (byteSequenceToCompare.SequenceEqual(byteSequenceToLookFor))
					{
						fndPosition = byteCnt;
						break;
					}
				}

				if (fndPosition == -1)
				{
					throw new ProtocolViolationException("No intial response line found from GET request");
				}

				// Now extract the actual byte content of the response
				var successResponseAckInBytes =
					UTF8Encoding.ASCII.GetBytes(ServerProtocol.ServerSuccessEndResponse + ServerProtocol.Command_Terminator);
				var startPosition = fndPosition + byteSequenceToCompare.Length;

				var objectData = new byte[response.RawData.Length - successResponseAckInBytes.Length - startPosition-1];
				Array.Copy(response.RawData,startPosition, objectData,0, (long)objectData.Length);
				response.ResponseObject = DeserialiseData(objectData);
			}
			return response;
		}
	}
}
