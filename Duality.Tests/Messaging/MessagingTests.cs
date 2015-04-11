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
		public void CanBroadcastFromGameObjects()
		{
			var gameObject = new GameObject();
			var receiver = new TestComponent();
			gameObject.AddComponent(receiver);
			Scene.Current.AddObject(gameObject);

			gameObject.SendMessage(new TestGameMessage(), gameObject);

			Assert.IsTrue(receiver.MessageHandled);
		}

		[Test]
		public void WhenMessageMarkedAsHandledMessageNotSentToFurtherGameObjects()
		{
			var firstObject = new GameObject();
			var firstReceiver = new HandledTestComponent();
			firstObject.AddComponent(firstReceiver);
			Scene.Current.AddObject(firstObject);
			
			var secondObject = new GameObject();
			var secondReceiver = new HandledTestComponent();
			secondObject.AddComponent(secondReceiver);
			Scene.Current.AddObject(secondObject);

			firstObject.SendMessage(new TestGameMessage(), null);

			Assert.IsTrue(firstReceiver.MessageHandled ^ secondReceiver.MessageHandled);
		}

		private static Component RegisterInactiveObject()
		{
			var gameObject = new GameObject();
			var component = gameObject.AddComponent<TestComponent>();
			Scene.Current.AddObject(gameObject);
			gameObject.Active = false;
			return component;
		}

		private class TestComponent : Component, ICmpHandlesMessages
		{
			public bool MessageHandled { get; set; }

			public void HandleMessage(GameObject sender, GameMessage msg)
			{
				MessageHandled = true;
			}

			public void TestBroadcastMessage()
			{
				this.SendMessage(new TestGameMessage());
			}

			public void TestBroadcastMessageToNamedGameObject()
			{
				this.SendMessage(new TestGameMessage(), "TestGameObject");
			}
		}

		private class HandledTestComponent : Component, ICmpHandlesMessages
		{
			public bool MessageHandled { get; set; }

			public void HandleMessage(GameObject sender, GameMessage msg)
			{
				MessageHandled = true;
				msg.Handled = true;
			}
		}

		private class TestGameMessage : GameMessage
		{
			
		}
	}
}
