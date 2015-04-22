using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.ES20;
using OpenTK.Platform;
using OpenTK.Android;

using Android.Views;
using Android.Content;
using Android.Util;
using System.Diagnostics;

namespace DualityLauncher.Android
{
	public class ProgramAndroid : AndroidGameView
	{
		public ProgramAndroid (Context context) : base (context)
		{
		}

		// This gets called when the drawing surface is ready
		protected override void OnLoad (EventArgs e)
		{
			base.OnLoad (e);

			// Run the render loop
		
		}


		protected override void OnUpdateFrame(FrameEventArgs e)
		{
			base.OnUpdateFrame(e);
		}

		Random r = new Random ();
		// This gets called on each frame render
		protected override void OnRenderFrame (FrameEventArgs e)
		{
			// you only need to call this if you have delegates
			// registered that you want to have called
			base.OnRenderFrame (e);

			GL.ClearColor ((float)r.NextDouble (), 0f, 0f, 0f);
			GL.Clear (ClearBufferMask.ColorBufferBit);

			SwapBuffers ();
		}

	}
}

