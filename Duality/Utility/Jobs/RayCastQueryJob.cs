using Duality.Components.Physics;
using Duality.Resources;
using FarseerPhysics.Dynamics;
using OpenTK;

namespace Duality.Utility.Jobs
{
	internal class RayCastQueryJob : Job<RayCastQueryJob>
	{
		public RayCastQuery Query { get; set; }
		
		public override void DoWork()
		{
			var callback = Query.Callback;
			if (Query.Callback == null) callback = Raycast_DefaultCallback;

			var fsWorldCoordA = PhysicsConvert.ToPhysicalUnit(Query.WorldCoordA);
			var fsWorldCoordB = PhysicsConvert.ToPhysicalUnit(Query.WorldCoordB);

			Scene.PhysicsWorld.RayCast(delegate(Fixture fixture, Vector2 pos, Vector2 normal, float fraction)
			{
				var data = new RayCastData(
					fixture.UserData as ShapeInfo,
					PhysicsConvert.ToDualityUnit(pos),
					normal,
					fraction);
				var result = callback(data);
				if (result >= 0.0f) Query.HitData.Add(data);
				return result;
			}, fsWorldCoordA, fsWorldCoordB);

			Query.HitData.StableSort((d1, d2) => (int)(1000000.0f * (d1.Fraction - d2.Fraction)));
		}

		private static float Raycast_DefaultCallback(RayCastData data)
		{
			return 1.0f;
		}
	}
}