using System;
using Duality.Drawing;

namespace Duality.Components
{
	public class PassCompleteEventArgs : EventArgs
	{
		public IDrawDevice DrawDevice { get; set; }
		public Camera.Pass Pass { get; set; }
	}
}