using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Duality
{
	public class NetworkProfileClient : IDisposable
	{
		public event EventHandler<FrameReceivedEventArgs> FrameReceived; 

		private TcpClient _client;
		private NetworkStream _stream;

		public NetworkStream Stream
		{
			get { return _stream; }
		}

		public bool Init(string hostname, int port)
		{
			try
			{
				_client = new TcpClient(hostname, port);
			}
			catch (SocketException e)
			{
				Log.Core.WriteError("An error occured while connecting to the network profiler: {0}", e.Message);
			}

			if (_client == null || _client.Connected == false)
			{
				Log.Core.WriteWarning("Couldn't connect to server");
				return false;
			}

			_stream = _client.GetStream();

			Task.Run(() => ReadMessages());
			return true;
		}

		private void ReadMessages()
		{
			var packetReader = new MessageReader(_stream);
			foreach (var message in packetReader.ReadMessages())
			{
				ParseMessage(message);
			}
		}

		private void ParseMessage(byte[] packet)
		{
			var frameData = new FrameData();
			frameData.FrameEvents = new List<FrameEvent>();

			using (var stream = new MemoryStream(packet))
			using (var reader = new BinaryReader(stream))
			{
				frameData.StartTime = reader.ReadInt64();
				frameData.EndTime = reader.ReadInt64();

				var numEvents = reader.ReadInt32();
				for (var i = 0; i < numEvents; i++)
				{
					var frameEvent = new FrameEvent {CounterName = reader.ReadString(), FrameTime = reader.ReadSingle()};
					frameData.FrameEvents.Add(frameEvent);
				}
			}
			
			OnFrameReceived(frameData);
		}

		private void OnFrameReceived(FrameData frameData)
		{
			var handler = FrameReceived;
			if(handler != null)
				handler(this, new FrameReceivedEventArgs(frameData));
		}

		public void Dispose()
		{
			if (_stream != null)
			{
				_stream.Close();
				_stream = null;
			}

			if (_client != null)
			{
				_client.Close();
				_client = null;
			}
		}
	}
}