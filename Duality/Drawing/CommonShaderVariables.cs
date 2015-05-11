using OpenTK;

namespace Duality.Drawing
{
	public static class CommonShaderVariables
	{
		private static float[] _modelView = new float[16];
		private static float[] _proj = new float[16];

		public static Matrix4 ModelView { get; set; }

		public static Matrix4 Proj { get; set; }

		public static float[] GetProjectionData()
		{
			return GetData(Proj, _proj);
		}
		public static float[] GetModelViewData()
		{
			return GetData(ModelView, _modelView);
		}

		private static float[] GetData(Matrix4 matrix, float[] floats)
		{
			floats[0] = matrix.M11;
			floats[1] = matrix.M21;
			floats[2] = matrix.M31;
			floats[3] = matrix.M41;

			floats[4] = matrix.M12;
			floats[5] = matrix.M22;
			floats[6] = matrix.M32;
			floats[7] = matrix.M42;

			floats[8] = matrix.M13;
			floats[9] = matrix.M23;
			floats[10] = matrix.M33;
			floats[11] = matrix.M43;

			floats[12] = matrix.M14;
			floats[13] = matrix.M24;
			floats[14] = matrix.M34;
			floats[15] = matrix.M44;
			return floats;
		}
	}
}
