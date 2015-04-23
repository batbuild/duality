using System;
using Duality.Editor;
using Duality.Properties;
using OpenTK.Graphics.OpenGL;

namespace Duality.Resources
{
	[Serializable]
#if ! __ANDROID__
	[EditorHintCategory(typeof(CoreRes), CoreResNames.CategoryGraphics)]
	[EditorHintImage(typeof (CoreRes), CoreResNames.ImageShaderProgram)]
#endif
	public class GeometryShader : AbstractShader
	{
		/// <summary>
		/// A GeometryShader resources file extension.
		/// </summary>
		public new const string FileExt = ".GeometryShader" + Resource.FileExt;

		public GeometryShader()
		{
			
		}

		public GeometryShader(string source) : base (source)
		{
		}

		protected override ShaderType OglShaderType
		{
			get { return ShaderType.GeometryShader; }
		}
	}
}