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

		[Test]
		public void ActiveSingleTree()
		{
			var scene = new Scene();
			Scene.SwitchTo(scene);

			var parent = new GameObject() {ActiveSingle = false, ParentScene = scene};
			var child = new GameObject() {Parent = parent, ActiveSingle = false};
			var grandChild = new GameObject() {Parent = child, ActiveSingle = false };

			child.AddComponent<TrackActivationComponent>();
			grandChild.AddComponent<TrackActivationComponent>();

			parent.ActiveSingleTree = true;

			Assert.IsFalse(child.GetComponent<TrackActivationComponent>().WasInitialized);
			Assert.IsFalse(grandChild.GetComponent<TrackActivationComponent>().WasInitialized);
		}

		public class TrackActivationComponent : Component, ICmpInitializable
		{
			public bool WasInitialized { get; set; }

			public void OnInit(InitContext context)
			{
				if(context == InitContext.Activate)
					WasInitialized = true;
			}

			public void OnShutdown(ShutdownContext context)
			{
				
			}
		}
	}
}
