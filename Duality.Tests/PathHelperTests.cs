using System;
using System.Diagnostics;
using System.IO;
using NUnit.Framework;

namespace Duality.Tests
{
	[TestFixture]
	public class PathHelperTests
	{
		[Test]
		public void GetFileHashReturnsSameHash()
		{
			var buffer = new Byte[10240000];
			MathF.Rnd.NextBytes(buffer);
			try
			{
				File.WriteAllText("Test.txt", BitConverter.ToString(buffer));
				
				Assert.AreEqual(PathHelper.GetFileHash("test.txt"), PathHelper.GetFileHash("test.txt"));
			}
			finally
			{
				if(File.Exists("test.txt"))
					File.Delete("test.txt");
			}
		}
	}
}
