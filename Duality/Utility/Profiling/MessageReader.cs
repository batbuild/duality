using System;
using System.Collections.Generic;
using System.Net.Sockets;
using LZ4;

namespace Duality
{
	public class MessageReader
	{
		private readonly NetworkStream _stream;

		public MessageReader(NetworkStream stream)
		{
			_stream = stream;
		}

		public IEnumerable<byte[]> ReadMessages()
		{
			while (true)
			{
				var messageLength = GetMessageLength();
				if (messageLength == 0)
					yield break;

				var decompressedMessageLength = GetMessageLength();
				if (decompressedMessageLength == 0)
					yield break;

				var message = new byte[messageLength];
				var bytesRead = 0;
				while (bytesRead < messageLength)
				{
					var read = _stream.Read(message, bytesRead, message.Length - bytesRead);
					if (read == 0)
						yield break;

					bytesRead += read;
				}

				yield return LZ4Codec.Decode(message, 0, message.Length, decompressedMessageLength);
			}
		}

		private int GetMessageLength()
		{
			var bytesRead = 0;
			var length = new byte[4];
			while (bytesRead < 4)
			{
				var read = _stream.Read(length, bytesRead, 4 - bytesRead);
				if (read == 0)
					return 0;

				bytesRead += read;
			}

			var messageLength = BitConverter.ToInt32(length, 0);
			return messageLength;
		}
	}
}