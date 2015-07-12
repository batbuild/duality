﻿using System;
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
using OpenTK.Graphics.OpenGL;
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
		DualityAndroidLauncher view;

		protected override void OnCreate(Bundle bundle)
		{

			try
			{
				base.OnCreate(bundle);

				RequestWindowFeature(WindowFeatures.NoTitle);
				Window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);

				// Create our OpenGL view, and display it
				view = new DualityAndroidLauncher(this);
				SetContentView(view);
				view.Run();
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


