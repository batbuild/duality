namespace EditorBase.Modules
{
	partial class ObjectSelector
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.PreviewImage = new System.Windows.Forms.PictureBox();
			this.PathLabel = new System.Windows.Forms.Label();
			this.SelectableObjectsTreeView = new Aga.Controls.Tree.TreeViewAdv();
			this.NameNodeTextBox = new Aga.Controls.Tree.NodeControls.NodeTextBox();
			this.panelBottom = new System.Windows.Forms.Panel();
			this.FilterTextBox = new System.Windows.Forms.TextBox();
			this.FilterLabel = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.PreviewImage)).BeginInit();
			this.panelBottom.SuspendLayout();
			this.SuspendLayout();
			// 
			// PreviewImage
			// 
			this.PreviewImage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.PreviewImage.Location = new System.Drawing.Point(6, 391);
			this.PreviewImage.Name = "PreviewImage";
			this.PreviewImage.Size = new System.Drawing.Size(110, 108);
			this.PreviewImage.TabIndex = 2;
			this.PreviewImage.TabStop = false;
			// 
			// PathLabel
			// 
			this.PathLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.PathLabel.AutoSize = true;
			this.PathLabel.Location = new System.Drawing.Point(122, 391);
			this.PathLabel.Name = "PathLabel";
			this.PathLabel.Size = new System.Drawing.Size(35, 13);
			this.PathLabel.TabIndex = 3;
			this.PathLabel.Text = "label1";
			// 
			// SelectableObjectsTreeView
			// 
			this.SelectableObjectsTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.SelectableObjectsTreeView.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(196)))), ((int)(((byte)(196)))), ((int)(((byte)(196)))));
			this.SelectableObjectsTreeView.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.SelectableObjectsTreeView.DefaultToolTipProvider = null;
			this.SelectableObjectsTreeView.DisplayDraggingNodes = true;
			this.SelectableObjectsTreeView.DragDropMarkColor = System.Drawing.Color.Black;
			this.SelectableObjectsTreeView.FullRowSelect = true;
			this.SelectableObjectsTreeView.FullRowSelectActiveColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
			this.SelectableObjectsTreeView.FullRowSelectInactiveColor = System.Drawing.Color.FromArgb(((int)(((byte)(212)))), ((int)(((byte)(212)))), ((int)(((byte)(212)))));
			this.SelectableObjectsTreeView.LineColor = System.Drawing.Color.FromArgb(((int)(((byte)(92)))), ((int)(((byte)(92)))), ((int)(((byte)(92)))));
			this.SelectableObjectsTreeView.Location = new System.Drawing.Point(0, 0);
			this.SelectableObjectsTreeView.Model = null;
			this.SelectableObjectsTreeView.Name = "SelectableObjectsTreeView";
			this.SelectableObjectsTreeView.NodeControls.Add(this.NameNodeTextBox);
			this.SelectableObjectsTreeView.NodeFilter = null;
			this.SelectableObjectsTreeView.SelectedNode = null;
			this.SelectableObjectsTreeView.ShowNodeToolTips = true;
			this.SelectableObjectsTreeView.ShowPlusMinus = false;
			this.SelectableObjectsTreeView.Size = new System.Drawing.Size(537, 385);
			this.SelectableObjectsTreeView.TabIndex = 1;
			// 
			// NameNodeTextBox
			// 
			this.NameNodeTextBox.DataPropertyName = "Text";
			this.NameNodeTextBox.IncrementalSearchEnabled = true;
			this.NameNodeTextBox.LeftMargin = 20;
			this.NameNodeTextBox.ParentColumn = null;
			// 
			// panelBottom
			// 
			this.panelBottom.BackColor = System.Drawing.Color.Transparent;
			this.panelBottom.Controls.Add(this.FilterTextBox);
			this.panelBottom.Controls.Add(this.FilterLabel);
			this.panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panelBottom.Location = new System.Drawing.Point(0, 504);
			this.panelBottom.Name = "panelBottom";
			this.panelBottom.Padding = new System.Windows.Forms.Padding(3);
			this.panelBottom.Size = new System.Drawing.Size(537, 26);
			this.panelBottom.TabIndex = 4;
			// 
			// FilterTextBox
			// 
			this.FilterTextBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(196)))), ((int)(((byte)(196)))), ((int)(((byte)(196)))));
			this.FilterTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.FilterTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FilterTextBox.Location = new System.Drawing.Point(41, 3);
			this.FilterTextBox.Name = "FilterTextBox";
			this.FilterTextBox.Size = new System.Drawing.Size(493, 20);
			this.FilterTextBox.TabIndex = 0;
			this.FilterTextBox.TextChanged += new System.EventHandler(this.TextBoxFilterTextChanged);
			// 
			// FilterLabel
			// 
			this.FilterLabel.AutoSize = true;
			this.FilterLabel.BackColor = System.Drawing.Color.Transparent;
			this.FilterLabel.Dock = System.Windows.Forms.DockStyle.Left;
			this.FilterLabel.Location = new System.Drawing.Point(3, 3);
			this.FilterLabel.Name = "FilterLabel";
			this.FilterLabel.Padding = new System.Windows.Forms.Padding(3);
			this.FilterLabel.Size = new System.Drawing.Size(38, 19);
			this.FilterLabel.TabIndex = 1;
			this.FilterLabel.Text = "Filter:";
			this.FilterLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// ObjectSelector
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(537, 530);
			this.Controls.Add(this.panelBottom);
			this.Controls.Add(this.PathLabel);
			this.Controls.Add(this.PreviewImage);
			this.Controls.Add(this.SelectableObjectsTreeView);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.Name = "ObjectSelector";
			this.Text = "Select ";
			((System.ComponentModel.ISupportInitialize)(this.PreviewImage)).EndInit();
			this.panelBottom.ResumeLayout(false);
			this.panelBottom.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Aga.Controls.Tree.TreeViewAdv SelectableObjectsTreeView;
		private Aga.Controls.Tree.NodeControls.NodeTextBox NameNodeTextBox;
		private System.Windows.Forms.PictureBox PreviewImage;
		private System.Windows.Forms.Label PathLabel;
		private System.Windows.Forms.Panel panelBottom;
		private System.Windows.Forms.TextBox FilterTextBox;
		private System.Windows.Forms.Label FilterLabel;
	}
}