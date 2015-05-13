using System;

using Duality.Properties;
using Duality.Editor;

using OpenTK.Graphics.OpenGL;


namespace Duality.Resources
{
	/// <summary>
	/// Represents an OpenGL FragmentShader.
	/// </summary>
	[Serializable]
#if ! __ANDROID__
	[EditorHintCategory(typeof(CoreRes), CoreResNames.CategoryGraphics)]
	[EditorHintImage(typeof(CoreRes), CoreResNames.ImageFragmentShader)]
#endif
	public class FragmentShader : AbstractShader
	{
		/// <summary>
		/// A FragmentShader resources file extension.
		/// </summary>
		public new const string FileExt = ".FragmentShader" + Resource.FileExt;

	
		/// <summary>
		/// [GET] A minimal FragmentShader. It performs a texture lookup
		/// and applies vertex-coloring.
		/// </summary>
		public static ContentRef<FragmentShader> Minimal	{ get; private set; }
		/// <summary>
		/// [GET] A FragmentShader designed for picking operations. It uses
		/// the provided texture for alpha output and forwards the incoming RGB color value.
		/// </summary>
		public static ContentRef<FragmentShader> Picking	{ get; private set; }
		/// <summary>
		/// [GET] The SmoothAnim FragmentShader. It performs two lookups
		/// on the same texture and blends the results using an incoming float value.
		/// </summary>
		public static ContentRef<FragmentShader> SmoothAnim	{ get; private set; }
		/// <summary>
		/// [GET] The SharpMask FragmentShader. It enforces an antialiazed sharp mask when upscaling linearly blended textures.
		/// </summary>
		public static ContentRef<FragmentShader> SharpAlpha	{ get; private set; }
		/// <summary>
		/// [GET] The AlphaTest FragmentShader. It performs alpha testing in the shader to discard fragments below a certain alpha threshold.
		/// </summary>
		public static ContentRef<FragmentShader> AlphaTest { get; set; }

		internal static void InitDefaultContent()
		{
			const string ContentPath			= "Data\\Default\\FragmentShader\\";
			const string ContentPath_Minimal	= ContentPath + "Minimal" + FileExt;
			const string ContentPath_Picking	= ContentPath + "Picking" + FileExt;
			const string ContentPath_SmoothAnim = ContentPath + "SmoothAnim" + FileExt;
			const string ContentPath_SharpMask	= ContentPath + "SharpAlpha" + FileExt;
			const string ContentPath_AlphaTest	= ContentPath + "AlphaTest" + FileExt;
			
			Minimal		= ContentProvider.RequestContent<FragmentShader>(ContentPath_Minimal);
			Picking		= ContentProvider.RequestContent<FragmentShader>(ContentPath_Picking);
			SmoothAnim	= ContentProvider.RequestContent<FragmentShader>(ContentPath_SmoothAnim);
			SharpAlpha	= ContentProvider.RequestContent<FragmentShader>(ContentPath_SharpMask);
			AlphaTest	= ContentProvider.RequestContent<FragmentShader>(ContentPath_AlphaTest);
		}

		protected override ShaderType OglShaderType
		{
			get { return ShaderType.FragmentShader; }
		}

		public FragmentShader() {}

		public FragmentShader(string sourceCode) : base(sourceCode) {}
	}
}
