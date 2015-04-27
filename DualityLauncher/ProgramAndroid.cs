using System;
using System.Drawing;
using Duality;
using Duality.Resources;
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
		private static bool _isDebugging = false;
		private static bool _isProfiling = false;
		private static bool _isRunFromEditor = false;
		private Stopwatch _frameLimiterWatch = new Stopwatch();
		Random r = new Random();

		public ProgramAndroid (Context context) : base (context)
		{
		}

		// This gets called when the drawing surface is ready
		protected override void OnLoad (EventArgs e)
		{
			base.OnLoad (e);

		}

		private static bool hasConsole = false;
		public static void ShowConsole()
		{
			/* DEBT: Not sure we need this console
			 */			
			if (hasConsole) return;
		//	NativeMethods.AllocConsole();
			hasConsole = true;
		}

		protected override void OnUpdateFrame(FrameEventArgs e)
		{
			base.OnUpdateFrame(e);

			if (!_isDebugging && !_isProfiling) // Don't limit frame rate when debugging or profiling.
			{
				//// Assure we'll at least wait 16 ms until updating again.
				//if (this.frameLimiterWatch.IsRunning)
				//{
				//    while (this.frameLimiterWatch.Elapsed.TotalSeconds < 0.016d) 
				//    {
				//        // Go to sleep if we'd have to wait too long
				//        if (this.frameLimiterWatch.Elapsed.TotalSeconds < 0.01d)
				//            System.Threading.Thread.Sleep(1);
				//    }
				//}

				// Give the processor a rest if we have the time, don't use 100% CPU even without VSync
	
				/* DEBT: waiting for VSync no control for VSync on android
 */			
				if (_frameLimiterWatch.IsRunning) // && this.VSync == VSyncMode.Off)
				{
					while (_frameLimiterWatch.Elapsed.TotalMilliseconds < Time.MsPFMult)
					{
						// Enough leftover time? Risk a millisecond sleep.
						if (_frameLimiterWatch.Elapsed.TotalMilliseconds < Time.MsPFMult * 0.75f)
							System.Threading.Thread.Sleep(1);
					}
				}
				_frameLimiterWatch.Restart();
			}
			DualityApp.Update();
		}

		

		protected override void OnRenderFrame (FrameEventArgs e)
		{
			// you only need to call this if you have delegates
			// registered that you want to have called
			base.OnRenderFrame (e);

			if (DualityApp.ExecContext == DualityApp.ExecutionContext.Terminated) 
				return;
			
			DualityApp.Render(new Rect(this.MinimumWidth, MinimumHeight));
			Profile.TimeRender.BeginMeasure();
			Profile.TimeSwapBuffers.BeginMeasure();
			SwapBuffers();
			Profile.TimeSwapBuffers.EndMeasure();
			Profile.TimeRender.EndMeasure();
			
		}

	}
}

