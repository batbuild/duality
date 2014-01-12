using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using Duality;

namespace DualityEditor
{
	public class EditorUserData
	{
		public const string		UserDataFile			= "editoruserdata.xml";
		private const string	UserDataDockSeparator	= "<!-- DockPanel Data -->";

		public string EditorData { get; private set; }
		public byte[] LayoutData { get; private set; }

		public void LoadUserData()
		{
			EditorData = "";
			LayoutData = null;

			if (!File.Exists(UserDataFile))
			{
				File.WriteAllText(UserDataFile, EditorRes.GeneralRes.DefaultEditorUserData);
				if (!File.Exists(UserDataFile)) return;
			}

			using (StreamReader reader = new StreamReader(UserDataFile))
			{
				string line;
				// Retrieve pre-DockPanel section
				StringBuilder editorData = new StringBuilder();
				while ((line = reader.ReadLine()) != null && line.Trim() != UserDataDockSeparator)
					editorData.AppendLine(line);
				// Retrieve DockPanel section
				StringBuilder dockPanelData = new StringBuilder();
				while ((line = reader.ReadLine()) != null)
					dockPanelData.AppendLine(line);

				EditorData = editorData.ToString();
				LayoutData = reader.CurrentEncoding.GetBytes(dockPanelData.ToString());
			}
		}

		public void SaveEditorUserData(StreamWriter writer, bool backupsEnabled, AutosaveFrequency autosaveFrequency, string launcherApp, List<EditorPlugin> plugins)
		{
			// --- Save custom user data here ---
			XmlDocument xmlDoc = new XmlDocument();
			XmlElement rootElement = xmlDoc.CreateElement("UserData");
			xmlDoc.AppendChild(rootElement);
			XmlElement editorAppElement = xmlDoc.CreateElement("EditorApp");
			rootElement.AppendChild(editorAppElement);
			editorAppElement.SetAttribute("backups", backupsEnabled.ToString(System.Globalization.CultureInfo.InvariantCulture));
			editorAppElement.SetAttribute("autosaves", autosaveFrequency.ToString());
			editorAppElement.SetAttribute("launcher", launcherApp);
			foreach (EditorPlugin plugin in plugins)
			{
				XmlElement pluginXmlElement = xmlDoc.CreateElement("Plugin_" + plugin.Id);
				rootElement.AppendChild(pluginXmlElement);
				plugin.SaveUserData(pluginXmlElement);
			}
			xmlDoc.Save(writer.BaseStream);
			// ----------------------------------
			writer.WriteLine();
			writer.WriteLine(UserDataDockSeparator);
			writer.Flush();
		}
	}
}