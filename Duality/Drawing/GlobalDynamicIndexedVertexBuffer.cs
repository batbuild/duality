using System;
using System.Diagnostics;
using Duality.Helpers;
using OpenTK.Graphics.OpenGL;

namespace Duality.Drawing
{
	public static class GlobalDynamicIndexedVertexBuffer<T> where T:struct, IVertexData
	{
		private const int InitialVboSize = 1024;

		private static InitState _initState = InitState.Disposed;

		private static int _vboHandle;
		private static int _iboHandle;
		private static int _vaoHandle;
		private static int _bufferOffset;
		private static int _previousVbo;
		private static int _previousIbo;

		public static void Initialize(T vertexData)
		{
			if (_initState == InitState.Initialized) return;

			_vaoHandle = GL.GenVertexArray();
			_vboHandle = GL.GenBuffer();
			_iboHandle = GL.GenBuffer();

			GL.BindBuffer(BufferTarget.ArrayBuffer, _vboHandle);
			vertexData.UploadToVBO(new T[InitialVboSize], InitialVboSize);
			
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, _iboHandle);
			GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(sizeof(ushort) * InitialVboSize), (IntPtr)null, BufferUsageHint.StreamDraw);

			GL.BindVertexArray(_vaoHandle);
			GL.BindBuffer(BufferTarget.ArrayBuffer, _vboHandle);
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, _iboHandle);
			
			vertexData.SetupVBO(null);

			GL.BindVertexArray(0);
			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

			_initState = InitState.Initialized;
		}

		public static void Bind(T[] vertexData)
		{
			Guard.NotNull(vertexData, "buffer cannot be null");

			EnsureInitialized();

			_previousVbo = GL.GetInteger(GetPName.ArrayBufferBinding);
			_previousIbo = GL.GetInteger(GetPName.ElementArrayBufferBinding);

			GL.BindVertexArray(_vaoHandle);
			GL.BindBuffer(BufferTarget.ArrayBuffer, _vboHandle);
		}
		
		public static void Unbind()
		{
			EnsureInitialized();

			GL.BindVertexArray(0);

			if (_previousVbo != 0)
				GL.BindBuffer(BufferTarget.ArrayBuffer, _previousVbo);

			if(_previousIbo != 0)
				GL.BindBuffer(BufferTarget.ElementArrayBuffer, _previousIbo);
		}

		public static void UploadVertexData(T[] vertexData, int vertexCount)
		{
			EnsureInitialized();

			vertexData[0].UploadToVBO(vertexData, vertexCount);
		}

		public static void UploadIndexData(ushort[] indexData, int indexCount)
		{
			EnsureInitialized();

			GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indexCount * sizeof(short)), (IntPtr)null, BufferUsageHint.StreamDraw);
			GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indexCount * sizeof(short)), indexData, BufferUsageHint.StreamDraw);
		}

		private static void EnsureInitialized()
		{
			Debug.Assert(_initState == InitState.Initialized, "GlobalDynamicIndexedVertexBuffer not initialized");
		}

		public static void Delete()
		{
			if(_iboHandle != 0)
				GL.DeleteBuffer(_iboHandle);

			if (_vboHandle != 0)
				GL.DeleteBuffer(_vboHandle);

			if (_vaoHandle != 0)
				GL.DeleteBuffer(_vaoHandle);
		}
	}
}