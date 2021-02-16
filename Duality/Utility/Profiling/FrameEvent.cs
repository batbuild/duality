namespace Duality
{
	public struct FrameEvent
	{
		public float FrameTime;
		public string CounterName;

		public FrameEvent(string counterName, long elapsedTicks)
		{
			FrameTime = elapsedTicks;
			CounterName = counterName;
		}
	}
}