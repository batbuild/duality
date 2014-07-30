using System.Drawing;
using System.IO;
using NUnit.Framework;
using Font = Duality.Resources.Font;

namespace Duality.Tests.Resources
{
	[TestFixture]
	public class FontTests_GetFontFamily
	{
		[Test]
		public void When_not_found_then_result_null()
		{
			var fontFamily = Font.GetFontFamily("Afont");
			Assert.Null(fontFamily);
		}

		[Test]
		public void When_font_found_then_names_match()
		{
			const string arial = "Arial";
			var fontFamily = Font.GetFontFamily(arial);
			Assert.AreEqual(arial, fontFamily.Name);
		}
	}

	[TestFixture]
	public class FontTests_LoadFontFamilyFromMemory
	{
		[Test]
		public void When_adding_font_Then_returns_the_font()
		{
			FontFamily fontFamily;
			using (var stream = new FileStream("Anonymous Pro.ttf", FileMode.Open))
			{
				var buffer = new byte[stream.Length];
				stream.Read(buffer, 0, buffer.Length);

				fontFamily = Font.LoadFontFamilyFromMemory(buffer);
			}

			Assert.NotNull(fontFamily);
			Assert.AreEqual("Anonymous Pro", fontFamily.Name);
		}
	}
	[TestFixture]
	public class FontTests_LoadFontFamilyFromFile
	{
		[Test,Ignore("Found a bug reported to Adam")]
		public void When_font_found_then_names_match()
		{
			const string font = "Anonymous Pro.ttf";
			var fontFamily = Font.LoadFontFamilyFromFile(font);
			Assert.AreEqual(font, fontFamily.Name);
		}
	}
}
