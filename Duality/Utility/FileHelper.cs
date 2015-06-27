using System;
using System.IO;
using System.Collections.Generic;

namespace Duality.Utility
{
	public static class FileHelper
	{
		public static bool FileExists(string path)
		{
#if !__ANDROID__
			return File.Exists(path);
#else
			if (filesInDataDir == null)
			{
				filesInDataDir = new HashSet<string>();
				BuildFileCache("");
			}
			
			return filesInDataDir.Contains(NormalizePath(path));
#endif
		}

		public static Stream OpenRead(string path)
		{
#if !__ANDROID__
			return File.OpenRead(path);
#else
			return ContentProvider.AndroidAssetManager.Open(NormalizePath(path));
#endif
		}

#if __ANDROID__

		private static HashSet<string> filesInDataDir;

		private static string NormalizePath(string path)
		{
			return path.Replace('\\', '/');
		}

		private static bool BuildFileCache(string path)
		{
			var list = ContentProvider.AndroidAssetManager.List(path);
			if(list.Length > 0)
			{
				foreach (var dir in list)
				{
					if (!BuildFileCache(string.IsNullOrEmpty(path) ? dir : path + "/" + dir))
						return false;
				}
			}
			else
			{
				filesInDataDir.Add(path);
			}

			return true;
		}
#endif

	}
}
