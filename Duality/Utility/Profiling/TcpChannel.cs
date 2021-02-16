using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Duality
{
	public class TcpChannel : IDisposable
	{
		public Action<TcpClient> ClientConnected;

		private static TcpListener _server;

		public TcpChannel(int port)
		{
			_server = new TcpListener(IPAddress.Any, port);
			_server.Start(10);

			Task.Run(() => AcceptClients());
		}

		private void AcceptClients()
		{
			while (_server != null)
			{
				try
				{
					var client = _server.AcceptTcpClient();
					Log.Core.Write("Network profiler client connected from {0}", client.Client.RemoteEndPoint);

					if (ClientConnected != null)
						ClientConnected(client);
				}
				catch (SocketException e)
				{
					Log.Core.WriteWarning("An exception was thrown when connecting to a client: {0}", e.Message);
					if (e.SocketErrorCode == SocketError.Interrupted)
						break;
				}
			}
		}

		public void Dispose()
		{
			if (_server != null)
			{
				_server.Stop();
				_server = null;
			}
		}
	}
}