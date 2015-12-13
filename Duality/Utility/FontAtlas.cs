using System.Collections.Generic;
using System.Linq;
using Duality.Drawing;
using Duality.Resources;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using SharpFont;

namespace Duality.Utility
{
	public class FontAtlas
	{
		private readonly Font.RenderMode _renderMode;
		private Dictionary<int, Rect> _glyphUVs = new Dictionary<int, Rect>();
		private readonly bool _pixelGridAlign;
		private RectanglePacker _packer;
		private int _textureSize;

		public Material Material { get; private set; }
		public Texture Texture { get; private set; }

		public FontAtlas(int textureSize, Font.RenderMode renderMode, bool pixelGridAlign)
		{
			_textureSize = textureSize;
			_renderMode = renderMode;
			_pixelGridAlign = pixelGridAlign;
			_packer = new RectanglePacker(textureSize);
			Resize();
		}

		public Rect GetTextureRect(int charIndex)
		{
			return _glyphUVs.ContainsKey(charIndex) ? _glyphUVs[charIndex] : Rect.Empty;
		}

		public Rect Insert(int charIndex, Face face)
		{
			Rect uvRect;
			var coords = _packer.Pack(face.Glyph.Metrics.Width.ToInt32(), face.Glyph.Metrics.Height.ToInt32());

			while (coords.X < 0 && _textureSize < 8192)
			{
				// this resets the atlas, but with a larger texture, so everything needs to be packed again
				_textureSize *= 2;
				Resize();

				// we don't want to duplicate all of the glyph byte data in the font atlas, so after resizing, we need to go through
				// each glyph and re-upload that data
				for (var i = 0; i < _glyphUVs.Keys.Count; i++)
				{
					var glyphIndex = _glyphUVs.Keys.ElementAt(i);
					face.LoadGlyph((uint)glyphIndex, LoadFlags.Default, LoadTarget.Normal);

					coords = _packer.Pack(face.Glyph.Metrics.Width.ToInt32(), face.Glyph.Metrics.Height.ToInt32());
					_glyphUVs[glyphIndex] = CalculateUVs(face, coords);

					UploadGlyph((uint) glyphIndex, face, coords);
				}

				coords = _packer.Pack(face.Glyph.Metrics.Width.ToInt32(), face.Glyph.Metrics.Height.ToInt32());
			}

			uvRect = CalculateUVs(face, coords);
			_glyphUVs[charIndex] = uvRect;

			// couldn't fit this glyph because the texture atlas reached the maximum size
			if (coords.X < 0)
				Log.Game.WriteWarning("Couldn't fit glyph in dynamic font atlas for font. Consider using a smaller font size or use fewer unique characters.");
			else
				UploadGlyph((uint) charIndex, face, coords);

			return uvRect;
		}

		private Rect CalculateUVs(Face face, Vector2 coords)
		{
			return new Rect((coords.X + _packer.Padding) / _packer.Size, (coords.Y + _packer.Padding) / _packer.Size, face.Glyph.Metrics.Width.ToSingle() / _packer.Size, face.Glyph.Metrics.Height.ToSingle() / _packer.Size);
		}

		private void UploadGlyph(uint charIndex, Face face, Vector2 coords)
		{
			face.LoadGlyph(charIndex, LoadFlags.Render | LoadFlags.ForceAutohint, LoadTarget.Normal);
			var glyph = face.Glyph.GetGlyph();
			var bitmap = glyph.ToBitmapGlyph();

			var data = new byte[bitmap.Bitmap.BufferData.Length * 4];
			for (var i = 0; i < data.Length; i += 4)
			{
				data[i] = 255;
				data[i + 1] = 255;
				data[i + 2] = 255;
				data[i + 3] = bitmap.Bitmap.BufferData[i / 4];
			}
			Texture.UploadSubImage((int) coords.X + _packer.Padding, (int) coords.Y + _packer.Padding, 
				(int) face.Glyph.Metrics.Width, (int) face.Glyph.Metrics.Height, data);
		}

		private void Resize()
		{
			_packer.Resize(_textureSize);
			
			if(Texture != null)
				Texture.Dispose();

			Texture = new Texture(_textureSize, _textureSize, Texture.SizeMode.Enlarge,
				_pixelGridAlign ? TextureMagFilter.Nearest : TextureMagFilter.Linear,
				_pixelGridAlign ? TextureMinFilter.Nearest : TextureMinFilter.Linear);
			Texture.Clear();
			
			if(Material != null)
				Material.Dispose();

			CreateInternalMaterial();
		}

		private void CreateInternalMaterial()
		{
			// Select DrawTechnique to use
			ContentRef<DrawTechnique> technique;
			switch (_renderMode)
			{
				case Font.RenderMode.MonochromeBitmap:
					technique = DrawTechnique.Mask;
					break;
				case Font.RenderMode.GrayscaleBitmap:
				case Font.RenderMode.SmoothBitmap:
					technique = DrawTechnique.Alpha;
					break;
				default:
					technique = DrawTechnique.SharpAlpha;
					break;
			}

			var matInfo = new BatchInfo(technique, ColorRgba.White, Texture);
			if (technique == DrawTechnique.SharpAlpha)
				matInfo.SetUniform("smoothness", _textureSize * 3.0f);

			Material = new Material(matInfo);
		}

		public void Dispose()
		{
			if(Texture != null)
				Texture.Dispose();

			if(Material != null)
				Material.Dispose();
		}
	}
}
