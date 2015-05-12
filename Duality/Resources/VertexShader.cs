using System;

using Duality.Properties;
using Duality.Editor;

using OpenTK.Graphics.OpenGL;


namespace Duality.Resources
{
	/// <summary>
	/// Represents an OpenGL VertexShader.
	/// </summary>
	[Serializable]
#if ! __ANDROID__
[EditorHintCategory(typeof(CoreRes), CoreResNames.CategoryGraphics)]
	[EditorHintImage(typeof(CoreRes), CoreResNames.ImageVertexShader)]
#endif

	public class VertexShader : AbstractShader
	{
		/// <summary>
		/// A VertexShader resources file extension.
		/// </summary>
		public new const string FileExt = ".VertexShader" + Resource.FileExt;

		private const string MinimalVertShader = @"#version 440

layout(location = 0) in vec4 colour;
layout(location = 1) in vec3 position;
layout(location = 2) in vec2 texCoord;

out vec2 iTexCoord;
out vec4 oColour;

uniform mat4 matProj;

void main()
{
	gl_Position = vec4(position, 1) * matProj;
	iTexCoord = texCoord;
	oColour = colour;
}";

		/// <summary>
		/// [GET] A minimal VertexShader. It performs OpenGLs default transformation
		/// and forwards a single texture coordinate and color to the fragment stage.
		/// </summary>
		public static ContentRef<VertexShader> Minimal		{ get; private set; }
		/// <summary>
		/// [GET] The SmoothAnim VertexShader. In addition to the <see cref="Minimal"/>
		/// setup, it forwards the custom animBlend vertex attribute to the fragment stage.
		/// </summary>
		public static ContentRef<VertexShader> SmoothAnim	{ get; private set; }

		internal static void InitDefaultContent()
		{
			const string VirtualContentPath			= ContentProvider.VirtualContentPath + "VertexShader:";
			const string ContentPath_Minimal		= VirtualContentPath + "Minimal";
			const string ContentPath_SmoothAnim		= VirtualContentPath + "SmoothAnim";

ContentProvider.AddContent(ContentPath_Minimal, new VertexShader(MinimalVertShader));
//			ContentProvider.AddContent(ContentPath_SmoothAnim, new VertexShader(DefaultContent.SmoothAnimVert));


			Minimal		= ContentProvider.RequestContent<VertexShader>(ContentPath_Minimal);
//			SmoothAnim	= ContentProvider.RequestContent<VertexShader>(ContentPath_SmoothAnim);
		}


		protected override ShaderType OglShaderType
		{
			get { return ShaderType.VertexShader; }
		}

		public VertexShader() : base(MinimalVertShader) {}

		public VertexShader(string sourceCode) : base(sourceCode) {}
	}
}
