using System.Runtime.InteropServices;
using OpenTK;

namespace Duality.Drawing
{
	public static class CommonShaderVariables
	{
		private static float[] _modelViewData = new float[16];
		private static float[] _projData = new float[16];

		private static float[] _cameraPositionData = new float[3];
		private static float[] _cameraZoomData = new float[1];
		private static float[] _applyCameraParallaxData = new float[1];

		private static Matrix4 _modelView;
		private static Matrix4 _proj;
		private static Vector3 _cameraPos;
		private static float _camZoom;
		private static bool _applyCameraParallax;


		public static Vector3 CameraPos
		{
			get { return _cameraPos; }
			set
			{
				_cameraPos = value;
				_cameraPositionData[0] = value.X;
				_cameraPositionData[1] = value.Y;
				_cameraPositionData[2] = value.Z;
			}
		}

		public static float CamZoom
		{
			get { return _camZoom; }
			set
			{
				_camZoom = value;
				_cameraZoomData[0] = value;
			}
		}

		public static bool ApplyCameraParallax
		{
			get { return _applyCameraParallax; }
			set
			{
				_applyCameraParallax = value;
				_applyCameraParallaxData[0] = value ? 1f : 0f;
			}
		}

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

		public static float[] GetCameraPositionData()
		{
			return _cameraPositionData;
		}

		public static float[] GetCameraZoomData()
		{
			return _cameraZoomData;
		}

		public static float[] GetApplyParallaxData()
		{
			return _applyCameraParallaxData;
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
