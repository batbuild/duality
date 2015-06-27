using System;
using System.IO;
using Duality;
using Duality.Resources;
using OpenTK;
using OpenTK.Android;
using Android.Content;
using System.Diagnostics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Graphics;
using Size=System.Drawing.Size;


namespace DualityLauncher.Android
{
	public class DualityAndroidLauncher : AndroidGameView
	{
		private static bool _isDebugging = false;
		private static bool _isProfiling = false;
		private Stopwatch _frameLimiterWatch = new Stopwatch();
		

		public DualityAndroidLauncher (Context context) : base (context)
		{
			Duality.ContentProvider.SetAndroidAssetManager(context.Assets);
		}

		// This gets called when the drawing surface is ready
		protected override void OnLoad (EventArgs e)
		{
			base.OnLoad (e);

			var logfile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "logfile.txt");
			DualityApp.Init(DualityApp.ExecutionEnvironment.Launcher, DualityApp.ExecutionContext.Game, new[] { "logfile", logfile });
			
			
			// Initialize default content
			//launcherWindow.MakeCurrent();
			MakeCurrent();

			Log.Core.Write("OpenGL initialized");
			Log.Core.PushIndent();
			Log.Editor.Write("Vendor: {0}", GL.GetString(StringName.Vendor));
			Log.Editor.Write("Version: {0}", GL.GetString(StringName.Version));
			Log.Editor.Write("Renderer: {0}", GL.GetString(StringName.Renderer));
			Log.Editor.Write("Shading language version: {0}", GL.GetString(StringName.ShadingLanguageVersion));
			Log.Core.PopIndent();

			DualityApp.TargetMode = GraphicsContext.CurrentContext.GraphicsMode; 

			Duality.ContentProvider.InitDefaultContent();

			Scene.SwitchTo(DualityApp.AppData.StartScene);
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

			try {
				DualityApp.Render(new Rect(this.Size.Width, this.Size.Height));
			} catch (Exception ex) {
				Log.Game.WriteError (ex.Message);
			}
			Profile.TimeRender.BeginMeasure();
			Profile.TimeSwapBuffers.BeginMeasure();
			SwapBuffers();
			Profile.TimeSwapBuffers.EndMeasure();
			Profile.TimeRender.EndMeasure();
		}
	}
}

