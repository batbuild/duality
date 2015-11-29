using System.Collections.Generic;
using Duality.Components.Physics;
using OpenTK;

namespace Duality.Utility.Jobs
{
	internal struct RayCastQuery
	{
		public Vector2 WorldCoordA { get; private set; }
		public Vector2 WorldCoordB { get; private set; }
		public List<RayCastData> HitData { get; private set; }
		public RayCastCallback Callback { get; private set; }

		public RayCastQuery(Vector2 worldCoordA, Vector2 worldCoordB, List<RayCastData> hitData, RayCastCallback callback)
			: this()
		{
			WorldCoordA = worldCoordA;
			WorldCoordB = worldCoordB;
			HitData = hitData;
			Callback = callback;
		}
	}
}