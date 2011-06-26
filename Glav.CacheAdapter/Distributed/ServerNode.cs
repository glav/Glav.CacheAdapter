﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Glav.CacheAdapter.Distributed
{
	public class ServerNode
	{
		public ServerNode()
		{
			IsAlive = true;
			Port = 11211;
			IPAddressOrHostName = "127.0.0.1";
		}

		public ServerNode(string ipAddress, int port)
		{
			IPAddressOrHostName = ipAddress;
			Port = port;
			IsAlive = true;
		}
		public string IPAddressOrHostName { get; set; }
		public int Port { get; set; }
		public bool IsAlive { get; set; }

		public string GetFullHostAddress()
		{
			if (Port == 0)
				Port = 11211;
			if (string.IsNullOrWhiteSpace(IPAddressOrHostName))
				IPAddressOrHostName = "127.0.0.1";

			return string.Format("{0}:{1}", IPAddressOrHostName, Port);
		}
	}
}
