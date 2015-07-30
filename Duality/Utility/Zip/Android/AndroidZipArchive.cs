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
	public class AndroidZipArchive : IZipArchive
	{
		private readonly string _fileName;
		private readonly ZipArchiveMode _mode;
		private ZipOutputStream _zipOutputStream;
		private ZipFile _zipFile;
		private FileStream _stream;
		private AndroidZipEntry _previousZipEntry;

		public AndroidZipArchive(string fileName, ZipArchiveMode mode)
		{
			_fileName = fileName;
			_mode = mode;
			if (mode == ZipArchiveMode.Write)
			{
				_stream = new FileStream(fileName, FileMode.Create);
				_zipOutputStream = new ZipOutputStream(_stream);
			}
		}

		public IEnumerable<IZipEntry> Entries
		{
			get
			{
				return IterateEntries();
			}
		}

		public IZipEntry CreateEntry(string key)
		{
			if (_zipOutputStream == null)
				throw new InvalidOperationException("Cannot create entry because the output stream is null");

			if (_previousZipEntry != null)
				_zipOutputStream.CloseEntry();

			_previousZipEntry = new AndroidZipEntry(new ZipEntry(key), _zipOutputStream);
			return _previousZipEntry;
		}

		public IZipEntry GetEntry(string name)
		{
			if (_zipFile == null)
				_zipFile = new ZipFile(new Java.IO.File(_fileName));

			if (_zipFile == null)
				throw new InvalidOperationException("Can't retrieve a zip entry because the input stream has not been opened");

			var zipEntry = new AndroidZipEntry(_zipFile, name);
			return zipEntry;
		}

		public void Dispose()
		{
			if (_mode == ZipArchiveMode.Read)
			{
				if (_zipFile != null)
				{
					_zipFile.Dispose();
					_zipFile = null;
				}
			}
			else
			{
				if (_zipOutputStream != null)
				{
					_zipOutputStream.Finish();
					_zipOutputStream.Dispose();
					_zipOutputStream = null;
				}
			}

			if (_stream != null)
			{
				_stream.Flush();
				_stream.Dispose();
				_stream = null;
			}
		}

		private IEnumerable<IZipEntry> IterateEntries()
		{
			using (var fileStream = new FileStream(_fileName, FileMode.Open))
			{
				var inputStream = new ZipInputStream(fileStream);

				try
				{
					ZipEntry entry;
					while ((entry = inputStream.NextEntry) != null)
						yield return new AndroidZipEntry(entry);
				}
				finally
				{
					inputStream.Close();
				}

			}
		}
	}
}
#endif