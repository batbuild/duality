using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using WeifenLuo.WinFormsUI.Docking;

namespace Duality.Editor
{
	public class Workspace
	{
		private const string					LayoutsFolder	= "Layouts";
		private DockPanel						mainDockPanel	= null;
		private List<EditorPlugin>				plugins			= null;

		public IEnumerable<string> GetLayoutNames()
		{
			CreateLayoutsFolderIfItDoesntExist();

			return Directory.EnumerateFiles(Path.Combine(GetLayoutsFolder()))
				.Select(Path.GetFileNameWithoutExtension)
				.OrderBy(s => s);
		}

		public void SetDockPanel(DockPanel dockPanel)
		{
			this.mainDockPanel = dockPanel;
		}

		public void SetPlugins(List<EditorPlugin> plugins)
		{
			this.plugins = plugins;
		}

		public void SaveLayout(string layoutName)
		{
			var dir = Path.Combine(Path.GetDirectoryName(DualityApp.UserDataPath), LayoutsFolder);

			if (Directory.Exists(dir) == false)
				Directory.CreateDirectory(dir);

			using(FileStream stream = new FileStream(Path.Combine(dir, layoutName + ".xml"), FileMode.Create))
				SaveLayout(stream, Encoding.UTF8);
		}

		public void LoadLayout(string layoutName)
		{
			var layoutDir = Path.Combine(Path.GetDirectoryName(DualityApp.UserDataPath), LayoutsFolder);
			LoadLayout(File.ReadAllBytes(Path.Combine(layoutDir, layoutName + ".xml")));
		}

		public void SaveLayout(FileStream stream, Encoding encoding)
		{
			this.mainDockPanel.SaveAsXml(stream, encoding);
		}

		public void LoadLayout(byte[] layoutData)
		{
			using (MemoryStream dockPanelDataStream = new MemoryStream(layoutData))
			{
				try
				{
					CloseOpenDockWindows();
					this.mainDockPanel.LoadFromXml(dockPanelDataStream, DeserializeDockContent);
				}
				catch (XmlException e)
				{
					Log.Editor.WriteError("Cannot load DockPanel data due to malformed or non-existent Xml: {0}", Log.Exception(e));
				}
			}
		}

		private void CloseOpenDockWindows()
		{
			for (int i = this.mainDockPanel.Contents.Count - 1; i >= 0; i--)
			{
				this.mainDockPanel.Contents[i].DockHandler.Pane.CloseActiveContent();
			}
		}

		private IDockContent DeserializeDockContent(string persistName)
		{
			Log.Editor.Write("Deserializing layout: '" + persistName + "'");

			Type dockContentType = null;
			Assembly dockContentAssembly = null;
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (Assembly a in assemblies)
			{
				if ((dockContentType = a.GetType(persistName)) != null)
				{
					dockContentAssembly = a;
					break;
				}
			}

			if (dockContentType == null)
				return null;
			
			// First ask plugins from the dock contents assembly for existing instances
			IDockContent deserializeDockContent = null;
			foreach (EditorPlugin plugin in plugins)
			{
				if (plugin.GetType().Assembly == dockContentAssembly)
				{
					deserializeDockContent = plugin.DeserializeDockContent(dockContentType);
					if (deserializeDockContent != null) break;
				}
			}

			// If none exists, create one
			return deserializeDockContent ?? (dockContentType.CreateInstanceOf() as IDockContent);
		}

		private static void CreateLayoutsFolderIfItDoesntExist()
		{
			if (Directory.Exists(GetLayoutsFolder())) 
				return;

			Log.Core.Write("Creating layouts folder in {0}", GetLayoutsFolder());
			Directory.CreateDirectory(GetLayoutsFolder());
		}

		private static string GetLayoutsFolder()
		{
			return Path.Combine(Path.GetDirectoryName(DualityApp.UserDataPath), LayoutsFolder);
		}
	}
}