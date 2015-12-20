#if __ANDROID__
using System;
using System.IO;
using Duality.Android.Utility.Zip;
using Duality.Android.Utility.Zip.Android;
using Duality.Utility;
using OS=Android.OS;


namespace Duality.Android
{
	public class ApkExpansionContentProvider
	{
		private static ApkExpansionContentProvider _instance;

		private IZipArchive _debugDataArchive;

		private static void Initialize()
		{
			if (_instance != null)
				return;

			_instance = new ApkExpansionContentProvider();
			try
			{
				// TODO: Find a way to deal with the version number of the expansion apk. 1 is unlikely to work for everything;)
				var obbFilePath = Path.Combine(OS.Environment.ExternalStorageDirectory.Path, 
					string.Format("Android/obb/{0}", DualityApp.AppData.PackageName), 
					string.Format("main.1.{0}.obb", DualityApp.AppData.PackageName));

				Log.Game.Write("Looking for expansion APK at " + obbFilePath);
				_instance._debugDataArchive = new AndroidZipArchive(obbFilePath, ZipArchiveMode.Read);
			}
			catch (Exception e)
			{
				Console.WriteLine("Couldn't open expansion APK. Make sure that the app has read permission for external storage\n" + e.Message);
			}
		}

		public static bool FileExists(string path)
		{
			if(_instance == null)
				Initialize();

			var asset = _instance._debugDataArchive.GetEntry(FileHelper.NormalizePath(path));
			return asset != null && asset.FullName != null;
		}

		public static Stream Open(string path)
		{
			if(_instance == null)
				Initialize();

			var asset = _instance._debugDataArchive.GetEntry(path);
			if (asset != null && asset.FullName != null)
				return asset.Open();

			return null;
		}
	}
}
#endif
