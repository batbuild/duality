using System.Net.Sockets;

namespace Duality
{
	public class ClientConnection
	{
		public TcpClient TcpClient { get; set; }
		public NetworkStream Stream { get; set; }
	}
}