#if __ANDROID__
using System;
using System.Diagnostics;
using System.IO;
using Duality.Android.Utility.Zip;
using Duality.Android.Utility.Zip.Android;
using Duality.Utility;
using OS=Android.OS;


namespace Duality.Android
{
#if DEBUG
	public static class DebugContent
	{
		private static Duality.Android.Utility.Zip.IZipArchive _debugDataArchive;

		static DebugContent()
		{
			try
			{
				_debugDataArchive = new AndroidZipArchive(Path.Combine(OS.Environment.ExternalStorageDirectory.Path, "onikira/data.zip"), ZipArchiveMode.Read);
			}
			catch (Exception e)
			{
				Console.WriteLine("Couldn't open debug data. Make sure that the app has read permission for external storage\n" + e.Message);
			}
		}

		public static bool FileExists(string path)
		{
			var asset = _debugDataArchive.GetEntry(FileHelper.NormalizePath(path));
			return asset != null && asset.FullName != null;
		}

		public static Stream Open(string path)
		{
			var asset = _debugDataArchive.GetEntry(path);
			if (asset != null && asset.FullName != null)
				return asset.Open();

			return null;
		}
	}
#endif
}
#endif
