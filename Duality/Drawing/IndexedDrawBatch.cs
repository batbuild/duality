using System;
using System.Collections.Generic;
using System.Linq;
using Duality.Resources;
using OpenTK.Graphics.OpenGL;

namespace Duality.Drawing
{
	internal class IndexedDrawBatch<T> : IDrawBatch where T : struct, IVertexData
	{
		private static ushort[] indices = {0, 1, 2, 0, 2, 3};
		private static T[] vertexUploadBuffer = null;

		private static ushort[] indexUploadBuffer;
		private static int[] counts;
		private static int[] indexBufferOffsets;
		private static int[] baseVertexOffsets;

		private T[] vertices = null;
		private int vertexCount = 0;
		private int sortIndex = 0;
		private float zSortIndex = 0.0f;
		private VertexMode vertexMode = VertexMode.Quads;
		private BatchInfo material = null;
		private int _vao;
		private uint _indexBuffer;

		public IndexedDrawBatch(BatchInfo material, T[] vertices, int vertexCount, float zSortIndex)
		{
			if (vertices == null || vertices.Length == 0) throw new ArgumentException("A zero-vertex DrawBatch is invalid.");

			GlobalDynamicIndexedVertexBuffer<T>.Initialize(vertices[0]);

			// Assign data
			this.vertexCount = Math.Min(vertexCount, vertices.Length);
			this.vertices = vertices;
			this.material = material;
			this.zSortIndex = zSortIndex;

			// Determine sorting index for non-Z-Sort materials
			if (!this.material.Technique.Res.NeedsZSort)
			{
				int vTypeSI = vertices[0].TypeIndex;
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
		}

		public int SortIndex
		{
			get { return this.sortIndex; }
		}

		public float ZSortIndex
		{
			get { return this.zSortIndex; }
		}

		public int VertexCount
		{
			get { return this.vertexCount; }
		}

		public VertexMode VertexMode
		{
			get { return VertexMode.Triangles; }
		}

		public int VertexTypeIndex
		{
			get { return this.vertices[0].TypeIndex; }
		}

		public BatchInfo Material
		{
			get { return this.material; }
		}

		public void UploadToVBO(List<IDrawBatch> batches)
		{
			GlobalDynamicIndexedVertexBuffer<T>.Bind(this.vertices);

			int vertexCount = 0;
			T[] vertexData = null;
			short[] indexData = null;

			if (batches.Count == 1)
			{
				// Only one batch? Don't bother copying data
				IndexedDrawBatch<T> b = batches[0] as IndexedDrawBatch<T>;
				vertexData = b.vertices;
				vertexCount = b.vertices.Length;
			}
			else
			{
				// Check how many vertices we got
				vertexCount = batches.Sum(t => t.VertexCount);

				// Allocate a static / shared buffer for uploading vertices
				if (vertexUploadBuffer == null)
					vertexUploadBuffer = new T[Math.Max(vertexCount, 64)];
				else if (vertexUploadBuffer.Length < vertexCount)
					Array.Resize(ref vertexUploadBuffer, Math.Max(vertexCount, vertexUploadBuffer.Length * 2));

				// Collect vertex data in one array
				int curVertexPos = 0;
				vertexData = vertexUploadBuffer;
				for (int i = 0; i < batches.Count; i++)
				{
					IndexedDrawBatch<T> b = batches[i] as IndexedDrawBatch<T>;
					Array.Copy(b.vertices, 0, vertexData, curVertexPos, b.vertexCount);
					curVertexPos += b.vertexCount;
				}
			}

			var indexCount = vertexCount / 4 * 6;
			if (indexUploadBuffer == null)
			{
				indexUploadBuffer = new ushort[Math.Max(indexCount, indices.Length * 64)];
				UpdateIndexData(0);
			}
			else if (indexUploadBuffer.Length < indexCount)
			{
				Array.Resize(ref indexUploadBuffer, Math.Max(indexCount, indexUploadBuffer.Length * 2));
				UpdateIndexData(indexUploadBuffer.Length);
			}
			
			// Submit vertex data to GPU
			GlobalDynamicIndexedVertexBuffer<T>.UploadVertexData(vertexData, vertexCount);
		}

		private static void UpdateIndexData(int startIndex)
		{
			for (var i = startIndex; i < indexUploadBuffer.Length / indices.Length; i++)
			{
				var indexBufferOffset = i*6;
				indexUploadBuffer[indexBufferOffset] = (ushort) (indices[0] + i*4);
				indexUploadBuffer[indexBufferOffset + 1] = (ushort) (indices[1] + i*4);
				indexUploadBuffer[indexBufferOffset + 2] = (ushort) (indices[2] + i*4);

				indexUploadBuffer[indexBufferOffset + 3] = (ushort) (indices[3] + i*4);
				indexUploadBuffer[indexBufferOffset + 4] = (ushort) (indices[4] + i*4);
				indexUploadBuffer[indexBufferOffset + 5] = (ushort) (indices[5] + i*4);
			}

			GlobalDynamicIndexedVertexBuffer<T>.UploadIndexData(indexUploadBuffer);
		}

		public void SetupVBO()
		{
			GlobalDynamicIndexedVertexBuffer<T>.Bind(this.vertices);
		}

		public void FinishVBO()
		{
			GlobalDynamicIndexedVertexBuffer<T>.Unbind();
		}

		public void Render(IDrawDevice device, ref int vertexOffset, ref IDrawBatch lastBatchRendered)
		{
			if (lastBatchRendered == null || lastBatchRendered.Material != this.material)
				this.material.SetupForRendering(device, lastBatchRendered == null ? null : lastBatchRendered.Material);

			GL.DrawElementsBaseVertex(PrimitiveType.Triangles, this.VertexCount / 2, DrawElementsType.UnsignedShort, IntPtr.Zero, vertexOffset);

			vertexOffset += this.vertexCount;
			lastBatchRendered = this;
		}

		public void FinishRendering()
		{
			this.material.FinishRendering();
		}

		public bool CanShareVBO(IDrawBatch other)
		{
			return other is IndexedDrawBatch<T>;
		}

		public bool CanAppendJIT<U>(float invZSortAccuracy, float zSortIndex, BatchInfo material, VertexMode vertexMode) where U : struct, IVertexData
		{
			if (invZSortAccuracy > 0.0f && this.material.Technique.Res.NeedsZSort)
			{
				// only batch vertices that are within invZSortAccuracy units of each other. Basically, only vertices in the same plane get batched
				if (Math.Abs(zSortIndex - this.ZSortIndex) > invZSortAccuracy) return false;
			}
			return
				vertexMode == VertexMode.Quads &&
				this is IndexedDrawBatch<U> &&
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
			this.vertexCount += length;

			if (this.material.Technique.Res.NeedsZSort)
				this.zSortIndex = CalcZSortIndex(this.vertices, this.vertexCount);
		}

		public bool CanAppend(IDrawBatch other)
		{
			return
				other.VertexMode == VertexMode.Quads &&
				other is IndexedDrawBatch<T> &&
				other.Material == this.material;
		}

		public void Append(IDrawBatch other)
		{
			this.Append((IndexedDrawBatch<T>)other);
		}

		public void Append(IndexedDrawBatch<T> other)
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

		public float CalcZSortIndex()
		{
			this.zSortIndex = CalcZSortIndex(this.vertices, this.vertexCount);
			return this.zSortIndex;
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
	}
}