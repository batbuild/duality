using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using LZ4;

namespace Duality
{
	public static class NetworkProfileServer
	{
		private static List<ClientConnection> _clients = new List<ClientConnection>();
		private static object _syncLock = new object();
		private static bool _hasBeenInitialized;
		private static TcpChannel _tcpChannel;
		private static Stopwatch _frameTime;
		private static FrameData _frameData;

		public static void Init(int port)
		{
			Profile.NetworkMode = true;

			_tcpChannel = new TcpChannel(port);
			_tcpChannel.ClientConnected += OnClientConnected;

			_frameTime = Stopwatch.StartNew();
			_frameData = new FrameData {FrameEvents = new List<FrameEvent>()};
			_hasBeenInitialized = true;
		}

		private static void OnClientConnected(TcpClient client)
		{
			lock (_syncLock)
			{
				_clients.Add(new ClientConnection { TcpClient = client, Stream = client.GetStream()});
			}
		}
		
		public static void BeginFrame()
		{
			if (_hasBeenInitialized == false)
				return;

			_frameData.StartTime = _frameTime.ElapsedTicks;
			_frameData.FrameEvents.Clear();
		}

		public static void EndFrame(IList<TimeCounter> counters)
		{
			if (_hasBeenInitialized == false)
				return;

			Profile.BeginMeasure("NetworkProfiler\\EndFrame");
			_frameData.EndTime = _frameTime.ElapsedTicks;
			var numCounters = counters.Count;

			lock (_syncLock)
			{
				for (var i = _clients.Count - 1; i >= 0; i--)
				{
					var client = _clients[i];
					using (var memoryStream = new MemoryStream())
					using (var writer = new BinaryWriter(memoryStream))
					{
						writer.Write(_frameData.StartTime);
						writer.Write(_frameData.EndTime);

						writer.Write(numCounters);
						foreach (var counter in counters)
						{
							writer.Write(counter.FullName);
							writer.Write(counter.LastValue);
						}
						
						try
						{
							var compressed = LZ4Codec.Encode(memoryStream.ToArray(), 0, (int)memoryStream.Length);
							client.Stream.Write(BitConverter.GetBytes(compressed.Length), 0, 4);
							client.Stream.Write(BitConverter.GetBytes(memoryStream.Length), 0, 4);
							client.Stream.Write(compressed, 0, compressed.Length);
						}
						catch (InvalidOperationException)
						{ }
						catch(IOException)
						{ }
						catch (SocketException)
						{ }

						if (client.TcpClient.Connected == false)
						{
							Log.Core.Write("Network profile client disconnected");
							_clients.Remove(client);
						}
					}
				}
			}
			Profile.EndMeasure("NetworkProfiler\\EndFrame");
		}
		
		public static void Shutdown()
		{
			if (_tcpChannel != null)
			{
				_tcpChannel.Dispose();
				_tcpChannel = null;
			}

			lock (_syncLock)
			{
				foreach (var tcpClient in _clients)
				{
					tcpClient.TcpClient.Close();
				}
				_clients.Clear();
			}
		}
	}
}