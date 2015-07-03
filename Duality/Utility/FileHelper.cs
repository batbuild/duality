﻿using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Duality.Utility
{
	public static class FileHelper
	{
		static FileHelper ()
		{
#if __ANDROID__
			if (filesInDataDir == null)
			{
				filesInDataDir = new HashSet<string>();
				Log.Core.Write("Started building file cache...");
				var sw = Stopwatch.StartNew();
				BuildFileCache("");
				sw.Stop();
				Log.Core.Write("Finished. Took " + sw.Elapsed + "ms");
			}
#endif
		}

		public static bool FileExists(string path)
		{
#if !__ANDROID__
			return File.Exists(path);
#else
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
		
		public static IEnumerable<string> EnumerateFiles(string pluginDirectory, string searchPattern)
		{
			
			return filesInDataDir.Where(file => AssertPatterMatchesTarget(file, searchPattern)==SearchResults.Found);
		}
#endif

		public static SearchResults AssertPatterMatchesTarget(string target, string searchPattern)
		{
			   char[] trimEndChars =
			   {
				   '\t',
				   '\n',
				   '\v',
				   '\f',
				   '\r',
				   ' ',
				   '\x0085',
				   ' '
			   };
			if (string.IsNullOrWhiteSpace(searchPattern))
				return SearchResults.Undefined;

			var strings = Regex.Split(searchPattern.Trim(trimEndChars), @"(\*)|(\?)");
			if (strings[0] == string.Empty)
			{
				var s = new List<string>(strings);
				s.RemoveAt(0);
				strings = s.ToArray();
			}
				
			var patternCount = strings.Count(x => x == "*" || x == "?");
			switch (patternCount)
			{
				case 0:
					return target.Contains(searchPattern) ? SearchResults.Found : SearchResults.NotFound;
				case 1:
					return OnePattern(target, strings, searchPattern);
				default:
					Log.Core.WriteError("Not implemented error, just one * or ? allowed in the pattern");
					return SearchResults.Undefined;
			}
		}

		private static SearchResults OnePattern(string target, string[] strings, string searchPattern)
		{
			var len = strings.Length - 1;
			
			// since only one pattern allowed and 
			if ((strings[len] == "?" || strings[len] == "*") && target.StartsWith(strings[0])) 
				return SearchResults.Found;
			if ((strings[0] == "?" || strings[0] == "*") && target.EndsWith(strings[1])) 
				return SearchResults.Found;
			if (strings[1]== "*")
				return target.StartsWith(strings[0]) && target.EndsWith(strings[2]) ? SearchResults.Found : SearchResults.NotFound;
			if (strings[1]== "?")
			{
				if (target.StartsWith(strings[0]) && target.EndsWith(strings[2]) && (target.Length == searchPattern.Length))
					return SearchResults.Found;
				return SearchResults.NotFound;
			}
			return SearchResults.NotFound;
		}
	}

	public enum SearchResults
	{
		Undefined,
		Found,
		NotFound
	}
}