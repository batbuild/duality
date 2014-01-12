using System;
using System.Windows.Forms;

namespace DualityEditor.Forms
{
	public partial class SaveLayoutDialog : Form
	{
		public SaveLayoutDialog()
		{
			InitializeComponent();
		}

		public string LayoutName { get; set; }

		private void SaveButton_Click(object sender, EventArgs e)
		{
			LayoutName = LayoutNameTextBox.Text;
		}

		private void CancelButton_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
