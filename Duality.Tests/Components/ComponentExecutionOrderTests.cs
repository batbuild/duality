using System.Collections.Generic;
using System.Linq;
using Duality.Resources;
using NUnit.Framework;

namespace Duality.Tests.Components
{
	[TestFixture]
	public class ComponentExecutionOrderTests
	{
		[Test]
		public void CanSetComponentExecutionOrder()
		{
			var updateOrder = new List<int>();
			DualityApp.AppData = new DualityAppData();
			Scene.SetComponentExecutionOrder(typeof (ComponentTwo), typeof(ComponentThree), typeof (ComponentOne));

			var gameObject = new GameObject();
			gameObject.AddComponent(new ComponentThree(updateOrder));
			gameObject.AddComponent(new ComponentOne(updateOrder));
			gameObject.AddComponent(new ComponentTwo(updateOrder));

			Scene.Current.AddObject(gameObject);
			Scene.Current.Update();

			Assert.True(updateOrder.SequenceEqual(new []{2, 3, 1}));

			Scene.Current.RemoveObject(gameObject);
		}

		[Test]
		public void UnspecifiedComponentsRunLast()
		{
			var updateOrder = new List<int>();
			DualityApp.AppData = new DualityAppData();
			Scene.SetComponentExecutionOrder(typeof(ComponentTwo), typeof(ComponentOne));

			var gameObject = new GameObject();
			gameObject.AddComponent(new ComponentOne(updateOrder));
			gameObject.AddComponent(new ComponentFour(updateOrder));
			gameObject.AddComponent(new ComponentThree(updateOrder));
			gameObject.AddComponent(new ComponentTwo(updateOrder));

			Scene.Current.AddObject(gameObject);
			Scene.Current.Update();

			Assert.True(updateOrder.SequenceEqual(new[] { 2, 1, 4, 3 }));
		}

		[Test]
		public void WhenComponentIsRemovedThenExecutionOrderIsUpdated()
		{
			var updateOrder = new List<int>();
			DualityApp.AppData = new DualityAppData();
			Scene.SetComponentExecutionOrder(typeof(ComponentTwo), typeof(ComponentOne), typeof(ComponentThree));

			var gameObject = new GameObject();
			gameObject.AddComponent(new ComponentOne(updateOrder));
			gameObject.AddComponent(new ComponentFour(updateOrder));

			Scene.Current.AddObject(gameObject);

			gameObject.RemoveComponent<ComponentOne>();
			Assert.DoesNotThrow(() => Scene.Current.Update());
		}

		[Test]
		public void WhenComponentIsAddedThenExecutionOrderIsUpdated()
		{
			var updateOrder = new List<int>();
			DualityApp.AppData = new DualityAppData();
			Scene.SetComponentExecutionOrder(typeof(ComponentTwo), typeof(ComponentOne), typeof(ComponentThree));

			var gameObject = new GameObject();
			gameObject.AddComponent(new ComponentOne(updateOrder));
			gameObject.AddComponent(new ComponentFour(updateOrder));

			Scene.Current.AddObject(gameObject);

			gameObject.AddComponent(new ComponentThree(updateOrder));
			
			Scene.Current.Update();

			Assert.True(updateOrder.SequenceEqual(new[] { 1, 3, 4 }));
		}

		[Test]
		public void ComponentsAreDeactivatedInExecutionOrder()
		{
			var updateOrder = new List<int>();
			var deactivationOrder = new List<int>();

			DualityApp.AppData = new DualityAppData();
			Scene.SetComponentExecutionOrder(typeof(ComponentTwo), typeof(ComponentOne), typeof(ComponentThree));

			var gameObject = new GameObject();
			gameObject.AddComponent(new ComponentOne(updateOrder, deactivationOrder));
			gameObject.AddComponent(new ComponentFour(updateOrder, deactivationOrder));

			Scene.Current.AddObject(gameObject);
			gameObject.Active = false;

			Assert.IsTrue(deactivationOrder.SequenceEqual(new[] { 1, 4 }));
		}

		private class BaseComponent : Component, ICmpInitializable, ICmpUpdatable
		{
			protected readonly List<int> _updateOrder;
			protected readonly List<int> _deactivateOrder; 

			protected BaseComponent(List<int> updateOrder, List<int> deactivateOrder = null)
			{
				_updateOrder = updateOrder;
				_deactivateOrder = deactivateOrder;
			}

			public virtual int Id { get; set; }

			public void OnInit(InitContext context)
			{
				
			}

			public void OnShutdown(ShutdownContext context)
			{
				if (_deactivateOrder == null)
					return;

				_deactivateOrder.Add(Id);
			}

			public virtual void OnUpdate()
			{
				if (_updateOrder == null)
					return;

				_updateOrder.Add(Id);
			}
		}

		private class ComponentOne : BaseComponent
		{
			public ComponentOne(List<int> updateOrder, List<int> deactivationOrder = null)
				: base(updateOrder, deactivationOrder)
			{
			}

			public override int Id { get { return 1; }}
		}

		private class ComponentTwo : BaseComponent
		{

			public ComponentTwo(List<int> updateOrder, List<int> deactivationOrder = null)
				: base(updateOrder, deactivationOrder)
			{
			}

			public override int Id { get { return 2; } }
		}

		private class ComponentThree : BaseComponent
		{
			public ComponentThree(List<int> updateOrder, List<int> deactivationOrder = null)
				: base(updateOrder, deactivationOrder)
			{
			}

			public override int Id { get { return 3; } }
		}

		private class ComponentFour : BaseComponent
		{
			public ComponentFour(List<int> updateOrder, List<int> deactivationOrder = null)
				: base(updateOrder, deactivationOrder)
			{
			}

			public override int Id { get { return 4; } }
		}
	}
}
