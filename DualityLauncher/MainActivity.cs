using System;
using System.IO;
using Android.App;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Content.PM;
using Duality;
using Duality.Resources;
using OpenTK;
using OpenTK.Graphics.ES20;
using Environment = System.Environment;


namespace DualityLauncher.Android
{
	// the ConfigurationChanges flags set here keep the EGL context
	// from being destroyed whenever the device is rotated or the
	// keyboard is shown (highly recommended for all GL apps)
	[Activity(Label = "DualityLauncher.Android",
#if __ANDROID_11__
 HardwareAccelerated = false,
#endif
 ConfigurationChanges = ConfigChanges.Orientation |
			ConfigChanges.KeyboardHidden |
			ConfigChanges.ScreenSize |
			ConfigChanges.Orientation,
		MainLauncher = true,
		Icon = "@drawable/icon")]
	public class MainActivity : Activity
	{
		ProgramAndroid view;

		protected override void OnCreate(Bundle bundle)
		{

			try
			{
				base.OnCreate(bundle);

				// Create our OpenGL view, and display it
				view = new ProgramAndroid(this);
				SetContentView(view);
				view.Run();

				var logfile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "logfile.txt");
				DualityApp.Init(DualityApp.ExecutionEnvironment.Launcher, DualityApp.ExecutionContext.Game, new[] { "logfile", logfile});
				//DualityApp.UserDataChanged += launcherWindow.OnUserDataChanged;


				// Initialize default content
				//launcherWindow.MakeCurrent();

				Log.Core.Write("OpenGL initialized");
				Log.Core.PushIndent();
				Log.Editor.Write("Vendor: {0}", GL.GetString(StringName.Vendor));
				Log.Editor.Write("Version: {0}", GL.GetString(StringName.Version));
				Log.Editor.Write("Renderer: {0}", GL.GetString(StringName.Renderer));
				Log.Editor.Write("Shading language version: {0}", GL.GetString(StringName.ShadingLanguageVersion));
				Log.Core.PopIndent();

				DualityApp.TargetResolution = new Vector2(view.Size.Width, view.Size.Height);
				// DualityApp.TargetMode = view.Context.ApplicationInfo. GraphicsMode;


				/* DEBT: waiting for duality not sure we need to init the default content
 */			
				//ContentProvider.InitDefaultContent();

				// Input setup
				//					DualityApp.Mouse.Source = new GameWindowMouseInputSource(launcherWindow.Mouse, launcherWindow.SetMouseDeviceX, launcherWindow.SetMouseDeviceY);
				//					DualityApp.Keyboard.Source = new GameWindowKeyboardInputSource(launcherWindow.Keyboard);

				// Load the starting Scene
				DualityApp.AppData = new DualityAppData(){StartScene = Scene.Load<Scene>(@"Data\SceneTest.Scene.res")};
				Scene.SwitchTo(DualityApp.AppData.StartScene);

				// Run the DualityApp
				//	launcherWindow.CursorVisible = isDebugging || DualityApp.UserData.SystemCursorVisible;
				//					launcherWindow.VSync = (isProfiling || isDebugging || !DualityApp.UserData.VSync) ? VSyncMode.Off : VSyncMode.On;

				/* DEBT: waiting for android not sure this needs to be here
 */			
				// Shut down the DualityApp
				//DualityApp.Terminate();		

			}
			catch (Exception exception)
			{
				Console.WriteLine("there was a problem {0}", exception.StackTrace);
			}
		}

		

		protected override void OnPause()
		{
			// never forget to do this!
			base.OnPause();
			view.Pause();
		}

		protected override void OnResume()
		{
			// never forget to do this!
			base.OnResume();
			view.Resume();
		}
	}
}


