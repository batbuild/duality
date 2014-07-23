using System.Linq;
using Duality.Resources;
using NUnit.Framework;

namespace Duality.Tests
{
	[TestFixture]
	public class GameObjectTests
	{
		private GameObject _staticGameObject;

		[SetUp]
		public void Setup()
		{
			_staticGameObject = new GameObject("static");
		}

		[Test]
		public void When_set_Parent_as_self_then_parent_still_null()
		{
			_staticGameObject.Parent = _staticGameObject;

			Assert.IsNull(_staticGameObject.Parent);
		}

		[Test]
		public void Parent_should_have_children_in_Childrens()
		{
			var parent = new GameObject("bla");
			_staticGameObject.Parent = parent;

			Assert.True(parent.Children.Count(x=> x == _staticGameObject) == 1);
		}
	}
}
