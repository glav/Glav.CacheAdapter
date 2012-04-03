using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Glav.CacheAdapter.Core.Diagnostics;

namespace Glav.CacheAdapter.Distributed.memcached.Protocol
{
	class SetCommand : GenericCommandProcessor
	{
		private const string FLAGS = "0";
		private const string CAS = "0";

		public SetCommand(ILogging logger, string ipAddress, int port): base(logger, SupportedCommands.Set, ipAddress,port)
		{
		}

		public string CacheKey { get; set; }
		public DateTime ItemExpiry { get; set; }
		public object Data { get; set; }

		public override CommandResponse ExecuteCommand()
		{
			long expiryTimeInSeconds = 0;
			if (ItemExpiry != DateTime.MaxValue)
			{
				var now = DateTime.Now;
				if (ItemExpiry > now)
				{
					var span = ItemExpiry - now;
					expiryTimeInSeconds = GetExpiryTimeInSeconds(span);
				}
				else
				{
					expiryTimeInSeconds = 1;
				}
			}
			var dataToStore = SerialiseData(Data);
			var dataLength = dataToStore.Length;
			var cmdBytes = SetCommandParameters(CacheKey, FLAGS, expiryTimeInSeconds.ToString(), dataLength.ToString(), CAS);
			List<byte> cmdSegment = new List<byte>(cmdBytes);
			cmdSegment.AddRange(dataToStore);
			cmdSegment.AddRange(UTF8Encoding.ASCII.GetBytes(ServerProtocol.Command_Terminator));

			var response = new CommandResponse();
			try
			{
				var result = ProtocolSocket.Send(cmdSegment.ToArray());
				response = ProcessResponse(result);
			}
			catch (Exception ex)
			{
				response.Status = CommandResponseStatus.Error;
			}
			return response;
		}

		private long GetExpiryTimeInSeconds(TimeSpan span)
		{
			long expiryTimeInSeconds;
			var roundedExpiryTime = Math.Round(span.TotalSeconds, 0);
			try
			{
				expiryTimeInSeconds = (long)roundedExpiryTime;
			}
			catch
			{
				expiryTimeInSeconds = 0;
			}
			if (expiryTimeInSeconds < 0)
			{
				expiryTimeInSeconds = 0;
			}
			return expiryTimeInSeconds;
		}

		protected override CommandResponse ProcessResponse(byte[] rawResponse)
		{
			var response = base.ProcessResponse(rawResponse);
			if (response.Status == CommandResponseStatus.Ok)
			{
				if (response.ResponseText != ServerProtocol.SetSuccessResponse + ServerProtocol.Command_Terminator)
				{
					response.Status = CommandResponseStatus.ServerError;
				}
			}

			return response;
		}
	}

}
