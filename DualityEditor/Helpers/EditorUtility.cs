using System;
using Duality.Resources;
using ManagedSquish;

namespace Duality.Editor.Helpers
{
	public static class EditorUtility
	{
		public static bool CompressTexture(Texture texture)
		{
			try
			{
				byte[] bytes = Squish.CompressImage(texture.BasePixmap.Res.ProcessedLayer.GetPixelDataByteRgba(), texture.PixelWidth, texture.PixelHeight, SquishFlags.Dxt5);
				var layer = texture.BasePixmap.Res.ProcessedLayer;
				layer.SetPixelDataDxtCompressed(bytes);

				DualityEditorApp.NotifyObjPropChanged(null, new ObjectSelection(texture.BasePixmap.Res));
				DualityEditorApp.NotifyObjPropChanged(null, new ObjectSelection(texture));
				return true;
			}
			catch (Exception e)
			{
				Log.Editor.WriteError("Something went wrong while DXT compressing {0}: {1}", texture.Name, e.Message);
				return false;
			}
		}
	}
}