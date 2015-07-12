using System;
using System.Runtime.InteropServices;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Duality.Drawing
{
	/// <summary>
	/// Vertex data providing each vertex a position (3x4 byte) and color (1x4 byte).
	/// </summary>
	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public struct VertexC1P3 : IVertexData
	{
		/// <summary>
		/// The vertices color.
		/// </summary>
		public ColorRgba Color;
		/// <summary>
		/// The vertices position.
		/// </summary>
		public Vector3 Pos;

		Vector3 IVertexData.Pos
		{
			get { return this.Pos; }
			set { this.Pos = value; }
		}
		ColorRgba IVertexData.Color
		{
			get { return this.Color; }
			set { this.Color = value; }
		}
		int IVertexData.TypeIndex
		{
			get { return VertexTypeIndex; }
		}
		
		void IVertexData.SetupVBO(Resources.BatchInfo mat)
		{
			GL.VertexAttribPointer(0, 4, VertexAttribPointerType.UnsignedByte, false, Size, 0);		// colour
			GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, Size, 4);			// pos

			GL.EnableVertexAttribArray(0);
			GL.EnableVertexAttribArray(1);
		}
		void IVertexData.UploadToVBO<T>(T[] vertexData, int vertexCount)
		{
			GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(Size * vertexCount), IntPtr.Zero, BufferUsageHint.StreamDraw);
			GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(Size * vertexCount), vertexData, BufferUsageHint.StreamDraw);
		}
		void IVertexData.FinishVBO(Resources.BatchInfo mat)
		{
		}

		/// <summary>
		/// Byte offset for the color.
		/// </summary>
		public const int OffsetColor		= 0;
		/// <summary>
		/// Byte offset for the position.
		/// </summary>
		public const int OffsetPos			= OffsetColor + 4 * sizeof(byte);
		/// <summary>
		/// Total size in bytes.
		/// </summary>
		public const int Size				= OffsetPos + 3 * sizeof(float);
		public const int VertexTypeIndex	= Duality.Resources.DrawTechnique.VertexType_C1P3;

		public VertexC1P3(float x, float y, float z, byte r = 255, byte g = 255, byte b = 255, byte a = 255)
		{
			this.Pos.X = x;
			this.Pos.Y = y;
			this.Pos.Z = z;
			this.Color.R = r;
			this.Color.G = g;
			this.Color.B = b;
			this.Color.A = a;
		}
		public VertexC1P3(float x, float y, float z, ColorRgba clr)
		{
			this.Pos.X = x;
			this.Pos.Y = y;
			this.Pos.Z = z;
			this.Color = clr;
		}
		public VertexC1P3(Vector3 pos, byte r = 255, byte g = 255, byte b = 255, byte a = 255)
		{
			this.Pos = pos;
			this.Color.R = r;
			this.Color.G = g;
			this.Color.B = b;
			this.Color.A = a;
		}
		public VertexC1P3(Vector3 pos, ColorRgba clr)
		{
			this.Pos = pos;
			this.Color = clr;
		}
	}
}
