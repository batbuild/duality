using Duality.Helpers;
using Duality.Resources;
using NUnit.Framework;

namespace Duality.Tests.Messaging
{
	[TestFixture]
	public class MessagingTests
	{
		[Test]
		public void CanBroadcastMessagesToGameObjects()
		{
			var gameObject = new GameObject();
			var receiver = new TestComponent();
			gameObject.AddComponent(receiver);
			Scene.Current.AddObject(gameObject);

			receiver.TestBroadcastMessage();

			Assert.IsTrue(receiver.MessageHandled);
		}

		[Test]
		public void CanBroadcastToNamedGameObject()
		{
			var gameObject = new GameObject {Name = "TestGameObject"};
			var receiver = new TestComponent();
			gameObject.AddComponent(receiver);
			Scene.Current.AddObject(gameObject);

			var gameObject2 = new GameObject();
			var receiver2 = new TestComponent();
			gameObject2.AddComponent(receiver2);
			Scene.Current.AddObject(gameObject2);

			receiver2.TestBroadcastMessageToNamedGameObject();

			Assert.IsTrue(receiver.MessageHandled);
			Assert.IsFalse(receiver2.MessageHandled);
		}

		[Test]
		public void BroadcastsToAllRecieversInNamedObject()
		{
			var gameObject = new GameObject { Name = "TestGameObject" };
			var receiver = new TestComponent();
			gameObject.AddComponent(receiver);
			var receiver2 = new SecondaryComponent();
			gameObject.AddComponent(receiver2);
			Scene.Current.AddObject(gameObject);

			var gameObject2 = new GameObject();
			var receiver3 = new TestComponent();
			gameObject2.AddComponent(receiver3);
			Scene.Current.AddObject(gameObject2);

			receiver3.TestBroadcastMessageToNamedGameObject();

			Assert.IsTrue(receiver.MessageHandled);
			Assert.IsTrue(receiver2.MessageHandled);
		}

		[Test]
		public void OnlyBroadcastsToActiveGameObjects()
		{
			var listener = (TestComponent)RegisterInactiveObject();

			listener.TestBroadcastMessage();

			Assert.IsFalse(listener.MessageHandled);
		}

		[Test]
		public void DoesNotBroadcastToInactiveNamedObjects()
		{
			var listener = (TestComponent) RegisterInactiveObject();
			listener.GameObj.Name = "TestGameObject";

			listener.TestBroadcastMessageToNamedGameObject();

			Assert.IsFalse(listener.MessageHandled);
		}

		[Test]
		public void SpawningNewObjectsInMessageHandlersDoesNotCauseAnException()
		{
			var gameObject = new GameObject { Name = "TestGameObject" };
			var receiver = new SpawnerComponent();
			gameObject.AddComponent(receiver);
			Scene.Current.AddObject(gameObject);

			var gameObject2 = new GameObject() { Name = "TestGameObject" };
			var receiver2 = new SpawnerComponent();
			gameObject2.AddComponent(receiver2);
			Scene.Current.AddObject(gameObject2);

			Assert.DoesNotThrow(receiver2.TestBroadcastMessageToNamedGameObject);			
		}

		[Test]
		public void CanBroadcastToGameObject()
		{
			var gameObject = new GameObject();
			var receiver = new TestComponent();
			gameObject.AddComponent(receiver);
			Scene.Current.AddObject(gameObject);

			var gameObject2 = new GameObject();
			var receiver2 = new TestComponent();
			gameObject2.AddComponent(receiver2);
			Scene.Current.AddObject(gameObject2);

			receiver2.TestBroadcastMessageToGameObject(gameObject);

			Assert.IsTrue(receiver.MessageHandled);
			Assert.IsFalse(receiver2.MessageHandled);
		}

		[Test]
		public void BroadcastsToAllRecieversInGameObject()
		{
			var gameObject = new GameObject();
			var receiver = new TestComponent();
			gameObject.AddComponent(receiver);
			var receiver2 = new SecondaryComponent();
			gameObject.AddComponent(receiver2);
			Scene.Current.AddObject(gameObject);

			var gameObject2 = new GameObject();
			var receiver3 = new TestComponent();
			gameObject2.AddComponent(receiver3);
			Scene.Current.AddObject(gameObject2);

			receiver3.TestBroadcastMessageToGameObject(gameObject);

			Assert.IsTrue(receiver.MessageHandled);
			Assert.IsTrue(receiver2.MessageHandled);
		}

		[Test]
		public void DoesNotBroadcastToInactiveGameObject()
		{
			var listener = (TestComponent)RegisterInactiveObject();

			listener.TestBroadcastMessage();

			Assert.IsFalse(listener.MessageHandled);
		}

		private static Component RegisterInactiveObject()
		{
			var gameObject = new GameObject {Active = false};
			var component = gameObject.AddComponent<TestComponent>();
			Scene.Current.AddObject(gameObject);
			return component;
		}

		private class TestComponent : Component, ICmpHandlesMessages
		{
			public bool MessageHandled { get; set; }

			public void HandleMessage(Component sender, GameMessage msg)
			{
				MessageHandled = true;
			}

			public void TestBroadcastMessage()
			{
				this.BroadcastMessage(new TestGameMessage());
			}

			public void TestBroadcastMessageToNamedGameObject()
			{
				this.BroadcastMessage(new TestGameMessage(), "TestGameObject");
			}

			public void TestBroadcastMessageToGameObject(GameObject target)
			{
				this.BroadcastMessage(new TestGameMessage(), target);
			}
		}

		private class SecondaryComponent : Component, ICmpHandlesMessages
		{
			public bool MessageHandled { get; set; }

			public void HandleMessage(Component sender, GameMessage msg)
			{
				MessageHandled = true;
			}
		}

		private class SpawnerComponent : Component, ICmpHandlesMessages
		{
			public void TestBroadcastMessageToNamedGameObject()
			{
				this.BroadcastMessage(new TestGameMessage(), "TestGameObject");
			}

			public void HandleMessage(Component sender, GameMessage msg)
			{
				var gameObject = new GameObject() { Name = "SpawnedTestGameObject" };
				Scene.Current.AddObject(gameObject);

				var gameObject2 = new GameObject() { Name = "TestSpawnedGameObject" };
				Scene.Current.AddObject(gameObject2);
			}
		}

		private class TestGameMessage : GameMessage
		{
			
		}
	}
}
