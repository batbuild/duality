﻿using System;
using GLPixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;
#if ! __ANDROID__
using BitmapPixelFormat = System.Drawing.Imaging.PixelFormat;
#endif
using Duality.Editor;
using Duality.Properties;
using Duality.Drawing;
using Duality.Helpers;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Duality.Resources
{
	/// <summary>
	/// A Texture refers to pixel data stored in video memory
	/// </summary>
	/// <seealso cref="Duality.Resources.Pixmap"/>
	/// <seealso cref="Duality.Resources.RenderTarget"/>
	[Serializable]
	[ExplicitResourceReference(typeof(Pixmap))]
#if ! __ANDROID__
	[EditorHintCategory(typeof(CoreRes), CoreResNames.CategoryGraphics)]
	[EditorHintImage(typeof(CoreRes), CoreResNames.ImageTexture)]

#endif
	public class Texture : Resource
	{
		/// <summary>
		/// A Texture resources file extension.
		/// </summary>
		public new const string FileExt = ".Texture" + Resource.FileExt;

		private const int ProcessedPixmapLayerIndex = 1;

		/// <summary>
		/// [GET] A plain white 1x1 Texture. Can be used as a dummy.
		/// </summary>
		public static ContentRef<Texture> White				{ get; private set; }
		/// <summary>
		/// [GET] A 256x256 black and white checkerboard texture.
		/// </summary>
		public static ContentRef<Texture> Checkerboard		{ get; private set; }

		internal static void InitDefaultContent()
		{
			string contentPath;
			string extension = string.Empty;

#if  __ANDROID__
			contentPath = "Data\\Default\\Texture\\";
			extension = FileExt;
#else
			contentPath = ContentProvider.VirtualContentPath + "Texture:";
#endif
			 string ContentPath_White = contentPath + "White" + extension;
			 string ContentPath_Checkerboard = contentPath + "Checkerboard" + extension;

			 ContentProvider.AddContent(ContentPath_White, new Texture(Pixmap.White, filterMin: TextureMinFilter.Linear, keepPixmapDataResident: true));
			 ContentProvider.AddContent(ContentPath_Checkerboard, new Texture(
				 Pixmap.Checkerboard,
				 SizeMode.Default,
				 TextureMagFilter.Nearest,
				 TextureMinFilter.Nearest,
				 TextureWrapMode.Repeat,
				 TextureWrapMode.Repeat,
				 keepPixmapDataResident: true));

			White				= ContentProvider.RequestContent<Texture>(ContentPath_White);
			Checkerboard		= ContentProvider.RequestContent<Texture>(ContentPath_Checkerboard);
		}


		/// <summary>
		/// Defines how to handle pixel data without power-of-two dimensions.
		/// </summary>
		public enum SizeMode
		{
			/// <summary>
			/// Enlarges the images dimensions without scaling the image, leaving
			/// the new space empty. Texture coordinates are automatically adjusted in
			/// order to display the image correctly. This preserves the images full
			/// quality but prevents tiling, if not power-of-two anyway.
			/// </summary>
			Enlarge,
			/// <summary>
			/// Stretches the image to fit power-of-two dimensions and downscales it
			/// again when displaying. This might blur the image slightly but allows
			/// tiling it.
			/// </summary>
			Stretch,
			/// <summary>
			/// The images dimensions are not affected, as OpenGL uses an actual 
			/// non-power-of-two texture. However, this might be unsupported on older hardware.
			/// </summary>
			NonPowerOfTwo,

			/// <summary>
			/// The default behaviour. Equals <see cref="Enlarge"/>.
			/// </summary>
			Default = NonPowerOfTwo
		}


		/// <summary>
		/// Refers to a null reference Texture.
		/// </summary>
		/// <seealso cref="ContentRef{T}.Null"/>
		public static readonly ContentRef<Texture> None	= ContentRef<Texture>.Null;

		private	static	bool			initialized		= false;
		private	static	int				activeTexUnit	= 0;
		private	static	Texture[]		curBound		= null;
		private	static	TextureUnit[]	texUnits		= null;
		private	static	float			maxAnisoLevel	= 0;

		/// <summary>
		/// [GET] The currently bound primary Texture.
		/// </summary>
		public static ContentRef<Texture> BoundTexPrimary
		{
			get { return new ContentRef<Texture>(curBound[0]); }
		}
		/// <summary>
		/// [GET] The currently bound secondary Texture
		/// </summary>
		public static ContentRef<Texture> BoundTexSecondary
		{
			get { return new ContentRef<Texture>(curBound[1]); }
		}
		/// <summary>
		/// [GET] The currently bound tertiary Texture
		/// </summary>
		public static ContentRef<Texture> BoundTexTertiary
		{
			get { return new ContentRef<Texture>(curBound[2]); }
		}
		/// <summary>
		/// [GET] The currently bound quartary Texture
		/// </summary>
		public static ContentRef<Texture> BoundTexQuartary
		{
			get { return new ContentRef<Texture>(curBound[3]); }
		}
		/// <summary>
		/// [GET] All Textures that are currently bound
		/// </summary>
		public static ContentRef<Texture>[] BoundTex
		{
			get 
			{ 
				ContentRef<Texture>[] result = new ContentRef<Texture>[curBound.Length];
				for (int i = 0; i < result.Length; i++)
				{
					result[i] = new ContentRef<Texture>(curBound[i]);
				}
				return result;
			}
		}

		private static void Init()
		{
			if (initialized) return;

			GL.GetFloat((GetPName)ExtTextureFilterAnisotropic.MaxTextureMaxAnisotropyExt, out maxAnisoLevel);
			
			int numTexUnits;
			GL.GetInteger(GetPName.MaxTextureImageUnits, out numTexUnits);
			texUnits = new TextureUnit[numTexUnits];
			curBound = new Texture[numTexUnits];

			for (int i = 0; i < numTexUnits; i++)
			{
				texUnits[i] = (TextureUnit)((int)TextureUnit.Texture0 + i);
			}

			initialized = true;
		}
		/// <summary>
		/// Binds the given Texture to a texture unit in order to use it for rendering.
		/// </summary>
		/// <param name="tex">The Texture to bind.</param>
		/// <param name="texUnit">The texture unit where the Texture will be bound to.</param>
		public static void Bind(ContentRef<Texture> tex, int texUnit = 0)
		{
			if (!initialized) Init();

			Texture texRes = tex.IsExplicitNull ? null : (tex.Res ?? Checkerboard.Res);
			if (curBound[texUnit] == texRes) return;
			if (activeTexUnit != texUnit) GL.ActiveTexture(texUnits[texUnit]);
			activeTexUnit = texUnit;

			if (texRes == null)
			{
				GL.BindTexture(TextureTarget.Texture2D, 0);
				curBound[texUnit] = null;
			}
			else
			{
				if (texRes.glTexId == 0)	throw new ArgumentException(string.Format("Specified texture '{0}' has no valid OpenGL texture Id! Maybe it hasn't been loaded / initialized properly?", texRes.Path), "tex");
				if (texRes.Disposed)		throw new ArgumentException(string.Format("Specified texture '{0}' has already been deleted!", texRes.Path), "tex");
					
				GL.BindTexture(TextureTarget.Texture2D, texRes.glTexId);
				curBound[texUnit] = texRes;
			}
		}
		/// <summary>
		/// Resets all Texture bindings to texture units beginning at a certain index.
		/// </summary>
		/// <param name="beginAtIndex">The first texture unit index from which on all bindings will be cleared.</param>
		public static void ResetBinding(int beginAtIndex = 0)
		{
			if (!initialized) Init();
			for (int i = beginAtIndex; i < texUnits.Length; i++)
			{
				Bind(None, i);
			}
		}

		/// <summary>
		/// Creates a new Texture Resource based on the specified Pixmap, saves it and returns a reference to it.
		/// </summary>
		/// <param name="pixmap"></param>
		/// <returns></returns>
		public static ContentRef<Texture> CreateFromPixmap(ContentRef<Pixmap> pixmap)
		{
			string texPath = PathHelper.GetFreePath(pixmap.FullName, FileExt);
			Texture tex = new Texture(pixmap);
			tex.Save(texPath);
			return tex;
		}

		/// <summary>
		/// For most textures, once they're uploaded to OpenGL, we can safely dispose the pixel data, but there are cases,
		/// such as with non-pregenerated fonts, where we want to keep the data around. This flag is false by default but can be set
		/// to true for those special cases.
		/// </summary>
		private bool keepPixmapDataResident;
		/// <summary>
		/// If set to false, loads pixel data directly from the associated pixmap. Set this to true if the intention is to load pixel
		/// data from some other source, using the LoadData(IntPtr...) override
		/// </summary>
		private bool useExternalPixelData;
		
		private	ContentRef<Pixmap>		basePixmap	= ContentRef<Pixmap>.Null;
		private	Vector2					size		= Vector2.Zero;
		private	SizeMode				texSizeMode	= SizeMode.Default;
		private	TextureMagFilter		filterMag	= TextureMagFilter.Linear;
		private	TextureMinFilter		filterMin	= TextureMinFilter.LinearMipmapLinear;
		private	TextureWrapMode			wrapX		= TextureWrapMode.ClampToEdge;
		private	TextureWrapMode			wrapY		= TextureWrapMode.ClampToEdge;
		private	PixelInternalFormat		pixelformat	= PixelInternalFormat.Rgba;
		private	PixelType				pixelType	= PixelType.UnsignedByte;
		private	bool					anisoFilter		= false;
		private bool					premultiplyAlpha = false;
		[NonSerialized]	private	int		pxWidth		= 0;
		[NonSerialized]	private	int		pxHeight	= 0;
		[NonSerialized]	private	int		glTexId		= 0;
		[NonSerialized]	private	float	pxDiameter	= 0.0f;
		[NonSerialized]	private	int		texWidth	= 0;
		[NonSerialized]	private	int		texHeight	= 0;
		[NonSerialized]	private	Vector2	uvRatio		= new Vector2(1.0f, 1.0f);
		[NonSerialized] private	bool	needsReload	= true;
		[NonSerialized] private	Rect[]	atlas		= null;

		private bool compressed;

		/// <summary>
		/// [GET] The Textures internal texel width
		/// </summary>
		[EditorHintFlags(MemberFlags.Invisible)]
		public int TexelWidth
		{
			get { return this.texWidth; }
		}	//	G
		/// <summary>
		/// [GET] The Textures internal texel height
		/// </summary>
		[EditorHintFlags(MemberFlags.Invisible)]
		public int TexelHeight
		{
			get { return this.texHeight; }
		}	//	G
		/// <summary>
		/// [GET] The Textures original pixel width
		/// </summary>
		[EditorHintFlags(MemberFlags.Invisible)]
		public int PixelWidth
		{
			get { return this.pxWidth; }
		}	//	G
		/// <summary>
		/// [GET] The Textures original pixel height
		/// </summary>
		[EditorHintFlags(MemberFlags.Invisible)]
		public int PixelHeight
		{
			get { return this.pxHeight; }
		}	//	G
		/// <summary>
		/// [GET] The Textures internal id value. You shouldn't need to use this value normally.
		/// </summary>
		internal int OglTexId
		{
			get { return this.glTexId; }
		}	//	G
		/// <summary>
		/// [GET] UV (Texture) coordinates for the Textures lower right
		/// </summary>
		[EditorHintFlags(MemberFlags.Invisible)]
		public Vector2 UVRatio
		{
			get { return this.uvRatio; }
		}	//	G
		/// <summary>
		/// [GET] Returns whether or not the texture uses mipmaps.
		/// </summary>
		[EditorHintFlags(MemberFlags.Invisible)]
		public bool HasMipmaps
		{
			get { return 
				this.filterMin == TextureMinFilter.LinearMipmapLinear ||
				this.filterMin == TextureMinFilter.LinearMipmapNearest ||
				this.filterMin == TextureMinFilter.NearestMipmapLinear ||
				this.filterMin == TextureMinFilter.NearestMipmapNearest; }
		}	//	G
		/// <summary>
		/// Indicates that the textures parameters have been changed in a way that will make it
		/// necessary to reload its data before using it next time.
		/// </summary>
		[EditorHintFlags(MemberFlags.Invisible)]
		public bool NeedsReload
		{
			get { return this.needsReload; }
		} //  G
		/// <summary>
		/// Enable for DXT compression
		/// </summary>
		public bool Compressed
		{
			get { return compressed; }
			set { compressed = value; }
		}
		/// <summary>
		/// [GET / SET] The Textures size. Readonly, when created from a <see cref="BasePixmap"/>.
		/// </summary>
		[EditorHintFlags(MemberFlags.AffectsOthers)]
		[EditorHintRange(0, int.MaxValue)]
		[EditorHintIncrement(1)]
		[EditorHintDecimalPlaces(0)]
		public Vector2 Size
		{
			get { return this.size; }
			set
			{
				if (this.basePixmap.IsExplicitNull && this.size != value)
				{
					this.AdjustSize(value.X, value.Y);
					this.needsReload = true;
				}
			}
		}						//	GS
		/// <summary>
		/// [GET / SET] The Textures magnifying filter
		/// </summary>
		public TextureMagFilter FilterMag
		{
			get { return this.filterMag; }
			set { if (this.filterMag != value) { this.filterMag = value; this.needsReload = true; } }
		}		//	GS
		/// <summary>
		/// [GET / SET] The Textures minifying filter
		/// </summary>
		public TextureMinFilter FilterMin
		{
			get { return this.filterMin; }
			set { if (this.filterMin != value) { this.filterMin = value; this.needsReload = true; } }
		}		//	GS
		/// <summary>
		/// [GET / SET] Specifies whether this texture uses anisotropic filtering.
		/// </summary>
		public bool AnisotropicFilter
		{
			get { return this.anisoFilter; }
			set { if (this.anisoFilter != value) { this.anisoFilter = value; this.needsReload = true; } }
		}			//	GS
		/// <summary>
		/// [GET / SET] The Textures horizontal wrap mode
		/// </summary>
		public TextureWrapMode WrapX
		{
			get { return this.wrapX; }
			set { if (this.wrapX != value) { this.wrapX = value; this.needsReload = true; } }
		}				//	GS
		/// <summary>
		/// [GET / SET] The Textures vertical wrap mode
		/// </summary>
		public TextureWrapMode WrapY
		{
			get { return this.wrapY; }
			set { if (this.wrapY != value) { this.wrapY = value; this.needsReload = true; } }
		}				//	GS
		/// <summary>
		/// [GET / SET] The Textures pixel format
		/// </summary>
		public PixelInternalFormat PixelFormat
		{
			get { return this.pixelformat; }
			set { if (this.pixelformat != value) { this.pixelformat = value; this.needsReload = true; } }
		}	//	GS
		/// <summary>
		/// [GET / SET] Handles how the Textures base Pixmap is adjusted in order to fit GPU texture size requirements (Power of Two dimensions)
		/// </summary>
		[EditorHintFlags(MemberFlags.AffectsOthers)]
		public SizeMode TexSizeMode
		{
			get { return this.texSizeMode; }
			set 
			{ 
				if (this.texSizeMode != value) 
				{ 
					this.texSizeMode = value; 
					this.AdjustSize(this.size.X, this.size.Y);
					this.needsReload = true;
				}
			}
		}				//	GS
		/// <summary>
		/// [GET / SET] Reference to a Pixmap that contains the pixel data that is or has been uploaded to the Texture
		/// </summary>
		[EditorHintFlags(MemberFlags.AffectsOthers)]
		public ContentRef<Pixmap> BasePixmap
		{
			get { return this.basePixmap; }
			set { if (this.basePixmap.Res != value.Res) { this.basePixmap = value; this.needsReload = true; } }
		}		//	GS

		/// <summary>
		/// [GET / SET] Gets/sets a value indicating whether this texture should be premultiplied by it's alpha value.
		/// </summary>
		public bool PremultiplyAlpha
		{
			get { return this.premultiplyAlpha; }
			set
			{
				this.premultiplyAlpha = value;
				if (this.basePixmap.Res != null)
				{
					if(value)
						this.basePixmap.Res.PremultiplyPixelData();
					else
						this.basePixmap.Res.ColourTransparentPixels();
				}

				this.needsReload = true;
			}
		}
		
		/// <summary>
		/// [GET / SET] If set to true, loads pixel data directly from the associated pixmap. Set this to false if the intention is to load pixel
		/// data from some other source, using the LoadData(IntPtr...) override
		/// </summary>
		[EditorHintFlags(MemberFlags.Invisible)]
		public bool UseExternalPixelData
		{
			get { return useExternalPixelData; }
			set { useExternalPixelData = value; }
		}

		/// <summary>
		/// Sets up a new, uninitialized Texture.
		/// </summary>
		public Texture() : this(0, 0) {}
		/// <summary>
		/// Creates a new Texture based on a <see cref="Duality.Resources.Pixmap"/>.
		/// </summary>
		/// <param name="basePixmap">The <see cref="Duality.Resources.Pixmap"/> to use as source for pixel data.</param>
		/// <param name="sizeMode">Specifies behaviour in case the source data has non-power-of-two dimensions.</param>
		/// <param name="filterMag">The OpenGL filter mode for drawing the Texture bigger than it is.</param>
		/// <param name="filterMin">The OpenGL fitler mode for drawing the Texture smaller than it is.</param>
		/// <param name="wrapX">The OpenGL wrap mode on the texel x axis.</param>
		/// <param name="wrapY">The OpenGL wrap mode on the texel y axis.</param>
		/// <param name="format">The format in which OpenGL stores the pixel data.</param>
		public Texture(ContentRef<Pixmap> basePixmap, 
			SizeMode sizeMode			= SizeMode.Default, 
			TextureMagFilter filterMag	= TextureMagFilter.Linear, 
			TextureMinFilter filterMin	= TextureMinFilter.LinearMipmapLinear,
			TextureWrapMode wrapX		= TextureWrapMode.ClampToEdge,
			TextureWrapMode wrapY		= TextureWrapMode.ClampToEdge,
			PixelInternalFormat format	= PixelInternalFormat.Rgba,
			PixelType pixelType			= PixelType.UnsignedByte,
			bool keepPixmapDataResident	= false)
		{
			this.filterMag = filterMag;
			this.filterMin = filterMin;
			this.wrapX = wrapX;
			this.wrapY = wrapY;
			this.pixelformat = format;
			this.pixelType = pixelType;
			this.keepPixmapDataResident = keepPixmapDataResident;
			this.LoadData(basePixmap, sizeMode);
		}
		/// <summary>
		/// Creates a new empty Texture with the specified size.
		/// </summary>
		/// <param name="width">The Textures width.</param>
		/// <param name="height">The Textures height</param>
		/// <param name="sizeMode">Specifies behaviour in case the specified size has non-power-of-two dimensions.</param>
		/// <param name="filterMag">The OpenGL filter mode for drawing the Texture bigger than it is.</param>
		/// <param name="filterMin">The OpenGL fitler mode for drawing the Texture smaller than it is.</param>
		/// <param name="wrapX">The OpenGL wrap mode on the texel x axis.</param>
		/// <param name="wrapY">The OpenGL wrap mode on the texel y axis.</param>
		/// <param name="format">The format in which OpenGL stores the pixel data.</param>
		public Texture(int width, int height, 
			SizeMode sizeMode			= SizeMode.Default, 
			TextureMagFilter filterMag	= TextureMagFilter.Linear, 
			TextureMinFilter filterMin	= TextureMinFilter.LinearMipmapLinear,
			TextureWrapMode wrapX		= TextureWrapMode.ClampToEdge,
			TextureWrapMode wrapY		= TextureWrapMode.ClampToEdge,
			PixelInternalFormat format	= PixelInternalFormat.Rgba,
			PixelType pixelType			= PixelType.UnsignedByte)
		{
			this.filterMag = filterMag;
			this.filterMin = filterMin;
			this.wrapX = wrapX;
			this.wrapY = wrapY;
			this.pixelformat = format;
			this.pixelType = pixelType;
			this.texSizeMode = sizeMode;
			this.AdjustSize(width, height);
			this.SetupOpenGLRes();
		}

		/// <summary>
		/// Reloads this Textures pixel data. If the referred <see cref="Duality.Resources.Pixmap"/> has been modified,
		/// changes will now be visible.
		/// </summary>
		public void ReloadData()
		{
			this.LoadData(this.basePixmap, this.texSizeMode);
		}
		/// <summary>
		/// Loads the specified <see cref="Duality.Resources.Pixmap">Pixmaps</see> pixel data.
		/// </summary>
		/// <param name="basePixmap">The <see cref="Duality.Resources.Pixmap"/> that is used as pixel data source.</param>
		public void LoadData(ContentRef<Pixmap> basePixmap)
		{
			this.LoadData(basePixmap, this.texSizeMode);
		}

		/// <summary>
		/// Loads the specified <see cref="Duality.Resources.Pixmap">Pixmaps</see> pixel data.
		/// </summary>
		/// <param name="basePixmap">The <see cref="Duality.Resources.Pixmap"/> that is used as pixel data source.</param>
		/// <param name="sizeMode">Specifies behaviour in case the source data has non-power-of-two dimensions.</param>
		public void LoadData(ContentRef<Pixmap> basePixmap, SizeMode sizeMode)
		{
			DualityApp.GuardSingleThreadState();
			if (this.glTexId == 0) this.glTexId = GL.GenTexture();
			this.needsReload = false;
			this.basePixmap = basePixmap;
			this.texSizeMode = sizeMode;

			int lastTexId;
			GL.GetInteger(GetPName.TextureBinding2D, out lastTexId);
			GL.BindTexture(TextureTarget.Texture2D, this.glTexId);
			
			if (!this.basePixmap.IsExplicitNull)
			{
				Pixmap.Layer pixelData = null;
				Pixmap basePixmapRes = this.basePixmap.IsAvailable ? this.basePixmap.Res : null;
				if (basePixmapRes != null)
				{
					pixelData = basePixmapRes.ProcessedLayer;
					this.atlas = basePixmapRes.Atlas != null ? basePixmapRes.Atlas.ToArray() : null;
				}

				if (pixelData == null)
					pixelData = Pixmap.Checkerboard.Res.MainLayer;

				this.AdjustSize(pixelData.Width, pixelData.Height);
				this.SetupOpenGLRes();
				if (this.texSizeMode != SizeMode.NonPowerOfTwo &&
					(this.pxWidth != this.texWidth || this.pxHeight != this.texHeight))
				{
					if (this.texSizeMode == SizeMode.Enlarge)
					{
						Pixmap.Layer oldData = pixelData;
						pixelData = oldData.CloneResize(this.texWidth, this.texHeight);
						// Fill border pixels manually - that's cheaper than ColorTransparentPixels here.
						oldData.DrawOnto(pixelData, BlendMode.Solid, this.pxWidth, 0, 1, this.pxHeight, this.pxWidth - 1, 0);
						oldData.DrawOnto(pixelData, BlendMode.Solid, 0, this.pxHeight, this.pxWidth, 1, 0, this.pxHeight - 1);
						pixelData.Dispose();
					}
					else
						pixelData = pixelData.CloneRescale(this.texWidth, this.texHeight, Pixmap.FilterMethod.Linear);
				}

				// Load pixel data to video memory
				if (Compressed && pixelData.CompressedData != null)
				{
					var size = ((pixelData.Width + 3) / 4) * ((pixelData.Height + 3) / 4) * 16;
					GL.CompressedTexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.CompressedRgbaS3tcDxt5Ext, pixelData.Width, pixelData.Height, 0, 
						size, pixelData.CompressedData);
				}
				else
				{
					GL.TexImage2D(TextureTarget.Texture2D, 0,
						this.pixelformat, pixelData.Width, pixelData.Height, 0,
						GLPixelFormat.Rgba, PixelType.UnsignedByte,
						pixelData.Data);
				}

				if (HasMipmaps)
				{
					GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
				}
				
				// Adjust atlas to represent UV coordinates
				if (this.atlas != null)
				{
					Vector2 scale;
					scale.X = this.uvRatio.X / this.pxWidth;
					scale.Y = this.uvRatio.Y / this.pxHeight;
					for (int i = 0; i < this.atlas.Length; i++)
					{
						this.atlas[i].X *= scale.X;
						this.atlas[i].W *= scale.X;
						this.atlas[i].Y *= scale.Y;
						this.atlas[i].H *= scale.Y;
					}
				}
			}
			else
			{
				this.atlas = null;
				this.AdjustSize(this.size.X, this.size.Y);
				this.SetupOpenGLRes();
			}

			GL.BindTexture(TextureTarget.Texture2D, lastTexId);

			if (this.keepPixmapDataResident == false && basePixmap.IsLoaded)
				basePixmap.Res.Dispose();
		}

		public void LoadData(IntPtr data, int width, int height)
		{
			DualityApp.GuardSingleThreadState();
			if (this.glTexId == 0) this.glTexId = GL.GenTexture();

			int lastTexId;
			GL.GetInteger(GetPName.TextureBinding2D, out lastTexId);
			GL.BindTexture(TextureTarget.Texture2D, this.glTexId);

			if (data == IntPtr.Zero)
				return;
			AdjustSize(width, height);
			this.SetupOpenGLRes();

			// Load pixel data to video memory
			if (Compressed)
			{
				var imageSize = ((this.PixelWidth + 3) / 4) * ((this.PixelHeight + 3) / 4) * 16;
				GL.CompressedTexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.CompressedRgbaS3tcDxt5Ext, this.PixelWidth,
					this.PixelHeight, 0, imageSize, data);
			}
			else
			{
				GL.TexImage2D(TextureTarget.Texture2D, 0,
					this.pixelformat, this.PixelWidth, this.PixelHeight, 0,
					GLPixelFormat.Rgba, PixelType.UnsignedByte,
					data);
			}

			if (HasMipmaps)
			{
				GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
			}

			// Adjust atlas to represent UV coordinates
			if (this.atlas != null)
			{
				Vector2 scale;
				scale.X = this.uvRatio.X/this.pxWidth;
				scale.Y = this.uvRatio.Y/this.pxHeight;
				for (int i = 0; i < this.atlas.Length; i++)
				{
					this.atlas[i].X *= scale.X;
					this.atlas[i].W *= scale.X;
					this.atlas[i].Y *= scale.Y;
					this.atlas[i].H *= scale.Y;
				}
			}
			
			GL.BindTexture(TextureTarget.Texture2D, lastTexId);
		}

		public void UploadSubImage(int x, int y, int width, int height, byte[] data)
		{
			DualityApp.GuardSingleThreadState();
			Guard.NotNull(data);

			if(width > this.TexelWidth)
				throw new InvalidOperationException("Can't upload texture data as the source region width is larger than the texture");

			if (height > this.TexelHeight)
				throw new InvalidOperationException("Can't upload texture data as the source region height is larger than the texture");

			int lastTexId;
			GL.GetInteger(GetPName.TextureBinding2D, out lastTexId);
			GL.BindTexture(TextureTarget.Texture2D, this.glTexId);

			GL.TexSubImage2D(TextureTarget.Texture2D, 0, x, y, width, height, GLPixelFormat.Rgba, this.pixelType, data);

			GL.BindTexture(TextureTarget.Texture2D, lastTexId);
		}

		/// <summary>
		/// Clears the contents of this texture
		/// </summary>
		public unsafe void Clear()
		{
			DualityApp.GuardSingleThreadState();

			int lastTexId;
			GL.GetInteger(GetPName.TextureBinding2D, out lastTexId);
			GL.BindTexture(TextureTarget.Texture2D, this.glTexId);

			GL.ClearTexImage(this.glTexId, 0, GLPixelFormat.Rgba, this.pixelType, (IntPtr)null);

			GL.BindTexture(TextureTarget.Texture2D, lastTexId);
		}

		/// <summary>
		/// Retrieves the pixel data that is currently stored in video memory.
		/// </summary>
		/// <returns></returns>
		public Pixmap.Layer RetrievePixelData()
		{
			DualityApp.GuardSingleThreadState();

			int lastTexId;
			GL.GetInteger(GetPName.TextureBinding2D, out lastTexId);
			GL.BindTexture(TextureTarget.Texture2D, this.glTexId);
			
			byte[] data = new byte[this.texWidth * this.texHeight * 4];
			GL.GetTexImage(TextureTarget.Texture2D, 0, 
				GLPixelFormat.Rgba, PixelType.UnsignedByte, 
				data);

			GL.BindTexture(TextureTarget.Texture2D, lastTexId);

			Pixmap.Layer result = new Pixmap.Layer();
			result.SetPixelDataRgba(data, this.texWidth, this.texHeight);
			return result;
		}

		/// <summary>
		/// Does a safe (null-checked, clamped) texture <see cref="Duality.Resources.Pixmap.Atlas"/> lookup.
		/// </summary>
		/// <param name="index"></param>
		/// <param name="uv"></param>
		public void LookupAtlas(int index, out Rect uv)
		{
			if (this.atlas == null)
			{
				uv.X = uv.Y = 0.0f;
				uv.W = this.uvRatio.X;
				uv.H = this.uvRatio.Y;
			}
			else
			{
				uv = this.atlas[MathF.Clamp(index, 0, this.atlas.Length - 1)];
			}
		}
		/// <summary>
		/// Does a safe (null-checked, clamped) texture <see cref="Duality.Resources.Pixmap.Atlas"/> lookup.
		/// </summary>
		/// <param name="index"></param>
		public Rect LookupAtlas(int index)
		{
			Rect result;
			this.LookupAtlas(index, out result);
			return result;
		}

		/// <summary>
		/// Processes the specified size based on the Textures <see cref="SizeMode"/>.
		/// </summary>
		/// <param name="width"></param>
		/// <param name="height"></param>
		protected void AdjustSize(float width, float height)
		{
			this.size = new Vector2(MathF.Abs(width), MathF.Abs(height));
			this.pxWidth = MathF.RoundToInt(this.size.X);
			this.pxHeight = MathF.RoundToInt(this.size.Y);
			this.pxDiameter = MathF.Distance(this.pxWidth, this.pxHeight);

			if (this.texSizeMode == SizeMode.NonPowerOfTwo)
			{
				this.texWidth = this.pxWidth;
				this.texHeight = this.pxHeight;
				this.uvRatio = Vector2.One;
			}
			else
			{
				this.texWidth = MathF.NextPowerOfTwo(this.pxWidth);
				this.texHeight = MathF.NextPowerOfTwo(this.pxHeight);
				if (this.pxWidth != this.texWidth || this.pxHeight != this.texHeight)
				{
					if (this.texSizeMode == SizeMode.Enlarge)
					{
						this.uvRatio.X = (float)this.pxWidth / (float)this.texWidth;
						this.uvRatio.Y = (float)this.pxHeight / (float)this.texHeight;
					}
					else
						this.uvRatio = Vector2.One;
				}
				else
					this.uvRatio = Vector2.One;
			}
		}
		/// <summary>
		/// Sets up the Textures OpenGL resources, clearing previously uploaded pixel data.
		/// </summary>
		protected void SetupOpenGLRes()
		{
			DualityApp.GuardSingleThreadState();
			if (!initialized) Init();
			if (this.glTexId == 0) this.glTexId = GL.GenTexture();

			int lastTexId;
			GL.GetInteger(GetPName.TextureBinding2D, out lastTexId);
			if (lastTexId != this.glTexId) GL.BindTexture(TextureTarget.Texture2D, this.glTexId);

			// Set texture parameters
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)this.filterMin);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)this.filterMag);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)this.wrapX);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)this.wrapY);
			 
			// Anisotropic filtering
			GL.TexParameter(TextureTarget.Texture2D, (TextureParameterName) ExtTextureFilterAnisotropic.TextureMaxAnisotropyExt, this.anisoFilter ? maxAnisoLevel : 1.0f);

			// Setup pixel format
			if (this.compressed)
				this.pixelformat = PixelInternalFormat.CompressedRgbaS3tcDxt5Ext;

			// this shouldn't be null but sometimes can be because the pixelType field was introduced quite late and not all assets have been
			// serialized with it.
			if (this.pixelType == 0)
				this.pixelType = PixelType.UnsignedByte;

			GL.TexImage2D(TextureTarget.Texture2D, 0,
				this.pixelformat, this.texWidth, this.texHeight, 0,
				GLPixelFormat.Bgra, this.pixelType, IntPtr.Zero);
			
			if (lastTexId != this.glTexId) GL.BindTexture(TextureTarget.Texture2D, lastTexId);
		}

		protected override void OnLoaded()
		{
			if(this.useExternalPixelData == false)
				this.LoadData(this.basePixmap, this.texSizeMode);

			base.OnLoaded();
		}
		protected override void OnDisposing(bool manually)
		{
			base.OnDisposing(manually);

			// Dispose unmanages Resources
			if (DualityApp.ExecContext != DualityApp.ExecutionContext.Terminated &&
				this.glTexId != 0)
			{
				DualityApp.GuardSingleThreadState();
				GL.DeleteTexture(this.glTexId);
				this.glTexId = 0;
			}

			// Get rid of big data references, so the GC can collect them.
			this.basePixmap.Detach();
		}

		protected override void OnCopyTo(Resource r, Duality.Cloning.CloneProvider provider)
		{
			base.OnCopyTo(r, provider);
			Texture c = r as Texture;
			c.size = this.size;
			c.filterMag = this.filterMag;
			c.filterMin = this.filterMin;
			c.wrapX = this.wrapX;
			c.wrapY = this.wrapY;
			c.pixelformat = this.pixelformat;
			c.premultiplyAlpha = premultiplyAlpha;
			c.compressed = compressed;
			c.LoadData(this.basePixmap, this.texSizeMode);
		}
	}
}
