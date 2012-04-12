using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Net;
using System.IO;

namespace Glav.CacheAdapter.Distributed.memcached.Protocol
{
	public class CommandSocket
	{
		private const string DEFAULT_IP = "127.0.0.1";
		private const int DEFAULT_PORT = 11211;  // default port for memcached
		private string _ipAddress;
		private int _port;
		private int _receiveTimeout = 10000;


		public event EventHandler<CommunicationFailureEventArgs> CommunicationFailure;

		public CommandSocket(string ipAddress, int port)
		{
			_ipAddress = ipAddress;
			if (string.IsNullOrWhiteSpace(ipAddress))
			{
				_ipAddress = DEFAULT_IP;
			}
			_port = port;

			if (_port == 0)
			{
				_port = DEFAULT_PORT;
			}
		}

		public int ReceiveTimeOut
		{
			get { return _receiveTimeout; }
			set
			{
				if (value > 0)
				{
					_receiveTimeout = value;
				}
			}
		}


		public byte[] Send(string command)
		{
			var cmdBuffer = UTF8Encoding.ASCII.GetBytes(command);
			return Send(cmdBuffer);
		}
		public byte[] Send(byte[] commandBuffer)
		{
			List<byte> allData = new List<byte>();

			try
			{
				using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
				{
					socket.ReceiveTimeout = _receiveTimeout;
					socket.Connect(_ipAddress, _port);
					using (var netStream = new NetworkStream(socket, true))
					{
						socket.Send(commandBuffer);
						socket.Shutdown(SocketShutdown.Send);

						bool keepReading = true;
						while (keepReading)
						{
							var byteRead = netStream.ReadByte();
							if (byteRead < 0)
								keepReading = false;

							if (byteRead >= 0)
							{
								allData.Add((byte)byteRead);
							}
						}
						socket.Close(1);
					}
				}
			}
			catch (Exception ex)
			{
				//todo: should log 'ex.Message' somewhere
				FireCommunicationFailedEvent(ex);
			}

			return allData.ToArray();
		}

		private void FireCommunicationFailedEvent(Exception failureException)
		{
			if (CommunicationFailure != null)
			{
				ServerNode failedNode = new ServerNode() { IPAddressOrHostName = _ipAddress, Port = _port, IsAlive = false };
				CommunicationFailure(this, new CommunicationFailureEventArgs(failedNode, failureException));
			}
		}
	}

	public class CommunicationFailureEventArgs : EventArgs
	{
		public CommunicationFailureEventArgs(ServerNode failedNode, Exception failureException)
		{
			FailedNode = failedNode;
			FailureException = failureException;
		}
		public ServerNode FailedNode { get; set; }
		public Exception FailureException { get; set; }
	}
}
