﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;

using AdamsLair.WinForms.PropertyEditing;
using AdamsLair.WinForms.Drawing;
using Aga.Controls.Tree;
using Duality.Editor.Plugins.Base.Modules;
using ButtonState = AdamsLair.WinForms.Drawing.ButtonState;
using BorderStyle = AdamsLair.WinForms.Drawing.BorderStyle;

using Duality;
using Duality.Resources;
using Duality.Editor;

namespace Duality.Editor.Plugins.Base.PropertyEditors
{
	[PropertyEditorAssignment(typeof(IContentRef))]
	public class IContentRefPropertyEditor : ObjectRefPropertyEditor
	{
		protected	Type		editedResType		= null;
		protected	string		contentPath			= null;
		
		public override object DisplayedValue
		{
			get 
			{ 
				IContentRef ctRef = (this.EditedType.CreateInstanceOf() ?? typeof(ContentRef<Resource>).CreateInstanceOf()) as IContentRef;
				ctRef.Path = this.contentPath;
				ctRef.MakeAvailable();
				return ctRef;
			}
		}
		public override string ReferenceName
		{
			get 
			{
				IContentRef r = this.DisplayedValue as IContentRef;
				return r.IsExplicitNull ? null : r.FullName;
			}
		}
		public override bool ReferenceBroken
		{
			get
			{
				IContentRef r = this.DisplayedValue as IContentRef;
				return !r.IsExplicitNull && !r.IsAvailable;
			}
		}

		public override void ShowObjectSelector()
		{
			var objectSelector = new ObjectSelector
			{
				StartPosition = FormStartPosition.CenterScreen,
				Text = "Select " + editedResType.Name
			};
			objectSelector.SetTreeViewItems(ContentProvider.GetAvailableContent(editedResType)
				.Select(c => new Node(c.Name){Tag = c})
				.OrderBy(n => n.Text));

			var result = objectSelector.ShowDialog();
			if (result == DialogResult.Cancel)
				return;

			var selectedObject = objectSelector.SelectedObject;
			UpdateContentPath(selectedObject != null ? selectedObject.Path : null);
		}

		public override void ShowReferencedContent()
		{
			if (string.IsNullOrEmpty(this.contentPath)) return;
			IContentRef resRef = ContentProvider.RequestContent(this.contentPath);
			DualityEditorApp.Highlight(this, new ObjectSelection(resRef.Res));
		}

		public override void ResetReference()
		{
			if (this.ReadOnly) return;
			this.contentPath = null;
			this.PerformSetValue();
			this.PerformGetValue();
			this.OnEditingFinished(FinishReason.LeapValue);
		}
		protected override void OnGetValue()
		{
			base.OnGetValue();
			IContentRef[] values = this.GetValue().Cast<IContentRef>().ToArray();

			this.BeginUpdate();
			string lastPath = this.contentPath;
			bool lastMultiple = this.multiple;
			if (!values.Any())
			{
				this.contentPath = null;
			}
			else
			{
				IContentRef first = values.NotNull().FirstOrDefault();
				this.contentPath = first != null ? first.Path : null;
				this.multiple = (values.Any(o => o == null) || values.Any(o => o.Path != first.Path));

				this.GeneratePreview();
			}
			this.EndUpdate();
			if (lastPath != this.contentPath || lastMultiple != this.multiple) this.Invalidate();
		}

		protected void GeneratePreview()
		{
			int prevHash = this.GetPreviewHash();
			if (this.prevImageHash == prevHash) return;
			this.prevImageHash = prevHash;
			
			if (this.prevImage != null) this.prevImage.Dispose();
			this.prevImage = null;
			this.Height = 22;

			Resource res = (this.DisplayedValue as IContentRef).Res;
			if (res != null)
			{
				this.prevImage = PreviewProvider.GetPreviewImage(res, this.ClientRectangle.Width - 4 - 22, 64 - 4, PreviewSizeMode.FixedHeight);
				if (this.prevImage != null)
				{
					this.Height = 64;
					var avgColor = this.prevImage.GetAverageColor();
					this.prevImageLum = avgColor.GetLuminance();
				}
			}
		}

		protected override int GetPreviewHash()
		{
			if (this.contentPath == null) return 0;

			IContentRef contentRef = new ContentRef<Resource>(null, this.contentPath);
			if (!contentRef.IsAvailable) return 0;

			ConvertOperation convOp = new ConvertOperation(new[] { contentRef.Res }, ConvertOperation.Operation.Convert);
			if (convOp.CanPerform<Pixmap>())
			{
				Pixmap basePx = convOp.Perform<Pixmap>().FirstOrDefault();
				Pixmap.Layer basePxLayer = basePx != null ? basePx.MainLayer : null;
				return basePxLayer != null ? basePxLayer.GetHashCode() : 0;
			}

			return this.contentPath.GetHashCode();
		}

		protected override void SerializeToData(DataObject data)
		{
			data.SetContentRefs(new[] { this.DisplayedValue as IContentRef });
		}

		protected override void DeserializeFromData(DataObject data)
		{
			ConvertOperation convert = new ConvertOperation(data, ConvertOperation.Operation.All);
			if (convert.CanPerform(this.editedResType))
			{
				var refQuery = convert.Perform(this.editedResType);
				if (refQuery != null)
				{
					Resource[] refArray = refQuery.Cast<Resource>().ToArray();
					UpdateContentPath((refArray != null && refArray.Length > 0) ? refArray[0].Path : null);
				}
			}
		}

		protected override bool CanDeserializeFromData(DataObject data)
		{
			return new ConvertOperation(data, ConvertOperation.Operation.All).CanPerform(this.editedResType);
		}

		protected override void OnEditedTypeChanged()
		{
			base.OnEditedTypeChanged();
			if (this.EditedType.IsGenericType)
				this.editedResType = this.EditedType.GetGenericArguments()[0];
			else
				this.editedResType = typeof(Resource);
		}

		private void UpdateContentPath(string contentPath)
		{
			this.contentPath = contentPath;
			this.PerformSetValue();
			this.PerformGetValue();
			this.OnEditingFinished(FinishReason.LeapValue);
		}
	}
}

