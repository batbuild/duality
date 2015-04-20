using System;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.ES20;
using OpenTK.Platform;

using Android.Views;
using Android.Content;
using Android.Util;

namespace DualityLauncher.Android
{
	class ProgramAndroid : OpenTK.Android.AndroidGameView
	{
		public ProgramAndroid (Context context) : base (context)
		{
		}

		// This gets called when the drawing surface is ready
		protected override void OnLoad (EventArgs e)
		{
			base.OnLoad (e);

			// Run the render loop
			Run ();
		}


		Random r = new Random ();
		// This gets called on each frame render
		protected override void OnRenderFrame (FrameEventArgs e)
		{
			// you only need to call this if you have delegates
			// registered that you want to have called
			base.OnRenderFrame (e);

			GL.ClearColor ((float)r.NextDouble (), 0f, 255f, 0f);
			GL.Clear (ClearBufferMask.ColorBufferBit);

			SwapBuffers ();
		}

	}
}

