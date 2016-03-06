using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using LZ4;

namespace Duality
{
	public static class NetworkProfileServer
	{
		private static readonly TimeCounter TimeEndFrame = Profile.RequestCounter<TimeCounter>(@"NetworkProfiler\EndFrame");

		private static BlockingCollection<byte[]> _transmitBuffer = new BlockingCollection<byte[]>();
		private static ClientConnection _client;
		private static bool _hasBeenInitialized;
		private static TcpChannel _tcpChannel;
		private static Stopwatch _frameTime;
		private static FrameData _frameData;

		public static void Init(int port)
		{
			Profile.NetworkMode = true;

			_tcpChannel = new TcpChannel(port);
			_tcpChannel.ClientConnected += OnClientConnected;
			Task.Factory.StartNew(TransmitData);

			_frameTime = Stopwatch.StartNew();
			_frameData = new FrameData {FrameEvents = new List<FrameEvent>()};
			_hasBeenInitialized = true;
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

			if (_client == null)
				return;

			TimeEndFrame.BeginMeasure();
			_frameData.EndTime = _frameTime.ElapsedTicks;
			var numCounters = counters.Count;

			using (var memoryStream = new MemoryStream())
			using (var writer = new BinaryWriter(memoryStream))
			{
				writer.Write(_frameData.StartTime);
				writer.Write(_frameData.EndTime);

				writer.Write(numCounters);
				foreach (var counter in counters)
				{
					writer.Write(counter.FullNameBytes.Length);
					writer.Write(counter.FullNameBytes);
					writer.Write(counter.LastValue);
				}

				_transmitBuffer.TryAdd(memoryStream.ToArray());
			}

			TimeEndFrame.EndMeasure();
		}
		
		public static void Shutdown()
		{
			if (_transmitBuffer != null)
				_transmitBuffer.CompleteAdding();

			if (_tcpChannel != null)
			{
				_tcpChannel.Dispose();
				_tcpChannel = null;
			}

			if (_client != null)
			{
				_client.TcpClient.Close();
				_client = null;
			}
		}

		private static void TransmitData()
		{
			foreach (var data in _transmitBuffer.GetConsumingEnumerable())
			{
				if (_client == null || _client.TcpClient == null || _client.Stream == null)
					continue;

				try
				{
					var compressed = LZ4Codec.Encode(data, 0, data.Length);
					_client.Stream.Write(BitConverter.GetBytes(compressed.Length), 0, 4);
					_client.Stream.Write(BitConverter.GetBytes(data.Length), 0, 4);
					_client.Stream.Write(compressed, 0, compressed.Length);
				}
				catch (InvalidOperationException e)
				{
					Log.Game.WriteError("An invalid operation occurred while sending data: {0}\nCallstack: {1}", e.Message, e.StackTrace);
				}
				catch (IOException e)
				{
					Log.Game.WriteError("An io exception occurred while sending data: {0}\nCallstack: {1}", e.Message, e.StackTrace);
				}
				catch (SocketException e)
				{
					Log.Game.WriteError("An socket exception occurred while sending data: {0}\nCallstack: {1}", e.Message, e.StackTrace);
				}
				catch (Exception e)
				{
					Log.Game.WriteError("An exception occurred while sending data: {0}\nCallstack: {1}", e.Message, e.StackTrace);
				}

				if (_client == null || _client.TcpClient.Connected == false)
				{
					Log.Core.Write("Network profile client disconnected");
					_client = null;
				}
			}
		}

		private static void OnClientConnected(TcpClient client)
		{
			_client = new ClientConnection { TcpClient = client, Stream = client.GetStream() };
		}
	}
}