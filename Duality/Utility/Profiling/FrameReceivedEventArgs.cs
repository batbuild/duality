using System;

namespace Duality
{
	public class FrameReceivedEventArgs : EventArgs
	{
		private readonly FrameData _frameData;

		public FrameReceivedEventArgs(FrameData frameData)
		{
			_frameData = frameData;
		}

		public FrameData FrameData
		{
			get { return _frameData; }
		}
	}
}