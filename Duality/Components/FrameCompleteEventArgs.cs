using System;
using Duality.Drawing;

namespace Duality.Components
{
	public class FrameCompleteEventArgs : EventArgs
	{
		public IDrawDevice DrawDevice { get; set; }
	}
}