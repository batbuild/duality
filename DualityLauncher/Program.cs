﻿using System;
using System.Linq;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Drawing;
using Duality;
using Duality.Resources;

using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using OpenTK.Graphics;
using OpenTK.Platform.Windows;

namespace Duality.Launcher
{
	public class DualityLauncher : GameWindow
	{
		private	static bool	isDebugging			= false;
		private	static bool	isProfiling			= false;
		private	static bool	isRunFromEditor		= false;
		private Stopwatch	frameLimiterWatch	= new Stopwatch();

		public DualityLauncher(int w, int h, GraphicsMode mode, string title, GameWindowFlags flags)
			: base(w, h, mode, title, flags)
		{
		}

		protected override void OnResize(EventArgs e)
		{
			DualityApp.TargetResolution = new Vector2(ClientSize.Width, ClientSize.Height);
		}
		protected override void OnUpdateFrame(FrameEventArgs e)
		{
			if (DualityApp.ExecContext == DualityApp.ExecutionContext.Terminated)
			{
				this.Close();
				return;
			}
			
			if (!isDebugging && !isProfiling) // Don't limit frame rate when debugging or profiling.
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
				if (this.frameLimiterWatch.IsRunning && this.VSync == VSyncMode.Off)
				{
					while (this.frameLimiterWatch.Elapsed.TotalMilliseconds < Time.MsPFMult)
					{
						// Enough leftover time? Risk a millisecond sleep.
						if (this.frameLimiterWatch.Elapsed.TotalMilliseconds < Time.MsPFMult * 0.75f)
							System.Threading.Thread.Sleep(1);
					}
				}
				this.frameLimiterWatch.Restart();
			}
			DualityApp.Update();
		}
		protected override void OnRenderFrame(FrameEventArgs e)
		{
			if (DualityApp.ExecContext == DualityApp.ExecutionContext.Terminated) return;

			DualityApp.Render(new Rect(this.ClientSize.Width, this.ClientSize.Height));
			Profile.TimeRender.BeginMeasure();
			Profile.TimeSwapBuffers.BeginMeasure();
			this.SwapBuffers();
			Profile.TimeSwapBuffers.EndMeasure();
			Profile.TimeRender.EndMeasure();
		}

		protected override void OnFocusedChanged(EventArgs e)
		{
			base.OnFocusedChanged(e);

			DualityApp.OnFocusChanged(this.Focused);
		}

		private void SetMouseDeviceX(int x)
		{
			if (!this.Focused) return;
			Point curPos;
			NativeMethods.GetCursorPos(out curPos);
			Point targetPos = this.PointToScreen(new Point(x, this.PointToClient(curPos).Y));
			NativeMethods.SetCursorPos(targetPos.X, targetPos.Y);
			return;
		}
		private void SetMouseDeviceY(int y)
		{
			if (!this.Focused) return;
			Point curPos;
			NativeMethods.GetCursorPos(out curPos);
			Point targetPos = this.PointToScreen(new Point(this.PointToClient(curPos).X, y));
			NativeMethods.SetCursorPos(targetPos.X, targetPos.Y);
			return;
		}

		private void OnUserDataChanged(object sender, EventArgs eventArgs)
		{
			
			switch (DualityApp.UserData.GfxMode)
			{
				case ScreenMode.Window:
				case ScreenMode.FixedWindow:
					this.WindowState = WindowState.Normal;
					this.WindowBorder = WindowBorder.Fixed;
					this.ClientSize = new Size(DualityApp.UserData.GfxWidth, DualityApp.UserData.GfxHeight);
					DisplayDevice.Default.RestoreResolution();
				
					break;

				case ScreenMode.Native:
				case ScreenMode.Fullscreen:
					this.WindowState = WindowState.Fullscreen;
					this.WindowBorder = WindowBorder.Hidden;
					this.ClientSize = new Size(DualityApp.UserData.GfxWidth, DualityApp.UserData.GfxHeight);
					DisplayDevice.Default.ChangeResolution(DualityApp.UserData.GfxWidth, DualityApp.UserData.GfxHeight, DisplayDevice.Default.BitsPerPixel, DisplayDevice.Default.RefreshRate);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			SetVSyncMode(this);
			DualityApp.TargetResolution = new Vector2(ClientSize.Width, ClientSize.Height);
			DualityApp.TargetMode = Context.GraphicsMode;
		}

		[STAThread]
		public static void Main(string[] args)
		{
			System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
			System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.InvariantCulture;

			isDebugging = System.Diagnostics.Debugger.IsAttached || args.Contains(DualityApp.CmdArgDebug);
			isRunFromEditor = args.Contains(DualityApp.CmdArgEditor);
			isProfiling = args.Contains(DualityApp.CmdArgProfiling);
			if (isDebugging || isRunFromEditor) ShowConsole();

			DualityApp.Init(DualityApp.ExecutionEnvironment.Launcher, DualityApp.ExecutionContext.Game, args);
			
			using (DualityLauncher launcherWindow = new DualityLauncher(
				DualityApp.UserData.GfxWidth, 
				DualityApp.UserData.GfxHeight, 
				DualityApp.DefaultMode, 
				DualityApp.AppData.AppName,
				(DualityApp.UserData.GfxMode == ScreenMode.Fullscreen && !isDebugging) ? GameWindowFlags.Fullscreen : GameWindowFlags.Default))
			{
				DualityApp.UserDataChanged += launcherWindow.OnUserDataChanged;

				// Retrieve icon from executable file and set it as window icon
				string executablePath = System.IO.Path.GetFileName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
				launcherWindow.Icon = System.Drawing.Icon.ExtractAssociatedIcon(executablePath);

				// Go into native fullscreen mode
				if (DualityApp.UserData.GfxMode == ScreenMode.Native && !isDebugging)
					launcherWindow.WindowState = WindowState.Fullscreen;

				if (DualityApp.UserData.GfxMode == ScreenMode.FixedWindow)
					launcherWindow.WindowBorder = WindowBorder.Fixed;
				else if (DualityApp.UserData.GfxMode == ScreenMode.Window)
					launcherWindow.WindowBorder = WindowBorder.Resizable;

				// Initialize default content
				launcherWindow.MakeCurrent();

				Log.Core.Write("OpenGL initialized");
				Log.Core.PushIndent();
				Log.Editor.Write("Vendor: {0}", GL.GetString(StringName.Vendor));
				Log.Editor.Write("Version: {0}", GL.GetString(StringName.Version));
				Log.Editor.Write("Renderer: {0}", GL.GetString(StringName.Renderer));
				Log.Editor.Write("Shading language version: {0}", GL.GetString(StringName.ShadingLanguageVersion));
				Log.Core.PopIndent();

				if (ValidateMinimumGPUSpec() == false)
				{
					DualityApp.Terminate();
					DisplayDevice.Default.RestoreResolution();
					return;
				}

				DualityApp.TargetResolution = new Vector2(launcherWindow.ClientSize.Width, launcherWindow.ClientSize.Height);
				DualityApp.TargetMode = launcherWindow.Context.GraphicsMode;
				ContentProvider.InitDefaultContent();

				// Input setup
				DualityApp.Mouse.Source = new GameWindowMouseInputSource(launcherWindow.Mouse, launcherWindow.SetMouseDeviceX, launcherWindow.SetMouseDeviceY);
				DualityApp.Keyboard.Source = new GameWindowKeyboardInputSource(launcherWindow.Keyboard);

				// Load the starting Scene
				Scene.SwitchTo(DualityApp.AppData.StartScene);

				// Run the DualityApp
				launcherWindow.CursorVisible = isDebugging || DualityApp.UserData.SystemCursorVisible;
				SetVSyncMode(launcherWindow);
				launcherWindow.Run();

				// Shut down the DualityApp
				DualityApp.Terminate();
				DisplayDevice.Default.RestoreResolution();
			}
		}

		private static void SetVSyncMode(GameWindow window)
		{
			if (isProfiling || isDebugging)
			{
				window.VSync = VSyncMode.Off;
				return;
			}

			if (DualityApp.UserData.VSync)
			{
				window.VSync = VSyncMode.Adaptive;

				// adaptive not supported?
				if (window.VSync != VSyncMode.Adaptive)
				{
					Log.Game.WriteWarning("Adaptive vsync not supported. Using normal vsync instead");
					window.VSync = VSyncMode.On;
				}
			}
			else
			{
				window.VSync = VSyncMode.Off;
			}
		}

		private static bool ValidateMinimumGPUSpec()
		{
			float shaderLanguageVersion;
			if (float.TryParse(GL.GetString(StringName.ShadingLanguageVersion), out shaderLanguageVersion))
			{
				if (shaderLanguageVersion < DualityApp.AppData.MinimumShaderVersion)
				{
					NativeMethods.MessageBox(IntPtr.Zero, 
						string.Format(
@"Your graphics card is reported as '{0}', which does not meet the minimum specification to run Onikira. 
If you are on a system with both a discreet and integrated GPU, try switching to your discreet or high-performance GPU in your graphics card's control panel and try to run the game again.", GL.GetString(StringName.Renderer)),
						"GPU under minimum spec", 0);
					return false;
				}
			}
			return true;
		}

		private static bool hasConsole = false;
		public static void ShowConsole()
		{
			if (hasConsole) return;
			NativeMethods.AllocConsole();
			hasConsole = true;
		}
	}
}
