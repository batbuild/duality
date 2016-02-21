using System.Net.Sockets;

namespace Duality
{
	public struct ClientConnection
	{
		public TcpClient TcpClient { get; set; }
		public NetworkStream Stream { get; set; }
	}
}