using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Aga.Controls.Tree;

namespace Duality.Editor.Plugins.Base.Modules
{
	public partial class ObjectSelector : Form
	{
		private readonly Dictionary<Node, bool> _tempNodeVisibilityCache = new Dictionary<Node, bool>();
		private readonly TreeModel _treeModel;
		private string _tempUpperFilter;

		public ObjectSelector()
		{
			InitializeComponent();

			_treeModel = new TreeModel();
			SelectableObjectsTreeView.Model = _treeModel;
			SelectableObjectsTreeView.NodeMouseDoubleClick += OnSelectNode;
			SelectableObjectsTreeView.NodeMouseClick += OnPreviewNode;
			SelectableObjectsTreeView.SelectionChanged += OnSelectionChanged;

			PathLabel.Text = "";
		}

		public Resource SelectedObject { get; set; }

		public void SetTreeViewItems(IEnumerable<Node> items)
		{
			foreach (var item in items)
			{
				_treeModel.Nodes.Add(item);
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			FilterTextBox.Select();
		}

		private void ApplyNodeFilter()
		{
			_tempUpperFilter = String.IsNullOrEmpty(FilterTextBox.Text) ? null : FilterTextBox.Text.ToUpper();
			_tempNodeVisibilityCache.Clear();
			SelectableObjectsTreeView.NodeFilter = _tempUpperFilter != null ? IsNodeVisible : (Predicate<object>)null;
		}

		private bool IsNodeVisible(object obj)
		{
			if (_tempUpperFilter == null) 
				return true;

			var vn = obj as TreeNodeAdv;
			var n = vn != null ? vn.Tag as Node : obj as Node;
			if (n == null) 
				return true;

			bool isVisible;
			if (!_tempNodeVisibilityCache.TryGetValue(n, out isVisible))
			{
				isVisible = n.Text.ToUpper().Contains(_tempUpperFilter);
				if (!isVisible) 
					isVisible = n.Nodes.Any(IsNodeVisible);

				_tempNodeVisibilityCache[n] = isVisible;
			}
			return isVisible;
		}

		private void OnPreviewNode(object sender, TreeNodeAdvMouseEventArgs e)
		{
			SetSelectedObject(e.Node);
			UpdatePreview();
		}

		private void OnSelectionChanged(object sender, EventArgs e)
		{
			var selectedNode = SelectableObjectsTreeView.SelectedNode;

			if (selectedNode == null)
				return;

			SetSelectedObject(selectedNode);
			UpdatePreview();
		}

		private void OnSelectNode(object sender, TreeNodeAdvMouseEventArgs e)
		{
			SetSelectedObject(e.Node);

			DialogResult = DialogResult.OK;
			Close();
		}

		private void UpdatePreview()
		{
			PreviewImage.Image = null;
			var previewImage = PreviewProvider.GetPreviewImage(SelectedObject, PreviewImage.Width, PreviewImage.Height,
				PreviewSizeMode.FixedBoth);
			if (previewImage != null)
				PreviewImage.Image = previewImage;

			PathLabel.Text = SelectedObject.Path;
		}

		private void SetSelectedObject(TreeNodeAdv node)
		{
			var contentRef = ((Node)node.Tag).Tag as IContentRef;

			if (contentRef == null)
				return;

			SelectedObject = contentRef.Res;
		}

		private void TextBoxFilterTextChanged(object sender, EventArgs e)
		{
			ApplyNodeFilter();
		}
	}
}
