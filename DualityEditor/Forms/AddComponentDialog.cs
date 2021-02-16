using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Aga.Controls.Tree;

namespace Duality.Editor.Forms
{
	public partial class AddComponentDialog : Form
	{
		private readonly Dictionary<Node, bool> tempNodeVisibilityCache = new Dictionary<Node, bool>();
		private TreeModel model = new TreeModel();
		private string tempUpperFilter;

		public AddComponentDialog()
		{
			InitializeComponent();

			this.objectTypeView.Model = this.model;
		}

		public Type SelectedType { get; set; }

		public void SetTreeViewItems(IEnumerable<Node> items)
		{
			foreach (var item in items)
			{
				this.model.Nodes.Add(item);
			}
		}

		private void FilterTextBox_TextChanged(object sender, EventArgs e)
		{
			ApplyNodeFilter();
		}

		private void ApplyNodeFilter()
		{
			tempUpperFilter = String.IsNullOrEmpty(FilterTextBox.Text) ? null : FilterTextBox.Text.ToUpper();
			tempNodeVisibilityCache.Clear();
			this.objectTypeView.NodeFilter = tempUpperFilter != null ? IsNodeVisible : (Predicate<object>)null;
		}

		private bool IsNodeVisible(object obj)
		{
			if (tempUpperFilter == null)
				return true;

			var vn = obj as TreeNodeAdv;
			var n = vn != null ? vn.Tag as Node : obj as Node;
			if (n == null)
				return true;

			bool isVisible;
			if (!tempNodeVisibilityCache.TryGetValue(n, out isVisible))
			{
				isVisible = n.Text.ToUpper().Contains(tempUpperFilter);
				if (!isVisible)
					isVisible = n.Nodes.Any(IsNodeVisible);

				tempNodeVisibilityCache[n] = isVisible;
			}
			return isVisible;
		}

		private void AddComponentDialog_Activated(object sender, EventArgs e)
		{
			this.FilterTextBox.Focus();
		}

		private void OkButton_Click(object sender, EventArgs e)
		{
			SelectItem();
		}

		private void CancelButton_Click(object sender, EventArgs e)
		{
			this.SelectedType = null;
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			if (keyData != Keys.Escape) 
				return base.ProcessCmdKey(ref msg, keyData);

			this.DialogResult = DialogResult.Cancel;
			this.Close();
			return true;
		}

		private void SelectItem()
		{
			var item = (this.objectTypeView.SelectedNode != null ? ((Node)this.objectTypeView.SelectedNode.Tag).Tag as Type : null);
			this.SelectedType = item;
			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		private void objectTypeView_DoubleClick(object sender, EventArgs e)
		{
			SelectItem();
		}
	}
}
