using OpenTK;

namespace Duality.Drawing
{
	public static class CommonShaderVariables
	{
		private static float[] _modelViewData = new float[16];
		private static float[] _projData = new float[16];
		private static Matrix4 _modelView;
		private static Matrix4 _proj;


		public static Vector3 CameraPos { get; set; }
		public static float CamZoom { get; set; }
		public static bool ApplyCameraParallax { get; set; }

		public static Matrix4 ModelView
		{
			get { return _modelView; }
			set
			{
				_modelView = value;
				MatrixToArray(ref _modelView, _modelViewData);
			}
		}

		public static Matrix4 Proj
		{
			get { return _proj; }
			set
			{
				_proj = value;
				MatrixToArray(ref _proj, _projData);
			}
		}

		public static float[] GetProjectionData()
		{
			return _projData;
		}

		public static float[] GetModelViewData()
		{
			return _modelViewData;
		}

		private static void MatrixToArray(ref Matrix4 matrix, float[] floats)
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
		}
	}
}
