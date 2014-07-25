using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AdamsLair.WinForms.PropertyEditing;
using Duality.Editor.Helpers;
using Duality.Resources;

namespace Duality.Editor.Plugins.Base.PropertyEditors
{
	[PropertyEditorAssignment(typeof(Texture), PropertyEditorAssignmentAttribute.PrioritySpecialized)]
	public class TexturePropertyEditor : ResourcePropertyEditor
	{
		public TexturePropertyEditor()
		{
			this.Indent = 0;
		}

		protected override bool IsAutoCreateMember(MemberInfo info)
		{
			return false;
		}
		protected override void BeforeAutoCreateEditors()
		{
			base.BeforeAutoCreateEditors();
			TexturePreviewPropertyEditor preview = new TexturePreviewPropertyEditor();
			preview.EditedType = this.EditedType;
			preview.Getter = this.GetValue;
			this.ParentGrid.ConfigureEditor(preview);
			this.AddPropertyEditor(preview);
			TextureContentPropertyEditor content = new TextureContentPropertyEditor();
			content.EditedType = this.EditedType;
			content.Getter = this.GetValue;
			content.Setter = this.SetValues;
			content.PreventFocus = true;
			this.ParentGrid.ConfigureEditor(content);
			this.AddPropertyEditor(content);
		}

		protected override void OnValueChanged(object sender, PropertyEditorValueEventArgs args)
		{
			base.OnValueChanged(sender, args);

			if (args.Editor.EditedMember == ReflectionInfo.Property_Texture_Compressed)
			{
				CompressSelectedTextures(args);
			}
			else if (args.Editor.EditedMember == ReflectionInfo.Property_Texture_PremultiplyAlpha)
			{
				foreach (var texture in GetValue().Cast<Texture>())
				{
					if (texture.BasePixmap.Res.ProcessedLayer.IsCompressed)
					{
						CompressTexture(texture);
					}
				}
			}
		}

		private void CompressSelectedTextures(PropertyEditorValueEventArgs args)
		{
			foreach (var texture in GetValue().Cast<Texture>())
			{
				if ((bool) args.Value)
				{
					CompressTexture(texture);
				}
				else
				{
					texture.BasePixmap.Res.DeleteCompressedPixelData();
					DualityEditorApp.NotifyObjPropChanged(this, new ObjectSelection(texture.BasePixmap.Res));
				}
			}
		}

		private static void CompressTexture(Texture texture)
		{
			if (EditorUtility.CompressTexture(texture) == false)
				texture.Compressed = false;
		}
	}
}
