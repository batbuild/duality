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
	public class AndroidZipEntry : IZipEntry
	{
		private readonly ZipFile _zipFile;
		private readonly string _entryName;
		private ZipArchiveMode _mode;
		private readonly ZipEntry _zipEntry;
		private readonly ZipOutputStream _outputStream;

		public AndroidZipEntry(ZipEntry zipEntry, ZipOutputStream stream)
		{
			_mode = ZipArchiveMode.Write;
			_zipEntry = zipEntry;
			_outputStream = stream;
		}

		public AndroidZipEntry(ZipEntry zipEntry)
		{
			_mode = ZipArchiveMode.Read;
			_zipEntry = zipEntry;
		}

		public AndroidZipEntry(ZipFile zipFile, string entryName)
		{
			_mode = ZipArchiveMode.Read;
			_zipFile = zipFile;
			_entryName = entryName;
		}

		public ZipEntry Entry
		{
			get { return _zipEntry; }
		}

		public long Length
		{
			get
			{
				if (_mode == ZipArchiveMode.Read)
				{
					if (_zipEntry == null)
						return _zipFile.GetEntry(_entryName).Size;

					return _zipEntry.Size;
				}

				return 0;
			}
		}

		public string FullName 
		{ 
			get 
			{
				if (_mode == ZipArchiveMode.Read)
				{
					var entry = _zipFile.GetEntry(_entryName);
					if(entry == null)
					{
						Log.Game.WriteWarning("Couldn't find '{0}' in debug content", _entryName);
						return "";
					}

					return entry.Name;
				}

				return _zipEntry.Name; 
			}
		}

		public Stream Open()
		{
			if (_mode == ZipArchiveMode.Write)
			{
				_outputStream.PutNextEntry(_zipEntry);
				return new ZipOutputStreamWriter(_outputStream);
			}

			return _zipFile.GetInputStream(_zipFile.GetEntry(_entryName));
		}
	}
}
#endif