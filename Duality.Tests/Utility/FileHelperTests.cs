using Duality.Utility;
using NUnit.Framework;

namespace Duality.Tests.Utility
{
	[TestFixture]

	public class FileHelperTests
	{

		[TestCase("*.core.dll",SearchResults.Found)]
		[TestCase("?uncho.core.dll", SearchResults.Found)]
		[TestCase("muncho*", SearchResults.Found)]
		[TestCase("muncho.core.dl?", SearchResults.Found)]
		[TestCase("mun*.core.dll", SearchResults.Found)]
		[TestCase("mun*.cor?.dll", SearchResults.Undefined)]
		[TestCase("?mun*.cor.dll", SearchResults.Undefined)]
		[TestCase("mun*.cor.dl?", SearchResults.Undefined)]
		[TestCase("muncho.core.dll", SearchResults.Found)]
		[TestCase("blka", SearchResults.NotFound)]
		[TestCase("", SearchResults.Undefined)]
		public void pattern_works(string pattern, SearchResults success)
		{
			Assert.AreEqual(success, FileHelper.AssertPatterMatchesTarget(pattern, "muncho.core.dll"), pattern);
		}
	}
}
