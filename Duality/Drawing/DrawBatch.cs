using System;
using System.Collections.Generic;
using System.Linq;
using Duality.Resources;
using OpenTK.Graphics.OpenGL;

namespace Duality.Drawing
{
	internal class DrawBatch<T> : IDrawBatch where T : struct, IVertexData
	{
		private static T[] uploadBuffer = null;

		private	T[]			vertices	= null;
		private	int			vertexCount	= 0;
		private	int			sortIndex	= 0;
		private	float		zSortIndex	= 0.0f;
		private	VertexMode	vertexMode	= VertexMode.Points;
		private	BatchInfo	material	= null;
		private int _vao;

		public int SortIndex
		{
			get { return this.sortIndex; }
		}
		public float ZSortIndex
		{
			get { return this.zSortIndex; }
		}

		public int ListSortIndex { get; set; }

		public int VertexCount
		{
			get { return this.vertexCount; }
		}
		public VertexMode VertexMode
		{
			get { return this.vertexMode; }
		}
		public int VertexTypeIndex
		{
			get { return this.vertices[0].TypeIndex; }
		}

		public bool Pooled { get; set; }

		public BatchInfo Material
		{
			get { return this.material; }
		}

		public DrawBatch(BatchInfo material, VertexMode vertexMode, T[] vertices, int vertexCount, float zSortIndex)
		{
			// Assign data
			this.material = material;
			this.vertexMode = vertexMode;
			this.zSortIndex = zSortIndex;

			// Determine sorting index for non-Z-Sort materials
			if (!this.material.Technique.Res.NeedsZSort)
			{
				int vTypeSI = default(T).TypeIndex;
				int matHash = this.material.GetHashCode() % (1 << 23);

				// Bit significancy is used to achieve sorting by multiple traits at once.
				// The higher a traits bit significancy, the higher its priority when sorting.
				this.sortIndex = 
					(((int)vertexMode & 15) << 0) |		//							  XXXX	4 Bit	Vertex Mode		Offset 4
					((matHash & 8388607) << 4) |		//	   XXXXXXXXXXXXXXXXXXXXXXXaaaa	23 Bit	Material		Offset 27
					((vTypeSI & 15) << 27);				//	XXXbbbbbbbbbbbbbbbbbbbbbbbaaaa	4 Bit	Vertex Type		Offset 31

				// Keep an eye on this. If for example two material hash codes randomly have the same 23 lower bits, they
				// will be sorted as if equal, resulting in blocking batch aggregation.
			}

			_vao = GL.GenVertexArray();
			GL.BindVertexArray(_vao);
		}

		public void SetupVBO()
		{
			// Set up VBO
			GL.BindVertexArray(_vao);

			this.vertices[0].SetupVBO(this.material);
		}
		public void UploadToVBO(List<IDrawBatch> batches)
		{
			Profile.TimeUploadVertexData.BeginMeasure();

			int vertexCount = 0;
			T[] vertexData = null;

			if (batches.Count == 1)
			{
				// Only one batch? Don't bother copying data
				DrawBatch<T> b = batches[0] as DrawBatch<T>;
				vertexData = b.vertices;
				vertexCount = b.vertices.Length;
			}
			else
			{
				// Check how many vertices we got
				vertexCount = batches.Sum(t => t.VertexCount);
					
				// Allocate a static / shared buffer for uploading vertices
				if (uploadBuffer == null)
					uploadBuffer = new T[Math.Max(vertexCount, 64)];
				else if (uploadBuffer.Length < vertexCount)
					Array.Resize(ref uploadBuffer, Math.Max(vertexCount, uploadBuffer.Length * 2));

				// Collect vertex data in one array
				int curVertexPos = 0;
				vertexData = uploadBuffer;
				for (int i = 0; i < batches.Count; i++)
				{
					DrawBatch<T> b = batches[i] as DrawBatch<T>;
					Array.Copy(b.vertices, 0, vertexData, curVertexPos, b.vertexCount);
					curVertexPos += b.vertexCount;
				}
			}

			// Submit vertex data to GPU
			this.vertices[0].UploadToVBO(vertexData, vertexCount);

			Profile.TimeUploadVertexData.EndMeasure();
		}
		public void FinishVBO()
		{
			// Finish VBO
			this.vertices[0].FinishVBO(this.material);
			GL.BindVertexArray(0);
		}
		public void Render(IDrawDevice device, ref int vertexOffset, ref IDrawBatch lastBatchRendered)
		{
			Profile.TimeDrawArrays.BeginMeasure();
			
			if (lastBatchRendered == null || lastBatchRendered.Material != this.material)
				this.material.SetupForRendering(device, lastBatchRendered == null ? null : lastBatchRendered.Material);

			var primitiveType = (PrimitiveType)this.vertexMode;
			if(primitiveType == PrimitiveType.Quads)
				primitiveType = PrimitiveType.Triangles;
			
			GL.DrawArrays(primitiveType, vertexOffset, this.vertexCount);

			vertexOffset += this.vertexCount;
			lastBatchRendered = this;

			Profile.TimeDrawArrays.EndMeasure();
		}
		public void FinishRendering()
		{
			this.material.FinishRendering();
		}

		public bool CanShareVBO(IDrawBatch other)
		{
			return other is DrawBatch<T>;
		}
		public bool CanAppendJIT<U>(float invZSortAccuracy, float zSortIndex, BatchInfo material, VertexMode vertexMode) where U : struct, IVertexData
		{
			if (invZSortAccuracy > 0.0f && this.material.Technique.Res.NeedsZSort)
			{
				if (Math.Abs(zSortIndex - this.ZSortIndex) > invZSortAccuracy) return false;
			}
			return 
				vertexMode == this.vertexMode && 
				this is DrawBatch<U> &&
				IsVertexModeAppendable(this.VertexMode) &&
				material == this.material;
		}
		public void AppendJIT(object vertexData, int length)
		{
			this.AppendJIT((T[])vertexData, length);
		}
		public void AppendJIT(T[] data, int length)
		{
			if (this.vertexCount + length > this.vertices.Length)
			{
				int newArrSize = MathF.Max(16, this.vertexCount * 2, this.vertexCount + length);
				Array.Resize(ref this.vertices, newArrSize);
			}
			Array.Copy(data, 0, this.vertices, this.vertexCount, length);

			if (this.material.Technique.Res.NeedsZSort)
				this.zSortIndex = (SumZPositions(data) + this.VertexCount * zSortIndex) / (this.VertexCount + length);

			this.vertexCount += length;
		}

		public bool CanAppend(IDrawBatch other)
		{
			return
				other.VertexMode == this.vertexMode && 
				other is DrawBatch<T> &&
				IsVertexModeAppendable(this.VertexMode) &&
				other.Material == this.material;
		}
		public void Append(IDrawBatch other)
		{
			this.Append((DrawBatch<T>)other);
		}

		public float CalcZSortIndex()
		{
			this.zSortIndex = CalcZSortIndex(this.vertices, this.vertexCount);
			return this.zSortIndex;
		}

		public void SetVertices(T[] vertices, int vertexCount)
		{
			if (this.vertexMode == VertexMode.Quads)
			{
				// we don't support quads anymore, so just convert the verts into triangle data
				// for every quad primitive, add two extra verts
				var numVerts = vertices.Length + (vertices.Length / 4) * 2;
				if (this.vertices == null || this.vertices.Length < numVerts)
					this.vertices = new T[numVerts];

				for (var i = 0; i < vertexCount; i += 4)
				{
					var triIndex = i / 4 * 6;
					this.vertices[triIndex] = vertices[i];
					this.vertices[triIndex + 1] = vertices[i + 1];
					this.vertices[triIndex + 2] = vertices[i + 2];

					this.vertices[triIndex + 3] = vertices[i];
					this.vertices[triIndex + 4] = vertices[i + 2];
					this.vertices[triIndex + 5] = vertices[i + 3];
				}

				this.vertexCount = Math.Min(vertexCount + (vertexCount / 4) * 2, this.vertices.Length);
			}
			else
			{
				if (this.vertices == null || this.vertices.Length < vertices.Length)
					this.vertices = new T[vertices.Length];

				Array.Copy(vertices, this.vertices, vertices.Length);

				this.vertexCount = Math.Min(vertexCount, this.vertices.Length);
			}
		}

		public void Append(DrawBatch<T> other)
		{
			if (this.vertexCount + other.vertexCount > this.vertices.Length)
			{
				int newArrSize = MathF.Max(16, this.vertexCount * 2, this.vertexCount + other.vertexCount);
				Array.Resize(ref this.vertices, newArrSize);
			}
			Array.Copy(other.vertices, 0, this.vertices, this.vertexCount, other.vertexCount);
			this.vertexCount += other.vertexCount;

			if (this.material.Technique.Res.NeedsZSort)
				CalcZSortIndex();
		}

		public static bool IsVertexModeAppendable(VertexMode mode)
		{
			return 
				mode == VertexMode.Lines || 
				mode == VertexMode.Points || 
				mode == VertexMode.Quads || 
				mode == VertexMode.Triangles;
		}
		public static float CalcZSortIndex(T[] vertices, int count = -1)
		{
			if (count < 0) count = vertices.Length;
			float zSortIndex = 0.0f;
			for (int i = 0; i < count; i++)
			{
				zSortIndex += vertices[i].Pos.Z;
			}
			return zSortIndex / count;
		}

		private static float SumZPositions(T[] data)
		{
			float sum = 0;
			foreach (var vertex in data)
			{
				sum += vertex.Pos.Z;
			}
			return sum;
		}
	}
}