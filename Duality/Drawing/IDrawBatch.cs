using System.Collections.Generic;
using Duality.Resources;

namespace Duality.Drawing
{
	public interface IDrawBatch
	{
		int SortIndex { get; }
		float ZSortIndex { get; }
		int VertexCount { get; }
		VertexMode VertexMode { get; }
		BatchInfo Material { get; }
		int VertexTypeIndex { get; }
		bool Pooled { get; set; }

		void UploadToVBO(List<IDrawBatch> batches);
		void SetupVBO();
		void FinishVBO();
		void Render(IDrawDevice device, ref int vertexOffset, ref IDrawBatch lastBatchRendered);
		void FinishRendering();

		bool CanShareVBO(IDrawBatch other);
		bool CanAppendJIT<T>(float invZSortAccuracy, float zSortIndex, BatchInfo material, VertexMode vertexMode) where T : struct, IVertexData;
		void AppendJIT(object vertexData, int length);
		bool CanAppend(IDrawBatch other);
		void Append(IDrawBatch other);
		float CalcZSortIndex();
	}
}