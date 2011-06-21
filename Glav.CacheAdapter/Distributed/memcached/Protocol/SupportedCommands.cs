using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Glav.CacheAdapter.Distributed.memcached.Protocol
{
	public enum SupportedCommands
	{
		Statistics,
		Set,
		Get,
		Delete,
		Version
	}

	public class CommandMapper
	{
		readonly Dictionary<SupportedCommands,string> _cmds = new Dictionary<SupportedCommands, string>();

		public CommandMapper()
		{
			_cmds.Add(SupportedCommands.Statistics, "stats");
			_cmds.Add(SupportedCommands.Get,"get {0}");  // key
			_cmds.Add(SupportedCommands.Set,"set {0} {1} {2} {3}");  // key flags expires length cas  -> then data follows on next line
			_cmds.Add(SupportedCommands.Delete,"delete {0}"); // key
			_cmds.Add(SupportedCommands.Version,"version");
		}

		public string GetCommandFormat(SupportedCommands command)
		{
			return _cmds[command];
		}
	}
}
