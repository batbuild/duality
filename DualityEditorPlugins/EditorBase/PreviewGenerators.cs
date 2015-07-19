using System;
using System.Drawing;
using System.Drawing.Drawing2D;

using OpenTK;

using Duality;
using Duality.Resources;
using Duality.Drawing;
using Duality.Editor;
using Font = Duality.Resources.Font;


namespace Duality.Editor.Plugins.Base.PreviewGenerators
{
	public class PixmapPreviewGenerator : PreviewGenerator<Pixmap>
	{
		public override void Perform(Pixmap pixmap, PreviewImageQuery query)
		{
			int desiredWidth = query.DesiredWidth;
			int desiredHeight = query.DesiredHeight;

			Pixmap.Layer layer = pixmap.MainLayer;
			if (layer == null)
			{
				query.Result = new Bitmap(1, 1);
				return;
			}
			float widthRatio = (float)layer.Width / (float)layer.Height;

			if (pixmap.Width * pixmap.Height > 4096 * 4096)
			{
				layer = layer.CloneSubImage(
					pixmap.Width / 2 - Math.Min(desiredWidth, pixmap.Width) / 2,
					pixmap.Height / 2 - Math.Min(desiredHeight, pixmap.Height) / 2,
					Math.Min(desiredWidth, pixmap.Width),
					Math.Min(desiredHeight, pixmap.Height));
				if (layer.Width != desiredWidth || layer.Height != desiredHeight)
					layer = layer.CloneRescale(desiredWidth, desiredHeight, Pixmap.FilterMethod.Linear);
			}
			else if (query.SizeMode == PreviewSizeMode.FixedBoth)
				layer = layer.CloneRescale(desiredWidth, desiredHeight, Pixmap.FilterMethod.Linear);
			else if (query.SizeMode == PreviewSizeMode.FixedWidth)
				layer = layer.CloneRescale(desiredWidth, MathF.RoundToInt(desiredWidth / widthRatio), Pixmap.FilterMethod.Linear);
			else if (query.SizeMode == PreviewSizeMode.FixedHeight)
				layer = layer.CloneRescale(MathF.RoundToInt(widthRatio * desiredHeight), desiredHeight, Pixmap.FilterMethod.Linear);
			else
				layer = layer.Clone();

			query.Result = layer.ToBitmap();
		}
	}
	public class FontPreviewGenerator : PreviewGenerator<Font>
	{
		public override void Perform(Font font, PreviewImageQuery query)
		{
			int desiredWidth = query.DesiredWidth;
			int desiredHeight = query.DesiredHeight;
			const string text = "/acThe quick brown fox jumps over the lazy dog.";
			FormattedText formatText = new FormattedText();
			formatText.MaxWidth = Math.Max(1, desiredWidth - 10);
			formatText.MaxHeight = Math.Max(1, desiredHeight - 10);
			formatText.WordWrap = FormattedText.WrapMode.Word;
			formatText.Fonts = new[] { new ContentRef<Font>(font) };
			formatText.ApplySource(text);
			Pixmap.Layer textLayer = new Pixmap.Layer(desiredWidth, MathF.RoundToInt(formatText.Size.Y));
			formatText.RenderToBitmap(text, textLayer, 5, 0);

			Bitmap resultBitmap = textLayer.ToBitmap();

			//// Debug Font metrics
			//const bool drawDebugFontMetrics = false;
			//if (drawDebugFontMetrics)
			//{
			//    bool brightFg = font.GlyphColor.GetLuminance() > 0.5f;
			//    Color fgColor = Color.FromArgb(font.GlyphColor.R, font.GlyphColor.G, font.GlyphColor.B);
			//    Color baseLineColor = brightFg ? Color.FromArgb(255, 0, 0) : Color.FromArgb(128, 0, 0);
			//    Color bodyAscentColor = brightFg ? Color.FromArgb(0, 192, 0) : Color.FromArgb(0, 64, 0);
			//    Color ascentColor = brightFg ? Color.FromArgb(64, 64, 255) : Color.FromArgb(0, 0, 128);
			//    Color descentColor = brightFg ? Color.FromArgb(255, 0, 255) : Color.FromArgb(128, 0, 128);
			//    using (Graphics g = Graphics.FromImage(resultBitmap))
			//    {
			//        for (int i = 0; i < metrics.LineCount; i++)
			//        {
			//            Rect lineBounds = metrics.LineBounds[i];
			//            g.DrawRectangle(new Pen(Color.FromArgb(128, fgColor)), lineBounds.X + 5, lineBounds.Y, lineBounds.W, lineBounds.H - 1);
			//            g.DrawLine(new Pen(Color.FromArgb(192, baseLineColor)), 0, lineBounds.Y + font.BaseLine, resultBitmap.Width, lineBounds.Y + font.BaseLine);
			//            g.DrawLine(new Pen(Color.FromArgb(192, bodyAscentColor)), 0, lineBounds.Y + font.BaseLine - font.BodyAscent, resultBitmap.Width, lineBounds.Y + font.BaseLine - font.BodyAscent);
			//            g.DrawLine(new Pen(Color.FromArgb(192, ascentColor)), 0, lineBounds.Y + font.BaseLine - font.Ascent, resultBitmap.Width, lineBounds.Y + font.BaseLine - font.Ascent);
			//            g.DrawLine(new Pen(Color.FromArgb(192, descentColor)), 0, lineBounds.Y + font.BaseLine + font.Descent, resultBitmap.Width, lineBounds.Y + font.BaseLine + font.Descent);
			//        }
			//    }
			//}

			query.Result = resultBitmap.Resize(desiredWidth, desiredHeight, Alignment.Left);
		}
	}
}
