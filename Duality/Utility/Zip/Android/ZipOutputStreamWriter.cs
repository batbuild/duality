#if __ANDROID__
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Java.Util.Zip;

namespace Duality.Android.Utility.Zip.Android
{
	public class ZipOutputStreamWriter : Stream
	{
		private readonly ZipOutputStream _stream;

		public ZipOutputStreamWriter(ZipOutputStream stream)
		{
			_stream = stream;
		}

		public override void Flush()
		{
			throw new NotImplementedException();
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotImplementedException();
		}

		public override void SetLength(long value)
		{
			throw new NotImplementedException();
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			throw new NotImplementedException();
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			_stream.Write(buffer, offset, count);
		}

		public override bool CanRead
		{
			get { return false; }
		}

		public override bool CanSeek
		{
			get { return false; }
		}

		public override bool CanWrite
		{
			get { return _stream != null; }
		}

		public override long Length
		{
			get { throw new NotImplementedException(); }
		}

		public override long Position { get; set; }
	}
}
#endif