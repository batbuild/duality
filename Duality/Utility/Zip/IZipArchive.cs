using System;
using System.Collections.Generic;

namespace Duality.Android.Utility.Zip
{
	public interface IZipArchive : IDisposable
	{
		IEnumerable<IZipEntry> Entries { get; }

		IZipEntry CreateEntry(string name);
		IZipEntry GetEntry(string name);
	}
}