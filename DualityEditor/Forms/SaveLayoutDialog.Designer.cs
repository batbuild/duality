namespace DualityEditor.Forms
{
	partial class SaveLayoutDialog
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
			this.LayoutNameLabel = new System.Windows.Forms.Label();
			this.LayoutNameTextBox = new System.Windows.Forms.TextBox();
			this.SaveButton = new System.Windows.Forms.Button();
			this.CancelButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// LayoutNameLabel
			// 
			this.LayoutNameLabel.AutoSize = true;
			this.LayoutNameLabel.Location = new System.Drawing.Point(12, 16);
			this.LayoutNameLabel.Name = "LayoutNameLabel";
			this.LayoutNameLabel.Size = new System.Drawing.Size(68, 13);
			this.LayoutNameLabel.TabIndex = 0;
			this.LayoutNameLabel.Text = "Layout name";
			// 
			// LayoutNameTextBox
			// 
			this.LayoutNameTextBox.Location = new System.Drawing.Point(87, 13);
			this.LayoutNameTextBox.Name = "LayoutNameTextBox";
			this.LayoutNameTextBox.Size = new System.Drawing.Size(301, 20);
			this.LayoutNameTextBox.TabIndex = 1;
			// 
			// SaveButton
			// 
			this.SaveButton.Cursor = System.Windows.Forms.Cursors.Default;
			this.SaveButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.SaveButton.Location = new System.Drawing.Point(232, 48);
			this.SaveButton.Name = "SaveButton";
			this.SaveButton.Size = new System.Drawing.Size(75, 23);
			this.SaveButton.TabIndex = 2;
			this.SaveButton.Text = "Save";
			this.SaveButton.UseVisualStyleBackColor = true;
			this.SaveButton.Click += new System.EventHandler(this.SaveButton_Click);
			// 
			// CancelButton
			// 
			this.CancelButton.Cursor = System.Windows.Forms.Cursors.Default;
			this.CancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelButton.Location = new System.Drawing.Point(313, 48);
			this.CancelButton.Name = "CancelButton";
			this.CancelButton.Size = new System.Drawing.Size(75, 23);
			this.CancelButton.TabIndex = 3;
			this.CancelButton.Text = "Cancel";
			this.CancelButton.UseVisualStyleBackColor = true;
			this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// SaveLayoutDialog
			// 
			this.AcceptButton = this.SaveButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.CancelButton;
			this.ClientSize = new System.Drawing.Size(400, 83);
			this.Controls.Add(this.CancelButton);
			this.Controls.Add(this.SaveButton);
			this.Controls.Add(this.LayoutNameTextBox);
			this.Controls.Add(this.LayoutNameLabel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "SaveLayoutDialog";
			this.ShowIcon = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Save Layout...";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label LayoutNameLabel;
		private System.Windows.Forms.TextBox LayoutNameTextBox;
		private System.Windows.Forms.Button SaveButton;
		private System.Windows.Forms.Button CancelButton;
	}
}