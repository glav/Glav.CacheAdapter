using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Glav.CacheAdapter.Distributed.memcached.Protocol
{
	public class CommandResponse
	{
		public byte[] RawData { get; set; }
		public string ResponseText { get; set; }
		public object ResponseObject { get; set; }
		public CommandResponseStatus Status { get; set; }
	}

	public enum CommandResponseStatus
	{
		Ok,
		Error,
		ClientError,
		ServerError
	}

	public static class ServerProtocol
	{
		public const string Command_Terminator = "\r\n";
		public const string GenericErrorResponse = "ERROR";
		public const string ClientErrorResponse = "CLIENT_ERROR ";
		public const string ServerErrorResponse = "SERVER_ERROR ";
		public const string ServerSuccessEndResponse = "END";
		public const string SetSuccessResponse = "STORED";
		public const string ItemDeletedSuccessResponse = "DELETED";
		public const string ItemDeletedNotFoundResoinse = "NOT_FOUND";
		public const string VersionResponse = "VERSION";
	}

}
