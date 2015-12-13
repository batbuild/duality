using Duality.Utility;
using NUnit.Framework;
using OpenTK;

namespace Duality.Tests.Utility
{
	[TestFixture]
	public class RectanglePackerTests
	{
		[Test]
		public void TestEvenPacking()
		{
			var atlas = new RectanglePacker(100) {Padding = 0};
			var coordsA = atlas.Pack(50, 50);
			var coordsB = atlas.Pack(50, 50);
			var coordsC = atlas.Pack(50, 50);
			var coordsD = atlas.Pack(50, 50);

			Assert.AreEqual(new Vector2(0, 0), coordsA);
			Assert.AreEqual(new Vector2(50, 0), coordsB);
			Assert.AreEqual(new Vector2(0, 50), coordsC);
			Assert.AreEqual(new Vector2(50, 50), coordsD);
		}

		/// <summary>
		///				100
		///		---------------------
		///		|				|	|
		///		|				|	|
		///		|				|	|
		///		|				|---|
		///	100	|				|	|
		///		|				|	|
		///		|				|	|
		///		|				|	|
		///		---------------------
		/// </summary>
		[Test]
		public void TestUnevenPacking()
		{
			var atlas = new RectanglePacker(100) {Padding = 0};
			var coordsA = atlas.Pack(75, 100);
			var coordsB = atlas.Pack(25, 50);
			var coordsC = atlas.Pack(25, 50);

			Assert.AreEqual(new Vector2(0, 0), coordsA);
			Assert.AreEqual(new Vector2(75, 0), coordsB);
			Assert.AreEqual(new Vector2(75, 50), coordsC);
		}

		[Test]
		public void TestDeepPacking()
		{
			var atlas = new RectanglePacker(100);

			var width = 50;
			var height = 50;
			for (var i = 0; i < 4; i++)
			{
				Assert.AreNotEqual(new Vector2(-1, -1), atlas.Pack(width, height));
				width /= 2;
				height /= 2;
			}
		}

		[Test]
		public void	ReturnsNegativeVectorWhenTheresNoRoomAtTheInn()
		{
			var atlas = new RectanglePacker(100) {Padding = 0};
			atlas.Pack(50, 50);
			atlas.Pack(50, 50);
			atlas.Pack(50, 50);
			atlas.Pack(50, 50);

			Assert.AreEqual(new Vector2(-1, -1), atlas.Pack(1, 1));
		}
	}
}
