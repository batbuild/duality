namespace Duality.Editor.Forms
{
	partial class AddComponentDialog
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
			this.objectTypeView = new Aga.Controls.Tree.TreeViewAdv();
			this.treeNodeIcon = new Aga.Controls.Tree.NodeControls.NodeIcon();
			this.treeNodeName = new Aga.Controls.Tree.NodeControls.NodeTextBox();
			this.FilterLabel = new System.Windows.Forms.Label();
			this.FilterTextBox = new System.Windows.Forms.TextBox();
			this.OkButton = new System.Windows.Forms.Button();
			this.CancelButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// objectTypeView
			// 
			this.objectTypeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.objectTypeView.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(196)))), ((int)(((byte)(196)))), ((int)(((byte)(196)))));
			this.objectTypeView.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.objectTypeView.ColumnHeaderHeight = 0;
			this.objectTypeView.DefaultToolTipProvider = null;
			this.objectTypeView.DragDropMarkColor = System.Drawing.Color.Black;
			this.objectTypeView.FullRowSelect = true;
			this.objectTypeView.FullRowSelectActiveColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
			this.objectTypeView.FullRowSelectInactiveColor = System.Drawing.Color.FromArgb(((int)(((byte)(212)))), ((int)(((byte)(212)))), ((int)(((byte)(212)))));
			this.objectTypeView.LineColor = System.Drawing.Color.FromArgb(((int)(((byte)(92)))), ((int)(((byte)(92)))), ((int)(((byte)(92)))));
			this.objectTypeView.LoadOnDemand = true;
			this.objectTypeView.Location = new System.Drawing.Point(3, 26);
			this.objectTypeView.Model = null;
			this.objectTypeView.Name = "objectTypeView";
			this.objectTypeView.NodeControls.Add(this.treeNodeIcon);
			this.objectTypeView.NodeControls.Add(this.treeNodeName);
			this.objectTypeView.NodeFilter = null;
			this.objectTypeView.SelectedNode = null;
			this.objectTypeView.Size = new System.Drawing.Size(458, 662);
			this.objectTypeView.TabIndex = 1;
			this.objectTypeView.DoubleClick += new System.EventHandler(this.objectTypeView_DoubleClick);
			// 
			// treeNodeIcon
			// 
			this.treeNodeIcon.LeftMargin = 1;
			this.treeNodeIcon.ParentColumn = null;
			this.treeNodeIcon.ScaleMode = Aga.Controls.Tree.ImageScaleMode.Clip;
			// 
			// treeNodeName
			// 
			this.treeNodeName.DataPropertyName = "Text";
			this.treeNodeName.IncrementalSearchEnabled = true;
			this.treeNodeName.LeftMargin = 3;
			this.treeNodeName.ParentColumn = null;
			// 
			// FilterLabel
			// 
			this.FilterLabel.AutoSize = true;
			this.FilterLabel.BackColor = System.Drawing.Color.Transparent;
			this.FilterLabel.Dock = System.Windows.Forms.DockStyle.Left;
			this.FilterLabel.Location = new System.Drawing.Point(0, 0);
			this.FilterLabel.Name = "FilterLabel";
			this.FilterLabel.Padding = new System.Windows.Forms.Padding(3);
			this.FilterLabel.Size = new System.Drawing.Size(38, 19);
			this.FilterLabel.TabIndex = 2;
			this.FilterLabel.Text = "Filter:";
			this.FilterLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// FilterTextBox
			// 
			this.FilterTextBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(196)))), ((int)(((byte)(196)))), ((int)(((byte)(196)))));
			this.FilterTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.FilterTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FilterTextBox.Location = new System.Drawing.Point(38, 0);
			this.FilterTextBox.Name = "FilterTextBox";
			this.FilterTextBox.Size = new System.Drawing.Size(435, 20);
			this.FilterTextBox.TabIndex = 0;
			this.FilterTextBox.TextChanged += new System.EventHandler(this.FilterTextBox_TextChanged);
			// 
			// OkButton
			// 
			this.OkButton.Location = new System.Drawing.Point(305, 694);
			this.OkButton.Name = "OkButton";
			this.OkButton.Size = new System.Drawing.Size(75, 23);
			this.OkButton.TabIndex = 2;
			this.OkButton.Text = "Ok";
			this.OkButton.UseVisualStyleBackColor = true;
			this.OkButton.Click += new System.EventHandler(this.OkButton_Click);
			// 
			// CancelButton
			// 
			this.CancelButton.Location = new System.Drawing.Point(386, 694);
			this.CancelButton.Name = "CancelButton";
			this.CancelButton.Size = new System.Drawing.Size(75, 23);
			this.CancelButton.TabIndex = 3;
			this.CancelButton.Text = "Cancel";
			this.CancelButton.UseVisualStyleBackColor = true;
			this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// AddComponentDialog
			// 
			this.AcceptButton = this.OkButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.CancelButton;
			this.ClientSize = new System.Drawing.Size(473, 724);
			this.Controls.Add(this.CancelButton);
			this.Controls.Add(this.OkButton);
			this.Controls.Add(this.FilterTextBox);
			this.Controls.Add(this.FilterLabel);
			this.Controls.Add(this.objectTypeView);
			this.Name = "AddComponentDialog";
			this.Text = "Select component";
			this.Activated += new System.EventHandler(this.AddComponentDialog_Activated);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Aga.Controls.Tree.TreeViewAdv objectTypeView;
		private System.Windows.Forms.Label FilterLabel;
		private System.Windows.Forms.TextBox FilterTextBox;
		private System.Windows.Forms.Button OkButton;
		private System.Windows.Forms.Button CancelButton;
		private Aga.Controls.Tree.NodeControls.NodeIcon treeNodeIcon;
		private Aga.Controls.Tree.NodeControls.NodeTextBox treeNodeName;
	}
}