using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
#if ! __ANDROID__
using System.Drawing.Text;
using SysDrawFont = System.Drawing.Font;
#else
using FontStyle = Duality.Resources.FontStyleA;
#endif
using Duality.Drawing;
using Duality.Editor;
using Duality.Properties;
using Duality.Utility;
using OpenTK;
using SharpFont;

namespace Duality.Resources
{

	public enum FontStyleA
	{
		Regular,
		Bold,
		Italic,
	}

	/// <summary>
	/// Represents a font. While any system font or imported TrueType font can be used, they are internally
	/// pre-rasterized and stored in a <see cref="Duality.Resources.Texture"/> with an <see cref="Duality.Resources.Pixmap.Atlas"/>.
	/// </summary>
	[Serializable]
	[ExplicitResourceReference()]
#if ! __ANDROID__
	[EditorHintCategory(typeof(CoreRes), CoreResNames.CategoryGraphics)]
	[EditorHintImage(typeof(CoreRes), CoreResNames.ImageFont)]
#endif
	public class Font : Resource
	{
		
		/// <summary>
		/// A Font resources file extension.
		/// </summary>
		public new const string FileExt = ".Font" + Resource.FileExt;

		private const int Dpi = 96;

		/// <summary>
		/// A generic <see cref="MonoSpace">monospace</see> Font (Size 8) that has been loaded from your systems font library.
		/// This is usually "Courier New".
		/// </summary>
		public static ContentRef<Font> GenericMonospace8	{ get; private set; }
		/// <summary>
		/// A generic <see cref="MonoSpace">monospace</see> Font (Size 10) that has been loaded from your systems font library.
		/// This is usually "Courier New".
		/// </summary>
		public static ContentRef<Font> GenericMonospace10	{ get; private set; }
		/// <summary>
		/// A generic serif Font (Size 12) that has been loaded from your systems font library.
		/// This is usually "Times New Roman".
		/// </summary>
		public static ContentRef<Font> GenericSerif12		{ get; private set; }
		/// <summary>
		/// A generic sans-serif Font (Size 12) that has been loaded from your systems font library.
		/// This is usually "Arial".
		/// </summary>
		public static ContentRef<Font> GenericSansSerif12	{ get; private set; }

		internal static void InitDefaultContent()
		{
			string contentPath;
			string extension = string.Empty;
#if __ANDROID__
			 contentPath = "Data\\Default\\Font\\";
			extension = FileExt;
#else
			contentPath = ContentProvider.VirtualContentPath + "Font:";
#endif
			string ContentPath_GenericMonospace10 = contentPath + "GenericMonospace10" + extension;
			string ContentPath_GenericMonospace8 = contentPath + "GenericMonospace8" + extension;
			string ContentPath_GenericSerif12 = contentPath + "GenericSerif12" + extension;
			string ContentPath_GenericSansSerif12 = contentPath + "GenericSansSerif12" + extension;

			var genericMonospace8 = new Font("LiberationMono-Regular", 8, isEmbeddedFont: true);
			var genericMonospace10 = new Font("LiberationMono-Regular", 10, isEmbeddedFont: true);
			var genericSansSerif12 = new Font("LiberationSans-Regular", 12, isEmbeddedFont: true);
			var genericSerif12 = new Font("LiberationSerif-Regular", 12, isEmbeddedFont: true);

			ContentProvider.AddContent(ContentPath_GenericMonospace8, genericMonospace8);
			ContentProvider.AddContent(ContentPath_GenericMonospace10, genericMonospace10);
			ContentProvider.AddContent(ContentPath_GenericSansSerif12, genericSansSerif12);
			ContentProvider.AddContent(ContentPath_GenericSerif12, genericSerif12);

			GenericMonospace8 = ContentProvider.RequestContent<Font>(ContentPath_GenericMonospace8);
			GenericMonospace10 = ContentProvider.RequestContent<Font>(ContentPath_GenericMonospace10);
			GenericSerif12 = ContentProvider.RequestContent<Font>(ContentPath_GenericSerif12);
			GenericSansSerif12 = ContentProvider.RequestContent<Font>(ContentPath_GenericSansSerif12);
		}

		/// <summary>
		/// Refers to a null reference Font.
		/// </summary>
		/// <seealso cref="ContentRef{T}.Null"/>
		public static readonly ContentRef<Font> None			= ContentRef<Font>.Null;

		private static Dictionary<string, byte[]> fontCache		= new Dictionary<string, byte[]>();

		static Font()
		{
		}


		/// <summary>
		/// Specifies how a text fitting algorithm works.
		/// </summary>
		public enum FitTextMode
		{
			/// <summary>
			/// Text is fit by character, i.e. can be separated anywhere.
			/// </summary>
			ByChar,
			/// <summary>
			/// Text is fit <see cref="ByWord">by word</see>, preferring leading whitespaces.
			/// </summary>
			ByWordLeadingSpace,
			/// <summary>
			/// Text is fit <see cref="ByWord">by word</see>, preferring trailing whitespaces.
			/// </summary>
			ByWordTrailingSpace,
			/// <summary>
			/// Text is fit by word boundaries, i.e. can only be separated between words.
			/// </summary>
			ByWord = ByWordTrailingSpace
		}

		/// <summary>
		/// Specifies how a Font is rendered. This affects both internal glyph rasterization and rendering.
		/// </summary>
		public enum RenderMode
		{
			/// <summary>
			/// A monochrome bitmap is used to store glyphs. Rendering is unfiltered and pixel-perfect.
			/// </summary>
			MonochromeBitmap,
			/// <summary>
			/// A greyscale bitmap is used to store glyphs. Rendering is unfiltered and pixel-perfect.
			/// </summary>
			GrayscaleBitmap,
			/// <summary>
			/// A greyscale bitmap is used to store glyphs. Rendering is properly filtered but may blur text display a little.
			/// </summary>
			SmoothBitmap,
			/// <summary>
			/// A greyscale bitmap is used to store glyphs. Rendering is properly filtered and uses a shader to enforce sharp masked edges.
			/// </summary>
			SharpBitmap
		}
		
		/// <summary>
		/// Contains data about a single glyph.
		/// </summary>
		[Serializable]
		public struct GlyphData
		{
			/// <summary>
			/// Thw width of the glyph
			/// </summary>
			public	int		width;
			/// <summary>
			/// The height of the glyph
			/// </summary>
			public	int		height;
			/// <summary>
			/// The glyphs X offset when rendering it.
			/// </summary>
			public	int		offsetX;
			/// <summary>
			/// The glyphs kerning samples to the left.
			/// </summary>
			public	int[]	kerningSamplesLeft;
			/// <summary>
			/// The glyphs kerning samples to the right.
			/// </summary>
			public	int[]	kerningSamplesRight;
		}



		private	string		familyName			= "LiberationSans-Regular";
		private float size						= 8.0f;
		private	FontStyle	style				= FontStyle.Regular;

		private bool		isEmbeddedFont		= false;
		private	RenderMode	renderMode			= RenderMode.SharpBitmap;
		private	float		spacing				= 0.0f;
		private	float		lineHeightFactor	= 1.0f;
		private	bool		monospace			= true;
		private	bool		kerning				= false;

		[NonSerialized] private Library		library			= null;
		[NonSerialized] private Face		face			= null;
		[NonSerialized] private	bool		needsReload		= true;
		[NonSerialized] private FontAtlas	fontAtlas		= null;

		private GlyphData[] glyphs			= null;
		private int			maxGlyphWidth	= 0;
		private	int			height			= 0;
		private	int			ascent			= 0;
		private	int			bodyAscent		= 0;
		private	int			descent			= 0;
		private	int			baseLine		= 0;

		public bool IsEmbeddedFont
		{
			get { return isEmbeddedFont; }
			set { isEmbeddedFont = value; }
		}

		/// <summary>
		/// [GET / SET] The name of the font family that is used.
		/// </summary>
		public string Family
		{
			get { return this.familyName; }
			set 
			{
				this.familyName = value;
				this.needsReload = true;
			}
		}

		/// <summary>
		/// [GET / SET] The size of the Font.
		/// </summary>
		[EditorHintFlags(MemberFlags.AffectsOthers)]
		[EditorHintRange(1, 150)]
		[EditorHintIncrement(1)]
		[EditorHintDecimalPlaces(1)]
		public float Size
		{
			get { return this.size; }
			set 
			{ 
				if (this.size != value)
				{
					this.size = Math.Max(1.0f, value);

					if (this.face == null)
					{
						this.needsReload = true;
						return;
					}

					this.face.SetCharSize(0, this.size, 0, Dpi);
					this.spacing = this.face.Size.Metrics.NominalWidth / 10.0f;
				}
			}
		}


		/// <summary>
		/// [GET / SET] The style of the font.
		/// </summary>
		public FontStyle Style
		{
			get { return this.style; }
			set
			{
				this.style = value;
				this.needsReload = true;
			}
		}

		/// [GET / SET] Specifies how a Font is rendered. This affects both internal glyph rasterization and rendering.
		/// </summary>
		[EditorHintFlags(MemberFlags.AffectsOthers)]
		public RenderMode GlyphRenderMode
		{
			get { return this.renderMode; }
			set
			{
				this.renderMode = value;
				this.needsReload = true;
			}
		}
		
		/// <summary>
		/// [GET] The <see cref="Duality.Resources.Material"/> to use when rendering text of this Font.
		/// </summary>
		[EditorHintFlags(MemberFlags.Invisible)]
		public Material Material
		{
			get { return this.fontAtlas.Material; }
		}
		/// <summary>
		/// [GET / SET] Additional spacing between each character. This is usually one tenth of the Fonts <see cref="Size"/>.
		/// </summary>
		public float CharSpacing
		{
			get { return this.spacing; }
			set { this.spacing = value; }
		}
		/// <summary>
		/// [GET / SET] A factor for the Fonts <see cref="Height"/> value that affects line spacings but not actual glyph sizes.
		/// </summary>
		[EditorHintFlags(MemberFlags.AffectsOthers)]
		public float LineHeightFactor
		{
			get { return this.lineHeightFactor; }
			set { this.lineHeightFactor = value; }
		}
		/// <summary>
		/// [GET / SET] Whether this is considered a monospace Font. If true, each character occupies exactly the same space.
		/// </summary>
		public bool MonoSpace
		{
			get { return this.monospace; }
			set { this.monospace = value; this.needsReload = true; }
		}
		/// <summary>
		/// [GET / SET] Whether this Font uses kerning, a technique where characters are moved closer together based on their actual shape,
		/// which usually looks much nicer. It has no visual effect when active at the same time with <see cref="MonoSpace"/>, however
		/// kerning sample data will be available on glyphs.
		/// </summary>
		/// <seealso cref="GlyphData"/>
		public bool Kerning
		{
			get { return this.kerning; }
			set { this.kerning = value; this.needsReload = true; }
		}
		/// <summary>
		/// [GET] Returns whether this Font needs a <see cref="ReloadData">reload</see> in order to apply
		/// changes that have been made to its Properties.
		/// </summary>
		[EditorHintFlags(MemberFlags.Invisible)]
		public bool NeedsReload
		{
			get { return this.needsReload; }
		}
		/// <summary>
		/// [GET] Returns whether this Font is (requesting to be) aligned to the pixel grid.
		/// </summary>
		public bool IsPixelGridAligned
		{
			get
			{ 
				return 
					this.renderMode == RenderMode.MonochromeBitmap || 
					this.renderMode == RenderMode.GrayscaleBitmap;
			}
		}
		/// <summary>
		/// [GET] The Fonts height.
		/// </summary>
		public int Height
		{
			get { return this.height; }
		}
		/// <summary>
		/// [GET] The y offset in pixels between two lines.
		/// </summary>
		public int LineSpacing
		{
			get { return MathF.RoundToInt(this.height * this.lineHeightFactor); }
		}
		/// <summary>
		/// [GET] The Fonts ascent value.
		/// </summary>
		public int Ascent
		{
			get { return this.ascent; }
		}
		/// <summary>
		/// [GET] The Fonts body ascent value.
		/// </summary>
		public int BodyAscent
		{
			get { return this.bodyAscent; }
		}
		/// <summary>
		/// [GET] The Fonts descent value.
		/// </summary>
		public int Descent
		{
			get { return this.descent; }
		}
		/// <summary>
		/// [GET] The Fonts base line height.
		/// </summary>
		public int BaseLine
		{
			get { return this.baseLine; }
		}

		/// <summary>
		/// Sets up a new Font.
		/// </summary>
		public Font()
		{
			
		}
		/// <summary>
		/// Creates a new Font based on a system font.
		/// </summary>
		/// <param name="familyName">The font family to use.</param>
		/// <param name="emSize">The Fonts <see cref="Size"/>.</param>
		/// <param name="style">The Fonts style.</param>
		public Font(string familyName, float emSize, FontStyle style = FontStyle.Regular, bool isEmbeddedFont = false) 
		{
			this.familyName = familyName;
			this.size = emSize;
			this.style = style;
			this.isEmbeddedFont = isEmbeddedFont;
			this.ReloadData();
		}

		/// <summary>
		/// Reloads this Fonts internal data and rasterizes its glyphs.
		/// </summary>
		public void ReloadData()
		{
			LoadFont();

			library = new Library();
			this.face = new Face(library, fontCache[this.familyName], 0);
			this.face.SetCharSize(0, this.size, 0, Dpi);

			this.needsReload = false;

			this.ReleaseResources();

			this.maxGlyphWidth = 0;
			this.height = 0;
			this.ascent = 0;
			this.bodyAscent = 0;
			this.descent = 0;
			this.baseLine = 0;
			this.glyphs = new GlyphData[16];

			this.GenerateResources();
			this.needsReload = true;
		}

		private void ReleaseResources()
		{
			if(this.fontAtlas != null)
				this.fontAtlas.Dispose();

			this.needsReload = true;
		}
		private void GenerateResources()
		{
			// Determine Font properties

			this.height = this.face.Size.Metrics.Height.ToInt32();
			this.ascent = this.face.Size.Metrics.Ascender.ToInt32();
			this.descent = this.face.Size.Metrics.Descender.ToInt32();
		}

		/// <summary>
		/// Emits a set of vertices based on a text. To render this text, simply use that set of vertices combined with
		/// the Fonts <see cref="Material"/>.
		/// </summary>
		/// <param name="text">The text to render.</param>
		/// <param name="vertices">The set of vertices that is emitted. You can re-use the same array each frame.</param>
		/// <param name="x">An X-Offset applied to the position of each emitted vertex.</param>
		/// <param name="y">An Y-Offset applied to the position of each emitted vertex.</param>
		/// <param name="z">An Z-Offset applied to the position of each emitted vertex.</param>
		/// <returns>The number of emitted vertices. This values isn't necessarily equal to the emitted arrays length.</returns>
		public int EmitTextVertices(string text, ref VertexC1P3T2[] vertices, float x, float y, float z = 0.0f)
		{
			return this.EmitTextVertices(text, ref vertices, x, y, z, ColorRgba.White);
		}
		/// <summary>
		/// Emits a set of vertices based on a text. To render this text, simply use that set of vertices combined with
		/// the Fonts <see cref="Material"/>.
		/// </summary>
		/// <param name="text">The text to render.</param>
		/// <param name="vertices">The set of vertices that is emitted. You can re-use the same array each frame.</param>
		/// <param name="x">An X-Offset applied to the position of each emitted vertex.</param>
		/// <param name="y">An Y-Offset applied to the position of each emitted vertex.</param>
		/// <param name="z">An Z-Offset applied to the position of each emitted vertex.</param>
		/// <param name="clr">The color value that is applied to each emitted vertex.</param>
		/// <param name="angle">An angle by which the text is rotated (before applying the offset).</param>
		/// <param name="scale">A factor by which the text is scaled (before applying the offset).</param>
		/// <returns>The number of emitted vertices. This values isn't necessarily equal to the emitted arrays length.</returns>
		public int EmitTextVertices(string text, ref VertexC1P3T2[] vertices, float x, float y, float z, ColorRgba clr, float angle = 0.0f, float scale = 1.0f)
		{
			int len = this.EmitTextVertices(text, ref vertices);
			
			Vector3 offset = new Vector3(x, y, z);
			Vector2 xDot, yDot;
			MathF.GetTransformDotVec(angle, scale, out xDot, out yDot);

			for (int i = 0; i < len; i++)
			{
				MathF.TransformDotVec(ref vertices[i].Pos, ref xDot, ref yDot);
				Vector3.Add(ref vertices[i].Pos, ref offset, out vertices[i].Pos);
				vertices[i].Color = clr;
			}

			return len;
		}
		/// <summary>
		/// Emits a set of vertices based on a text. To render this text, simply use that set of vertices combined with
		/// the Fonts <see cref="Material"/>.
		/// </summary>
		/// <param name="text">The text to render.</param>
		/// <param name="vertices">The set of vertices that is emitted. You can re-use the same array each frame.</param>
		/// <param name="x">An X-Offset applied to the position of each emitted vertex.</param>
		/// <param name="y">An Y-Offset applied to the position of each emitted vertex.</param>
		/// <param name="clr">The color value that is applied to each emitted vertex.</param>
		/// <returns>The number of emitted vertices. This values isn't necessarily equal to the emitted arrays length.</returns>
		public int EmitTextVertices(string text, ref VertexC1P3T2[] vertices, float x, float y, ColorRgba clr)
		{
			int len = this.EmitTextVertices(text, ref vertices);
			
			Vector3 offset = new Vector3(x, y, 0);

			for (int i = 0; i < len; i++)
			{
				Vector3.Add(ref vertices[i].Pos, ref offset, out vertices[i].Pos);
				vertices[i].Color = clr;
			}

			return len;
		}
		/// <summary>
		/// Emits a set of vertices based on a text. To render this text, simply use that set of vertices combined with
		/// the Fonts <see cref="Material"/>.
		/// </summary>
		/// <param name="text">The text to render.</param>
		/// <param name="vertices">The set of vertices that is emitted. You can re-use the same array each frame.</param>
		/// <returns>The number of emitted vertices. This values isn't necessarily equal to the emitted arrays length.</returns>
		public int EmitTextVertices(string text, ref VertexC1P3T2[] vertices)
		{
			int len = text.Length * 6;
			if (vertices == null || vertices.Length < len) vertices = new VertexC1P3T2[len];
			
			float curOffset = 0.0f;
			GlyphData glyphData;
			Rect uvRect;
			float glyphXOff;
			float glyphYOff;
			float glyphXAdv;
			for (int i = 0; i < text.Length; i++)
			{
				this.ProcessTextAdv(text, i, out glyphData, out uvRect, out glyphXAdv, out glyphXOff, out glyphYOff);

				Vector2 glyphPos;
				glyphPos.X = MathF.Round(curOffset + glyphXOff);
				glyphPos.Y = MathF.Round(glyphYOff);

				vertices[i * 6 + 0].Pos.X = glyphPos.X;
				vertices[i * 6 + 0].Pos.Y = glyphPos.Y;
				vertices[i * 6 + 0].Pos.Z = 0.0f;
				vertices[i * 6 + 0].TexCoord = uvRect.TopLeft;
				vertices[i * 6 + 0].Color = ColorRgba.White;
							 
				vertices[i * 6 + 1].Pos.X = glyphPos.X + glyphData.width;
				vertices[i * 6 + 1].Pos.Y = glyphPos.Y;
				vertices[i * 6 + 1].Pos.Z = 0.0f;
				vertices[i * 6 + 1].TexCoord = uvRect.TopRight;
				vertices[i * 6 + 1].Color = ColorRgba.White;
							 
				vertices[i * 6 + 2].Pos.X = glyphPos.X + glyphData.width;
				vertices[i * 6 + 2].Pos.Y = glyphPos.Y + glyphData.height;
				vertices[i * 6 + 2].Pos.Z = 0.0f;
				vertices[i * 6 + 2].TexCoord = uvRect.BottomRight;
				vertices[i * 6 + 2].Color = ColorRgba.White;
							 
				vertices[i * 6 + 3].Pos.X = glyphPos.X;
				vertices[i * 6 + 3].Pos.Y = glyphPos.Y;
				vertices[i * 6 + 3].Pos.Z = 0.0f;
				vertices[i * 6 + 3].TexCoord = uvRect.TopLeft;
				vertices[i * 6 + 3].Color = ColorRgba.White;
							 
				vertices[i * 6 + 4].Pos.X = glyphPos.X + glyphData.width;
				vertices[i * 6 + 4].Pos.Y = glyphPos.Y + glyphData.height;
				vertices[i * 6 + 4].Pos.Z = 0.0f;
				vertices[i * 6 + 4].TexCoord = uvRect.BottomRight;
				vertices[i * 6 + 4].Color = ColorRgba.White;
							 
				vertices[i * 6 + 5].Pos.X = glyphPos.X;
				vertices[i * 6 + 5].Pos.Y = glyphPos.Y + glyphData.height;
				vertices[i * 6 + 5].Pos.Z = 0.0f;
				vertices[i * 6 + 5].TexCoord = uvRect.BottomLeft;
				vertices[i * 6 + 5].Color = ColorRgba.White;

				curOffset += glyphXAdv;
			}

			return len;
		}
		
		/// <summary>
		/// Renders a text to the specified target Image.
		/// </summary>
		/// <param name="text"></param>
		/// <param name="target"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		public void RenderToBitmap(string text, Image target, float x = 0.0f, float y = 0.0f)
		{
			this.RenderToBitmap(text, target, x, y, ColorRgba.White);
		}
		/// <summary>
		/// Renders a text to the specified target Image.
		/// </summary>
		/// <param name="text"></param>
		/// <param name="target"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="clr"></param>
		public void RenderToBitmap(string text, Image target, float x, float y, ColorRgba clr)
		{
#if !__ANDROID__
			if (this.fontAtlas == null || this.fontAtlas.Texture == null)
				return;

			Bitmap pixelData = this.fontAtlas.Texture.RetrievePixelData().ToBitmap();
			if (pixelData == null)
				return;

			using (Graphics g = Graphics.FromImage(target))
			{
				g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
				g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

				float curOffset = 0.0f;
				GlyphData glyphData;
				Rect uvRect;
				float glyphXOff;
				float glyphYOff;
				float glyphXAdv;
				var attrib = new System.Drawing.Imaging.ImageAttributes();
				attrib.SetColorMatrix(new System.Drawing.Imaging.ColorMatrix(new[] {
					new[] {clr.R / 255.0f,					0.0f, 			0.0f, 			0.0f, 0.0f},
					new[] {0.0f,			clr.G / 255.0f, 0.0f, 			0.0f, 			0.0f, 0.0f},
					new[] {0.0f,			0.0f, 			clr.B / 255.0f, 0.0f, 			0.0f, 0.0f},
					new[] {0.0f, 			0.0f, 			0.0f, 			clr.A / 255.0f, 0.0f, 0.0f},
					new[] {0.0f, 			0.0f, 			0.0f, 			0.0f, 			0.0f, 0.0f},
					new[] {0.0f, 			0.0f, 			0.0f, 			0.0f, 			0.0f, 0.0f} }));
				for (int i = 0; i < text.Length; i++)
				{
					this.ProcessTextAdv(text, i, out glyphData, out uvRect, out glyphXAdv, out glyphXOff, out glyphYOff);
					Vector2 dataCoord = uvRect.Pos * new Vector2(pixelData.Width, pixelData.Height) / this.fontAtlas.Texture.UVRatio;

					if (clr == ColorRgba.White)
					{
						g.DrawImage(pixelData,
							new Rectangle(MathF.RoundToInt(x + curOffset + glyphXOff), MathF.RoundToInt(y), glyphData.width, glyphData.height),
							new Rectangle(MathF.RoundToInt(dataCoord.X), MathF.RoundToInt(dataCoord.Y), glyphData.width, glyphData.height),
							GraphicsUnit.Pixel);
					}
					else
					{
						g.DrawImage(pixelData,
							new Rectangle(MathF.RoundToInt(x + curOffset + glyphXOff), MathF.RoundToInt(y), glyphData.width, glyphData.height),
							dataCoord.X, dataCoord.Y, glyphData.width, glyphData.height,
							GraphicsUnit.Pixel,
							attrib);
					}

					curOffset += glyphXAdv;
				}
			}
#endif

		}
		/// <summary>
		/// Renders a text to the specified target <see cref="Duality.Resources.Pixmap"/> <see cref="Duality.Resources.Pixmap.Layer"/>.
		/// </summary>
		/// <param name="text"></param>
		/// <param name="target"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		public void RenderToBitmap(string text, Pixmap.Layer target, float x = 0.0f, float y = 0.0f)
		{
			this.RenderToBitmap(text, target, x, y, ColorRgba.White);
		}
		/// <summary>
		/// Renders a text to the specified target <see cref="Duality.Resources.Pixmap"/> <see cref="Duality.Resources.Pixmap.Layer"/>.
		/// </summary>
		/// <param name="text"></param>
		/// <param name="target"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="clr"></param>
		public void RenderToBitmap(string text, Pixmap.Layer target, float x, float y, ColorRgba clr)
		{
			if (this.fontAtlas == null || this.fontAtlas.Texture == null)
				return;

			Pixmap.Layer pixelData = this.fontAtlas.Texture.RetrievePixelData();
			if (pixelData == null)
				return;

			float curOffset = 0.0f;
			GlyphData glyphData;
			Rect uvRect;
			float glyphXOff;
			float glyphYOff;
			float glyphXAdv;
			for (int i = 0; i < text.Length; i++)
			{
				this.ProcessTextAdv(text, i, out glyphData, out uvRect, out glyphXAdv, out glyphXOff, out glyphYOff);
				Vector2 dataCoord = uvRect.Pos * new Vector2(pixelData.Width, pixelData.Height) / this.fontAtlas.Texture.UVRatio;
				
				pixelData.DrawOnto(target, 
					BlendMode.Alpha, 
					MathF.RoundToInt(x + curOffset + glyphXOff), 
					MathF.RoundToInt(y),
					glyphData.width, 
					glyphData.height,
					MathF.RoundToInt(dataCoord.X), 
					MathF.RoundToInt(dataCoord.Y), 
					clr);

				curOffset += glyphXAdv;
			}
		}
		/// <summary>
		/// Measures the size of a text rendered using this Font.
		/// </summary>
		/// <param name="text">The text to measure.</param>
		/// <returns>The size of the measured text.</returns>
		public Vector2 MeasureText(string text)
		{
			Vector2 textSize = Vector2.Zero;

			float curOffset = 0.0f;
			GlyphData glyphData;
			Rect uvRect;
			float glyphXOff;
			float glyphYOff;
			float glyphXAdv;
			for (int i = 0; i < text.Length; i++)
			{
				this.ProcessTextAdv(text, i, out glyphData, out uvRect, out glyphXAdv, out glyphXOff, out glyphYOff);

				textSize.X = Math.Max(textSize.X, curOffset + glyphXAdv - this.spacing);
				textSize.Y = Math.Max(textSize.Y, glyphData.height);

				curOffset += glyphXAdv;
			}

			textSize.X = MathF.Round(textSize.X);
			textSize.Y = MathF.Round(textSize.Y);
			return textSize;
		}
		/// <summary>
		/// Measures the size of a multiline text rendered using this Font.
		/// </summary>
		/// <param name="text">The text to measure.</param>
		/// <returns>The size of the measured text.</returns>
		public Vector2 MeasureText(string[] text)
		{
			Vector2 textSize = Vector2.Zero;
			if (text == null) return textSize;

			for (int i = 0; i < text.Length; i++)
			{
				Vector2 lineSize = this.MeasureText(text[i]);
				textSize.X = MathF.Max(textSize.X, lineSize.X);
				textSize.Y += i == 0 ? this.Height : this.LineSpacing;
			}

			return textSize;
		}
		/// <summary>
		/// Returns a text that is cropped to fit a maximum width using this Font.
		/// </summary>
		/// <param name="text">The original text.</param>
		/// <param name="maxWidth">The maximum width it may occupy.</param>
		/// <param name="fitMode">The mode by which the text fitting algorithm operates.</param>
		/// <returns></returns>
		public string FitText(string text, float maxWidth, FitTextMode fitMode = FitTextMode.ByChar)
		{
			Vector2 textSize = Vector2.Zero;

			float curOffset = 0.0f;
			GlyphData glyphData;
			Rect uvRect;
			float glyphXOff;
			float glyphYOff;
			float glyphXAdv;
			int lastValidLength = 0;
			for (int i = 0; i < text.Length; i++)
			{
				this.ProcessTextAdv(text, i, out glyphData, out uvRect, out glyphXAdv, out glyphXOff, out glyphYOff);

				textSize.X = Math.Max(textSize.X, curOffset + glyphXAdv);
				textSize.Y = Math.Max(textSize.Y, glyphData.height);

				if (textSize.X > maxWidth) return lastValidLength > 0 ? text.Substring(0, lastValidLength) : "";

				if (fitMode == FitTextMode.ByChar)
					lastValidLength = i;
				else if (text[i] == ' ')
					lastValidLength = fitMode == FitTextMode.ByWordLeadingSpace ? i : i + 1;

				curOffset += glyphXAdv;
			}

			return text;
		}
		/// <summary>
		/// Measures position and size of a specific glyph inside a text.
		/// </summary>
		/// <param name="text">The text that contains the glyph to measure.</param>
		/// <param name="index">The index of the glyph to measure.</param>
		/// <returns>A rectangle that describes the specified glyphs position and size.</returns>
		public Rect MeasureTextGlyph(string text, int index)
		{
			float curOffset = 0.0f;
			GlyphData glyphData;
			Rect uvRect;
			float glyphXOff;
			float glyphYOff;
			float glyphXAdv;
			for (int i = 0; i < text.Length; i++)
			{
				this.ProcessTextAdv(text, i, out glyphData, out uvRect, out glyphXAdv, out glyphXOff, out glyphYOff);

				if (i == index) return new Rect(curOffset + glyphXOff, 0, glyphData.width, glyphData.height);

				curOffset += glyphXAdv;
			}

			return new Rect();
		}
		/// <summary>
		/// Returns the index of the glyph that is located at a certain location within a text.
		/// </summary>
		/// <param name="text">The text from which to pick a glyph.</param>
		/// <param name="x">X-Coordinate of the position where to look for a glyph.</param>
		/// <param name="y">Y-Coordinate of the position where to look for a glyph.</param>
		/// <returns>The index of the glyph that is located at the specified position.</returns>
		public int PickTextGlyph(string text, float x, float y)
		{
			float curOffset = 0.0f;
			GlyphData glyphData;
			Rect uvRect;
			Rect glyphRect;
			float glyphXOff;
			float glyphYOff;
			float glyphXAdv;
			for (int i = 0; i < text.Length; i++)
			{
				this.ProcessTextAdv(text, i, out glyphData, out uvRect, out glyphXAdv, out glyphXOff, out glyphYOff);

				glyphRect = new Rect(curOffset + glyphXOff, 0, glyphData.width, glyphData.height);
				if (glyphRect.Contains(x, y)) return i;

				curOffset += glyphXAdv;
			}

			return -1;
		}

		private void ProcessTextAdv(string text, int index, out GlyphData glyphData, out Rect uvRect, out float glyphXAdv, out float glyphXOff, out float glyphYOff)
		{
			if (this.face == null)
				throw new InvalidOperationException("this.face is null for font " + this.familyName);

			var charIndex = this.face.GetCharIndex(text[index]);
			this.face.LoadGlyph(charIndex, LoadFlags.Render | LoadFlags.ForceAutohint, LoadTarget.Normal);

			glyphData = new GlyphData
			{
				width = this.face.Glyph.Metrics.Width.ToInt32(),
				height = this.face.Glyph.Metrics.Height.ToInt32()
			};

			if (this.fontAtlas == null)
				this.fontAtlas = new FontAtlas(128, this.renderMode, this.IsPixelGridAligned);

			if (text[index] == ' ')
			{
				uvRect = Rect.Empty;
			}
			else
			{
				uvRect = this.fontAtlas.GetTextureRect((int) charIndex);
				if (uvRect == Rect.Empty)
					uvRect = this.fontAtlas.Insert((int) charIndex, this.face);
			}

			// what should glyphXOff actually be?
			glyphXOff = 0;
			glyphYOff = this.face.Size.Metrics.Ascender.ToInt32() - this.face.Glyph.GetGlyph().ToBitmapGlyph().Top;

			if (this.kerning && !this.monospace && index > 0 && this.face.HasKerning)
			{
				var previousIndex = this.face.GetCharIndex(text[index - 1]);
				var kerning = this.face.GetKerning(charIndex, previousIndex, KerningMode.Default);
				glyphXAdv = kerning.X.ToInt32();
			}
			else
			{
				glyphXAdv = this.face.Glyph.Metrics.HorizontalAdvance.ToSingle() + this.spacing;
			}
		}

		protected override void OnLoaded()
		{
			this.ReloadData();
			base.OnLoaded();
		}

		private void LoadFont()
		{
			if (fontCache.ContainsKey(this.familyName)) return;

			using (var stream = this.IsEmbeddedFont ? LoadFontFromEmbeddedResource() : LoadFontFromDisk())
			using (var memoryStream = new MemoryStream())
			{
				stream.CopyTo(memoryStream);
				fontCache.Add(this.familyName, memoryStream.ToArray());
				memoryStream.Dispose();
			}
		}

		protected override void OnDisposing(bool manually)
		{
			base.OnDisposing(manually);
			this.ReleaseResources();
		}
		protected override void OnCopyTo(Resource r, Duality.Cloning.CloneProvider provider)
		{
			base.OnCopyTo(r, provider);
			Font c = r as Font;
			c.familyName = this.familyName;
			c.size = this.size;
			c.style = this.style;
			c.renderMode = this.renderMode;
			c.monospace = this.monospace;
			c.kerning = this.kerning;
			c.spacing = this.spacing;
			c.ReloadData();
		}

		private Stream LoadFontFromDisk()
		{
			Stream stream = null;
#if !__ANDROID__
			var library = new Library();
			var fontPath = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);
			foreach (var file in Directory.EnumerateFiles(fontPath, "*.ttf", SearchOption.AllDirectories))
			{
				var face = new Face(library, file);
				if (face.FamilyName == this.familyName)
				{
					face.Dispose();
					library.Dispose();
					return FileHelper.OpenRead(file);
				}

				face.Dispose();
			}
			library.Dispose();
#else
			stream = FileHelper.OpenRead(System.IO.Path.Combine(@"Data\Fonts", "DroidSansFallback.ttf"));
			if(stream == null)
				Log.Game.WriteError("Couldn't open DroidSansFallback.ttf");
#endif
			return stream;
		}

		private Stream LoadFontFromEmbeddedResource()
		{
			var assembly = typeof(Font).Assembly;
			var font = assembly.GetManifestResourceStream(string.Format("{0}.{1}.ttf", assembly.GetShortAssemblyName(), this.familyName));
			if(font== null)
				Log.Game.WriteError("Couldn't load embedded font " + familyName);

			return font;
		}
	}
}
